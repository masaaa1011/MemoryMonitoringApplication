using MemoryMonitoringApplication.Logger;
using MemoryMonitoringDomein.ProcessMonitor.Data;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryMonitoringApplication.ChainHandler
{
    /// <summary>
    /// Chain objects recieveing, and returning "IMonitoringResult"
    /// </summary>
    public interface IMonitoringResultChainHandler
    {
        void Chain(List<IMonitoringResult> targets);
        IMonitoringResultChainHandler AddChain(IMonitoringResultChainHandler handler);
        bool HasNext();
    }
    public abstract class AChainHandler : IMonitoringResultChainHandler
    {
        protected ILogger m_logger;
        protected IWindowsEventLogger m_winLogger;
        protected List<IMonitoringResultChainHandler> m_handlers = new List<IMonitoringResultChainHandler>();
        public AChainHandler(ILogger logger, IWindowsEventLogger winLogger)
        {
            m_logger = logger;
            m_winLogger = winLogger;
        }

        public IMonitoringResultChainHandler AddChain(IMonitoringResultChainHandler handler)
        { 
            m_handlers.Add(handler);
            return this;
        }

        public abstract void Chain(List<IMonitoringResult> targets);

        public bool HasNext()
            => m_handlers.Count > 0;
    }
    /// <summary>
    /// Write Windows Event Log
    /// args: IMonitoringREsult
    /// return: IMonitoringREsult
    /// </summary>
    public class HandleWindowsEventLog : AChainHandler
    {
        public HandleWindowsEventLog(ILogger logger, IWindowsEventLogger winLogger) : base(logger, winLogger)
        {

        }

        public override void Chain(List<IMonitoringResult> targets)
        {
            targets.ForEach(f =>
            {
                m_winLogger.WriteLog($"{f.Option.ApplicationName}が設定されたメモリ使用量を超えています。{Environment.NewLine}    設定された閾値: {f.Option.ThresholdUnitOfMB}(MB){Environment.NewLine}    検知されたメモリ使用量{f.PagedMemorySize64UnitOfMB()}(MB)");
            });

            if (HasNext())
                m_handlers.ForEach(f => f.Chain(targets));
        }
    }

    /// <summary>
    /// Notify current working memory size was abandoned using windows nofity.
    /// if user selected "restart" process will restart.
    /// args: IMonitoringResult
    /// return: IMonitoringResult
    /// </summary>
    public class NotifyAbandonedApplicationWithProcessRestart : AChainHandler
    {
        public NotifyAbandonedApplicationWithProcessRestart(ILogger logger, IWindowsEventLogger winLogger) : base(logger, winLogger)
        {

        }
        public override void Chain(List<IMonitoringResult> targets)
        {

            var notifies = new List<ToastContentBuilder>();
            targets.ForEach(f => 
            {
                var builder = new ToastContentBuilder()
                    .AddArgument("application", f.Option.ApplicationName)
                    .AddText($"{f.Option.ApplicationName}が設定されたメモリ使用量を超えています。")
                    .AddText($"設定された閾値: {f.Option.ThresholdUnitOfMB}(MB)")
                    .AddText($"検知されたメモリ使用量{ f.PagedMemorySize64UnitOfMB()}(MB)")
                    .AddButton(new ToastButton()
                                    .SetContent("プロセスを再起動")
                                    .AddArgument("action", "restart")
                                    .SetBackgroundActivation()
                              )
                    .AddButton(new ToastButton()
                                    .SetContent("何もしない")
                                    .AddArgument("action", "none")
                                    .SetBackgroundActivation()
                              );
                notifies.Add(builder);
            });

            notifies.ForEach(f => f.Show());
            if (HasNext())
                m_handlers.ForEach(f => f.Chain(targets));
        }
    }

    /// <summary>
    /// Notify current working memory size was abandoned using windows nofity.
    /// </summary>
    public class NotifyAbandonedApplication : AChainHandler
    {
        public NotifyAbandonedApplication(ILogger logger, IWindowsEventLogger winLogger) : base(logger, winLogger)
        {

        }
        public override void Chain(List<IMonitoringResult> targets)
        {

            var notifies = new List<ToastContentBuilder>();
            targets.ForEach(f =>
            {
                var builder = new ToastContentBuilder()
                    .AddArgument("application", f.Option.ApplicationName)
                    .AddText($"{f.Option.ApplicationName}が設定されたメモリ使用量を超えています。")
                    .AddText($"設定された閾値: {f.Option.ThresholdUnitOfMB}(MB)")
                    .AddText($"検知されたメモリ使用量{ f.PagedMemorySize64UnitOfMB()}(MB)");
                notifies.Add(builder);
            });

            notifies.ForEach(f => f.Show());
            if (HasNext())
                m_handlers.ForEach(f => f.Chain(targets));
        }
    }
}
