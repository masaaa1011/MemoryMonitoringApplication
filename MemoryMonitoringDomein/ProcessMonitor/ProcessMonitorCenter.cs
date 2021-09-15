using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MemoryMonitoringDomein.ProcessMonitor.Data;
using MemoryMonitoringDomein.ProcessMonitor.Options;
using MemoryMonitoringDomein.ProcessMonitor.Factories;
using MemoryMonitoringDomein.ProcessMonitor;

namespace MemoryMonitoringDomein.ProcessMonitor
{
    public interface IMonitoringCenter
    {
        IEnumerable<IMonitoringResult> Monitor(List<IMonitoringOptions> targets);
    }
    public class ProcessMonitorCenter : IMonitoringCenter
    {
        private static List<IMonitor> CreateMonitersInternal(List<Process> source, IMonitoringOptions option, IMonitorFactory factory)
        {
            var monitors = new List<IMonitor>();
            source.ForEach(p =>
            {
                monitors.Add(
                        factory.Create(p, option)
                    );
            });
            return monitors;
        }
        private IMonitorFactory m_factory;
        internal ProcessMonitorCenter(IMonitorFactory factory)
            => m_factory = factory;
        public IEnumerable<IMonitoringResult> Monitor(List<IMonitoringOptions> targets)
        {
            var monitors = new List<IMonitor>();
            targets.ForEach(f =>
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(
                            f.ApplicationName.EndsWith(".exe") ? f.ApplicationName.TrimEnd(".exe".ToCharArray()) : f.ApplicationName
                        ).ToList();
                monitors.AddRange(
                    CreateMonitersInternal(processes, f, m_factory)
                        );
            });
            return monitors.Select(s => s.Result());
        }
    }
}
