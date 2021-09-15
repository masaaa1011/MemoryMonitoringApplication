using MemoryMonitoringDomein.ProcessMonitor.Options;
using MemoryMonitoringDomein.ProcessMonitor.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MemoryMonitoringDomein.ProcessMonitor
{
    /// <summary>
    /// execution monitoring. and create monitorning results.
    /// </summary>
    public interface IMonitor
    {
        IMonitoringResult Result();
    }
    /// <summary>
    /// execution monitoring. and create monitorning results.
    /// </summary>
    internal class ProcessMonitor : IMonitor
    {
        private Process m_process;
        private IMonitoringOptions m_option;
        internal ProcessMonitor(Process process, IMonitoringOptions option)
        {
            m_process = process;
            m_option = option;
        }
        /// <summary>
        /// execute monitoring and return result IMonitoringResult.
        /// </summary>
        /// <returns></returns>
        public IMonitoringResult Result()
            => new ProcessMonitoringResult(m_option, m_process.PagedMemorySize64, m_process.PagedSystemMemorySize64, m_process.Id, m_process.MainModule.FileName);
    }
}
