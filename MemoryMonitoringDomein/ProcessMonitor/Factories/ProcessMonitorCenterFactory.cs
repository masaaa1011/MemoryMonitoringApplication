using MemoryMonitoringDomein.ProcessMonitor;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryMonitoringDomein.ProcessMonitor.Factories
{
    public interface IMonitorCenterFactory
    {
        IMonitoringCenter Create();
    }
    /// <summary>
    /// recieve request monitorring.
    /// </summary>
    public class ProcessMonitorCenterFactory : IMonitorCenterFactory
    {
        private IMonitorFactory m_factory;
        public ProcessMonitorCenterFactory(IMonitorFactory factory)
        {
            m_factory = factory;
        }
        public IMonitoringCenter Create()
            => new ProcessMonitorCenter(m_factory);
    }
}
