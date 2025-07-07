namespace DiscountManager.Domain.Entities
{
    public class GenerateResponse
    {
        public bool Result { get; set; }
        public IEnumerable<string> Codes { get; set; }
    }
}
