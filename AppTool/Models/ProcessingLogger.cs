using Interfaces;
using System;
using System.Diagnostics;

namespace AppTool.Models;

public class ProcessingLogger : ILogger
{
    public void Error(string message, Exception? exception = null, bool clearLine = true)
    {
        Debug.WriteLine("[Error] " + message);
    }

    public void Warning(string message, bool clearLine = true)
    {
        Debug.WriteLine("[Warning] " + message);
    }
}
