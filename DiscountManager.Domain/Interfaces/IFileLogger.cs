using System.Runtime.CompilerServices;

namespace DiscountManager.Domain.Interfaces
{
    public interface IFileLogger
    {
        bool Info(string message,
                  [CallerMemberName] string caller = "");

        bool Warn(string message,
                  [CallerMemberName] string caller = "");

        bool Error(string message,
                   Exception? ex = null,
                   [CallerMemberName] string caller = "");
    }
}
