using MemoryMonitoringApplication.ChainHandler;
using MemoryMonitoringApplication.Logger;
using MemoryMonitoringDomein.ProcessMonitor.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryMonitoringApplication.Restartor
{
    /// <summary>
    /// will restart process.
    /// </summary>
    public interface IRestartor
    {
        void Restart(List<IMonitoringResult> targets);
    }

    /// will restart process using chain handler "IProcessRestartChainHandler"
    public class ProcessRestartor : IRestartor
    {
        private ILogger m_logger;
        private IProcessRestartChainHandler m_handler;
        public ProcessRestartor(ILogger logger, IProcessRestartChainHandler handler)
        {
            m_logger = logger;
            m_handler = handler;
        }
        public void Restart(List<IMonitoringResult> targets)
        {
            var processes = new List<Process>();
            targets.ForEach(f =>
            {
                processes.AddRange(System.Diagnostics.Process.GetProcessesByName(
                                f.Option.ApplicationName.EndsWith(".exe") ? f.Option.ApplicationName.TrimEnd(".exe".ToCharArray()) : f.Option.ApplicationName
                            ).Where(w => w.Id == f.Pid).ToList()
                        );
            });

            m_handler.Chain(processes);
        }
    }
}
