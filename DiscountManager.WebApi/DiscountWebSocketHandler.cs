using DiscountManager.Application.Interfaces;
using DiscountManager.Domain.Entities;
using DiscountManager.Domain.Interfaces;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DiscountManager.WebApi;

public class DiscountWebSocketHandler
{
    private const int BufferSize = 4 * 1024;

    private readonly IDiscountService _service;
    private readonly IFileLogger _log;

    public DiscountWebSocketHandler(IDiscountService service, IFileLogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessAsync(WebSocket ws, CancellationToken ct = default)
    {
        var connId = Guid.NewGuid().ToString("N");
        _log.Info($"[{connId}] WebSocket connected.");

        var buffer = new byte[BufferSize];

        try
        {
            while (!ct.IsCancellationRequested &&
                   ws.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                WebSocketReceiveResult result = await ws.ReceiveAsync(buffer, ct);
                
                if (result.CloseStatus.HasValue) break;

                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                _log.Info($"[{connId}] Received {json}");

                if (!TryDeserialize(json, out MessageEnvelope envelope, connId)) continue;

                switch (envelope.Action)
                {
                    case "GenerateCodes":
                        await HandleGenerateAsync(ws, envelope, connId, ct);
                        break;

                    case "UseCode":
                        await HandleUseCodeAsync(ws, envelope, connId, ct);
                        break;

                    default:
                        _log.Warn($"[{connId}] Unknown action '{envelope.Action}'.");
                        await SendErrorAsync(ws, "Unknown action.", ct);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _log.Error($"[{connId}] Unhandled error", ex);
            await SendErrorAsync(ws, ex.Message, ct);
            await SafeCloseAsync(ws, WebSocketCloseStatus.InternalServerError, ct);
        }
        finally
        {
            await SafeCloseAsync(ws, WebSocketCloseStatus.NormalClosure, ct);
            _log.Info($"[{connId}] Connection closed.");
        }
    }

    #region --- Handlers ---

    private async Task HandleGenerateAsync(
        WebSocket ws,
        MessageEnvelope env,
        string connId,
        CancellationToken ct)
    { 
        GenerateRequest? req = env.Payload.Deserialize<GenerateRequest>()!;

        if(req == null)
        {
            _log.Warn($"[{connId}] Deserialization failed of Generate Request object");
            throw new Exception("Deserialization failed of Generate Request object");
        }

        var codes = _service.GenerateCodes(req.Count, req.Length);

        var resp = new GenerateResponse
        {
            Result = true,
            Codes = codes
        };

        await SendAsync(ws, "GenerateResponse", resp, ct);
        _log.Info($"[{connId}] Generated {resp.Codes.Count()} codes.");
    }

    private async Task HandleUseCodeAsync(
        WebSocket ws,
        MessageEnvelope env,
        string connId,
        CancellationToken ct)
    {
        UseCodeRequest? req = env.Payload.Deserialize<UseCodeRequest>();

        if (req is null || string.IsNullOrWhiteSpace(req.Code))
        {
            _log.Warn($"[{connId}] Invalid UseCode payload.");

            await SendErrorAsync(ws, "Invalid parameters.", ct);

            return;
        }

        bool success = _service.UseCode(req.Code);

        var resp = new UseCodeResponse { Result = success ? (byte)1 : (byte)0 };

        await SendAsync(ws, "UseCode", resp , ct);

        _log.Info($"[{connId}] UseCode '{req.Code}' => {(success ? "Success" : "Fail")}");
    }

    #endregion

    #region --- Helpers ---

    private static async Task SendAsync(
        WebSocket ws,
        string action,
        object payload,
        CancellationToken ct)
    {
        var envelope = new MessageEnvelope
        {
            Action = action,
            Payload = JsonDocument.Parse(JsonSerializer.Serialize(payload))
        };

        var json = JsonSerializer.Serialize(envelope);
        var bytes = Encoding.UTF8.GetBytes(json);

        await ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    private static async Task SendErrorAsync(WebSocket ws, string msg, CancellationToken ct)
        => await SendAsync(ws, "Error", new { Message = msg }, ct);

    private static async Task SafeCloseAsync(
        WebSocket ws,
        WebSocketCloseStatus status,
        CancellationToken ct)
    {
        if (ws.State is WebSocketState.Open or WebSocketState.CloseReceived)
            await ws.CloseAsync(status, "Connection closed", ct);
    }

    private bool TryDeserialize(string json, out MessageEnvelope env, string connId)
    {
        env = default!;
        try
        {
            env = JsonSerializer.Deserialize<MessageEnvelope>(json)!;
            
            if (env is null) 
                throw new Exception("Null envelope.");

            return true;
        }
        catch (Exception ex)
        {
            _log.Warn($"[{connId}] Deserialization failed: {ex.Message}");
            return false;
        }
    }


    #endregion
}
