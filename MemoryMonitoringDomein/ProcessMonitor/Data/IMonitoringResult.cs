using MemoryMonitoringDomein.ProcessMonitor.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryMonitoringDomein.ProcessMonitor.Data
{
    /// <summary>
    /// data object
    /// </summary>
    public interface IMonitoringResult
    {
        bool IsSafety();
        int Pid { get; }
        string FullName { get; }
        IMonitoringOptions Option { get; }
        long PagedMemorySize64UnitOfMB();
        long PagedSystemMemorySize64UnitOfMB();
    }
    /// <summary>
    /// data object for monitoring
    /// </summary>
    public class ProcessMonitoringResult : IMonitoringResult, IMonitoringOptions
    {
        IMonitoringOptions m_option;
        private int m_pid;
        private string m_fullPath;
        private readonly long m_pagedMemorySize64;
        private readonly long m_pagedSystemMemorySize64;
        public ProcessMonitoringResult(IMonitoringOptions option, long pagedMemorySize64, long pagedSystemMemorySize64, int pid, string fullPath)
        {
            m_option = option;
            m_pid = pid;
            m_fullPath = fullPath;
            m_pagedMemorySize64 = pagedMemorySize64;
            m_pagedSystemMemorySize64 = pagedSystemMemorySize64;
        }

        public string ApplicationName { get => m_option.ApplicationName; }
        public long ThresholdUnitOfMB { get => m_option.ThresholdUnitOfMB; }
        public string FullName => m_fullPath;
        int IMonitoringResult.Pid => m_pid;
        public bool IsSafety()
            => PagedMemorySize64UnitOfMB() < m_option.ThresholdUnitOfMB;
        public IMonitoringOptions Option => m_option;
        public long PagedMemorySize64UnitOfMB()
            => (m_pagedMemorySize64 / 1024L) / 1024L;
        public long PagedSystemMemorySize64UnitOfMB()
            => (m_pagedSystemMemorySize64 / 1024L) / 1024L;
    }
}
