using MemoryMonitoringDomein.ProcessMonitoring;
using MemoryMonitoringDomein.MonitorCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryMonitoringDomein.MonitorCenter.Options;
using MemoryMonitoringDomein.ProcessMonitoring.Factories;

namespace MemoryMonitoringConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            IMonitorFactory monitorFactory = new ProcessMonitorFactory();
            IMonitorCenterFactory monitorCenterFactory = new ProcessMonitorCenterFactory(monitorFactory);
            var target = monitorCenterFactory.Create();

            var _args = new List<IMonitoringOptions>()
            {
                new ProcessMonitoringOptions(){ ApplicationName = "notepad.exe", ThresholdUnitOfMB = 8000000 },
                new ProcessMonitoringOptions(){ ApplicationName = "explorer.exe", ThresholdUnitOfMB = 8000000 },
            };

            var results = target.Monitor(_args);
        }
    }
}
