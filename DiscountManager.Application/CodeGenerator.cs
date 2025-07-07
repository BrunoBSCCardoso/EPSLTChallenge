using System.Text;

namespace DiscountManager.Application
{
    public static class CodeGenerator
    {
        public static string GenerateGuidCode(int length)
        {
            var guidBytes = Guid.NewGuid().ToByteArray();

            var slice = guidBytes.Take(6).ToArray(); 
                                                     
            ulong num = BitConverter.ToUInt64(slice.Concat(new byte[2]).ToArray(), 0);

            string code = Base36Encode(num);

            if (code.Length < length)
                code = code.PadLeft(length, '0');
            else if (code.Length > length)
                code = code.Substring(0, length);
            return code.ToUpperInvariant();
        }

        private static string Base36Encode(ulong value)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var sb = new StringBuilder();
            do
            {
                sb.Insert(0, chars[(int)(value % 36)]);
                value /= 36;
            } while (value > 0);
            return sb.ToString();
        }

    }
}
