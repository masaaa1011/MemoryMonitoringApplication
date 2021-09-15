using MemoryMonitoringDomein.ProcessMonitor.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MemoryMonitoringDomein.ProcessMonitor.Factories
{
    /// <summary>
    /// create process monitor objects.
    /// </summary>
    public interface IMonitorFactory
    {
        IMonitor Create(Process p, IMonitoringOptions o);
    }
    public class ProcessMonitorFactory : IMonitorFactory
    {
        public IMonitor Create(Process p, IMonitoringOptions o)
            => new  ProcessMonitor(p, o);
    }
}
