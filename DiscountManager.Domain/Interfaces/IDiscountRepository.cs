namespace DiscountManager.Domain.Interfaces
{
    public interface IDiscountRepository
    {
        IEnumerable<string> GetAll();
        void SaveAll(IEnumerable<string> codes);
    }
}
