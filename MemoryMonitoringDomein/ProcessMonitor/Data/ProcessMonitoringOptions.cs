using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryMonitoringDomein.ProcessMonitor.Options
{
    /// <summary>
    /// interface: monitoring setting object
    /// </summary>
    public interface IMonitoringOptions
    {
        string ApplicationName { get; }
        long ThresholdUnitOfMB { get; }
    }
    /// <summary>
    /// monitoring setting object
    /// </summary>
    public struct ProcessMonitoringOptions : IMonitoringOptions
    {
        public string ApplicationName {  get; set; }
        public long ThresholdUnitOfMB { get; set; }
    }
}
