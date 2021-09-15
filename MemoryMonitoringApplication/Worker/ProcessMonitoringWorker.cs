using MemoryMonitoringApplication.ChainHandler;
using MemoryMonitoringApplication.Extentions;
using MemoryMonitoringApplication.Logger;
using MemoryMonitoringDomein.ProcessMonitor.Options;
using MemoryMonitoringDomein.ProcessMonitor;
using MemoryMonitoringDomein.ProcessMonitor.Factories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemoryMonitoringApplication.Worker
{
    /// <summary>
    /// worker object.
    /// will manage execution monitoring function.
    /// </summary>
    public interface IWorkerService
    {
        void Execute(CancellationToken stoppingToken);
        void SendToMeStartSignal();
        void SendToMeStopSignal();
    }

    public class ProcessMonitoringWorker : IWorkerService
    {
        private ILogger m_Logger;
        private IWindowsEventLogger m_winLogger;
        private IMonitorCenterFactory m_factory;
        private List<IMonitoringOptions> m_options;
        private IMonitorTimeStateMachine m_state;
        private IMonitoringResultChainHandler m_chainHandler;
        private bool m_isExecutionAllowed = false;
        public ProcessMonitoringWorker(IMonitorCenterFactory factory, NameValueCollection options, ILogger logger, IWindowsEventLogger winLogger, IMonitoringResultChainHandler chainhandler)
        {
            m_Logger = logger;
            m_winLogger = winLogger;
            m_factory = factory;
            m_options = options.FetchMonitorOptions();
            m_state = new MonitorTimeStateMachine(
                m_Logger,
                    TimeSpan.Parse(ConfigurationManager.AppSettings.Get("MonitoringTimehhmm"))
                );
            m_chainHandler = chainhandler;
        }

        public async void Execute(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (m_isExecutionAllowed && EnsureExecuteAllowed())
                    {
                        m_Logger.WriteLog("監視を実行します。");
                        var executor = m_factory.Create();
                        var result = executor.Monitor(m_options);

                        result.ToList().ForEach(f =>
                        {
                            m_Logger.WriteLog($"exe名: {f.Option.ApplicationName} / 使用メモリ(MB)：{f.PagedMemorySize64UnitOfMB()} / 閾値(MB): {f.Option.ThresholdUnitOfMB}");
                        });

                        if (result.Any(a => !a.IsSafety()))
                            m_chainHandler.Chain(
                                    result.Where(w => !w.IsSafety()).ToList()
                                );
                    }
                }
                catch (Exception e)
                {
                    m_state.Failed();
                    m_Logger.WriteLog("モニターの監視に失敗しました。", e);
                }

                await Task.Delay(20000);
            }
        }
        public bool EnsureExecuteAllowed()
            => m_state.IsMonitoringAllowed();
        public void SendToMeStartSignal()
            => m_isExecutionAllowed = true;
        public void SendToMeStopSignal()
            => m_isExecutionAllowed = false;
        public void RefreshOptions()
        {
            ConfigurationManager.RefreshSection("monitoringApplications");
            m_options = ((NameValueCollection)ConfigurationManager.GetSection("monitoringApplications")).FetchMonitorOptions();
        }
    }
}
