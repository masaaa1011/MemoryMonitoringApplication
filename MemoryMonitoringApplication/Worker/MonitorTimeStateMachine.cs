using MemoryMonitoringApplication.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace MemoryMonitoringApplication.Worker
{
    /// <summary>
    /// state machine object.
    /// will manage execution monitoring.
    /// </summary>
    public interface IMonitorTimeStateMachine
    {
        bool IsMonitoringAllowed();
        MonitorWaitingState Now();
        MonitorWaitingState Failed();
    }
    public class MonitorTimeStateMachine : IMonitorTimeStateMachine
    {
        private ILogger m_logger;
        private MonitorWaitingState m_state = MonitorWaitingState.WILL_START;
        private DateTime? m_invokeTime;
        private DateTime Nowhhmm => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
        public MonitorTimeStateMachine(ILogger logger, TimeSpan invokeTime)
        {
            m_logger = logger;
            m_invokeTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0).Add(invokeTime);
            if (m_invokeTime < Nowhhmm)
                m_state = MonitorWaitingState.COMPLETED;
        }
        public bool IsMonitoringAllowed()
        {
            if (Nowhhmm.Date.AddDays(1) <= m_invokeTime.Value.Date) return false;
            if (m_state != MonitorWaitingState.WILL_START && Nowhhmm <= m_invokeTime.Value) m_state = MonitorWaitingState.WILL_START;
            switch (m_state)
            {
                case MonitorWaitingState.WILL_START:
                    if(m_invokeTime <= Nowhhmm)
                    {
                        m_state = MonitorWaitingState.COMPLETED;
                        m_invokeTime = m_invokeTime.Value.AddDays(1);
                        m_logger.WriteLog($"次回開始時間: {m_invokeTime.Value.ToString("yyyy/MM:dd HH:mm")}");
                        return true;
                    }
                    break;
                case MonitorWaitingState.COMPLETED:
                    break;
                case MonitorWaitingState.FAILED:
                    m_logger.WriteLog($"前回の監視に失敗したため再度監視を開始します。");
                    m_state = MonitorWaitingState.WILL_START;
                    break;
            }
            return false;
        }
        public MonitorWaitingState Now()
            => m_state;
        public MonitorWaitingState Failed()
        {
            m_state = MonitorWaitingState.FAILED;
            return m_state;
        }
    }
    public enum MonitorWaitingState
    {
        WILL_START = 1,
        COMPLETED = 2,
        FAILED = 3,
    }
}
