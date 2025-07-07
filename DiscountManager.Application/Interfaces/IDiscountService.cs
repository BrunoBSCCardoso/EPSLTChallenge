namespace DiscountManager.Application.Interfaces
{
    public interface IDiscountService
    {
        IEnumerable<string> GenerateCodes(int count, int codeLength);
        bool UseCode(string code);
    }
}
