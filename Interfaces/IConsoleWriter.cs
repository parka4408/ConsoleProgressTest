namespace Interfaces
{
    public interface IConsoleWriter
    {
        void Write(string message);
        void WriteLine(string message = "");
    }
}