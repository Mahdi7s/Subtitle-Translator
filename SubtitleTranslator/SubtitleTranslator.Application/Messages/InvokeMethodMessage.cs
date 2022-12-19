using System.Reflection;

namespace SubtitleTranslator.Application.Messages
{
    public class InvokeMethodMessage<T>
    {
        public string MethodName { get; set; }
        public object[] Arguments { get; set; }
        public object Result { get; set; }
    }

    public static class InvokeMethodMessageExtensions
    {
        public static void Invoke<T>(this InvokeMethodMessage<T> message, T obj)
        {
            message.Result = obj.GetType().InvokeMember(message.MethodName, BindingFlags.InvokeMethod, null, obj, message.Arguments);
        }
    }
}