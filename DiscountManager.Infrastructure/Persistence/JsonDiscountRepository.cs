using DiscountManager.Domain.Interfaces;
using System.Text.Json;

namespace DiscountManager.Infrastructure.Persistence
{
    public class JsonDiscountRepository : IDiscountRepository
    {
        private readonly IFileLogger _log;
        private readonly string _file;
        public JsonDiscountRepository(string file, IFileLogger log)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public IEnumerable<string> GetAll()
        {
            try
            {
                if (!File.Exists(_file))
                {
                    _log.Warn($"Discount file does not exist in '{_file}'. Returning empty.");
                    return Enumerable.Empty<string>();
                }

                string json = File.ReadAllText(_file);
                var codes = JsonSerializer.Deserialize<HashSet<string>>(json) ?? Enumerable.Empty<string>();
                _log.Info($"Read {codes.Count()} discount codes from '{_file}'.");

                return codes;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to read discount codes from '{_file}'.", ex);
                return Enumerable.Empty<string>();
            }
           
        }

        public void SaveAll(IEnumerable<string> codes)
        {
            try
            {
                var json = JsonSerializer.Serialize(codes);
                File.WriteAllText(_file, json);
                _log.Info($"Saved {codes.Count()} discount codes in '{_file}'.");
            }
            catch (Exception ex)
            {
                _log.Error($"Error saving discounts to '{_file}'.", ex);
            }
        }
    }
}
