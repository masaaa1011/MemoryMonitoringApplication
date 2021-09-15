using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryMonitoringApplication.Logger
{
    public interface ILogger
    {
        void WriteLog(string message);
        void WriteLog(string message, Exception e);
    }
    public class Log4NetLogger : ILogger
    {
        private object _lock = new object();
        private ILog m_logger = LogManager.GetLogger("MemoryMonitoringApplicationLogger");
        public void WriteLog(string message)
        {
            lock (_lock)
            {
                m_logger.Info(message);
            }
        }
        public void WriteLog(string message, Exception e)
        {
            lock (_lock)
            {
                m_logger.Error(message, e);
            }
        }
    }
}
