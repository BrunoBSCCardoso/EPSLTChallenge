using DiscountManager.Application.Interfaces;
using DiscountManager.Domain.Interfaces;

namespace DiscountManager.Application.Services
{
    public class DiscountCodeService : IDiscountService
    {
        private readonly IDiscountRepository _repository;
        private readonly object _lock = new();
        private readonly HashSet<string> _codes;

        public DiscountCodeService(IDiscountRepository repository)
        {
            _repository = repository;
            _codes = _repository.GetAll().ToHashSet();
        }

        public IEnumerable<string> GenerateCodes(int numberOfCodesRequested, int codeLength)
        {
            DiscountCodeValidator.ValidateGenerationParams(numberOfCodesRequested, codeLength);

            List<string> codes = new List<string>();
            var tempCodes = new HashSet<string>();

            while (tempCodes.Count < numberOfCodesRequested)
            {
                string code = CodeGenerator.GenerateGuidCode(codeLength);
                tempCodes.Add(code);
            }
            
            lock (_lock)
            {
                foreach (var code in tempCodes)
                {
                    if (_codes.Add(code))
                        codes.Add(code);
                }
                _repository.SaveAll(_codes);
            }

            return codes;
        }

        public bool UseCode(string code)
        {
            lock (_lock)
            {
                if (_codes.Remove(code))
                {
                    _repository.SaveAll(_codes);
                    return true;
                }
                return false;
            }
        }
    }
}
