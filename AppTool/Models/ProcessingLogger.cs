using Interfaces;
using System;
using System.Diagnostics;

namespace AppTool.Models;

public class ProcessingLogger : ILogger
{
    public void Info(string message)
    {
        Debug.WriteLine("[Info] " + message);
    }

    public void Warning(string message)
    {
        Debug.WriteLine("[Warning] " + message);
    }

    public void Error(string message, Exception? exception = null)
    {
        Debug.WriteLine("[Error] " + message);
    }
}
