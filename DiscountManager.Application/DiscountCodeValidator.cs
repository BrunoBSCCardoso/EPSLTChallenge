namespace DiscountManager.Application
{
    public static class DiscountCodeValidator
    {
        public static void ValidateGenerationParams(int count, int length)
        {
            if (length < 7 || length > 8)
                throw new ArgumentException("Code length must be 7 or 8.", nameof(length));
            if (count < 1 || count > 2000)
                throw new ArgumentException("Count must be between 1 and 2000.", nameof(count));
        }
    }    
}
