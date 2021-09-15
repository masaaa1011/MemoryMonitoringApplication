using System;

namespace Logger
{
    public interface ILogger
    {
        void WriteLog();
    }
    public class EventLogger : ILogger
    {

    }
}
