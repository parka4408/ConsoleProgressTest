using System;

namespace Interfaces
{
    public interface ILogger
    {
        void Error(string message, Exception exception = null, bool clearLine = true);
        void Warning(string message, bool clearLine = true);
    }
}
