using System;
using System.IO;

namespace HarmonyMixinBootstrap;

public static class LoggerUtility
{
    private static void Log(TextWriter writer, object message)
    {
        writer.WriteLine($"[HarmonyMixin] {message}");
    }
    
    public static void Info(object message)
    {
        Log(Console.Out, message);
    }
    
    public static void Error(object message)
    {
        Log(Console.Error, message);
    }
}