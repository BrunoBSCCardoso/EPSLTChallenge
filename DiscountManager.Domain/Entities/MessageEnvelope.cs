using System.Text.Json;

namespace DiscountManager.Domain.Entities
{
    public class MessageEnvelope
    {
        public string Action { get; set; }
        public JsonDocument Payload { get; set; }
    }
}
