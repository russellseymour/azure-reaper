

namespace Azure.Reaper.Lib.Interfaces
{
    public interface ILog
    {

        void Verbose(string message, params object[] propertyValues);
        void Debug(string message, params object[] propertyValues);
        void Information(string message, params object[] propertyValues);
        void Warning(string message, params object[] propertyValues);
        void Error(string message, params object[] propertyValues);
        void Fatal(string message, params object[] propertyValues);

    }
}