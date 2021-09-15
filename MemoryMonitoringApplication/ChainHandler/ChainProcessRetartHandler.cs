using MemoryMonitoringApplication.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryMonitoringApplication.ChainHandler
{
    /// <summary>
    /// モニタリング結果のプロセスに対するActionオブジェクト
    /// Chain Of Responsibilityパターン
    /// </summary>
    public interface IProcessRestartChainHandler
    {
        bool HasNext();
        IProcessRestartChainHandler SetChain(IProcessRestartChainHandler chain);
        List<Process> Chain(List<Process> processes);
    }

    public abstract class AProcessChain : IProcessRestartChainHandler
    {
        protected ILogger m_logger;
        protected IProcessRestartChainHandler m_handler;
        public AProcessChain(ILogger logger)
        {
            m_logger = logger;
        }
        public IProcessRestartChainHandler SetChain(IProcessRestartChainHandler chain)
        {
            m_handler = chain;
            return this;
        }


        public abstract List<Process> Chain(List<Process> processes);

        public bool HasNext()
            => m_handler != null;
    }
    /// <summary>
    /// ※Attension※
    /// args: working Process
    /// return: after process shutdown, create new process and return. and its not started.
    /// </summary>
    public class KillProcessChain : AProcessChain
    {
        public KillProcessChain(ILogger logger) : base(logger) { }
        public override List<Process> Chain(List<Process> processes)
        {
            var processStartInfos = new List<ProcessStartInfo>();
            processes.ForEach(p =>
            {
                try
                {
                    //p.CloseMainWindow();
                    var processStartInfo = new ProcessStartInfo()
                    {
                        FileName = p.MainModule.FileName,
                    };

                    p.Kill();
                    p.WaitForExit(10000);
                    if (p.HasExited)
                    {
                        processStartInfos.Add(processStartInfo);
                        m_logger.WriteLog($"プロセスをKillしました。 プロセス名={p.ProcessName} / PID={p.Id}");
                    }
                    else
                    {
                        m_logger.WriteLog($"プロセスを終了出来ませんでした。 プロセス名={p.ProcessName} / PID={p.Id}");
                    }
                }
                catch (Exception e)
                {
                    m_logger.WriteLog($"プロセスを終了出来ませんでした。 プロセス名={p.ProcessName} / PID={p.Id}", e);
                }
            });

            var response = processStartInfos.Select(s => new Process() { StartInfo = s }).ToList();
            if (HasNext())
                return m_handler.Chain(response);
            else
                return processes;
        }
    }
    /// <summary>
    /// ※Attention
    /// args: not started processes
    /// return: recieved process that was started this instance.
    /// </summary>
    public class StartProcessChain : AProcessChain
    {
        public StartProcessChain(ILogger logger) : base(logger) { }
        public override List<Process> Chain(List<Process> processes)
        {
            processes.ForEach(f =>
            {
                var isSucessed = f.Start();
                if (!isSucessed)
                    m_logger.WriteLog($"プロセスの再起動に失敗しました。{f.ProcessName}");
            });

            if (HasNext())
                return m_handler.Chain(processes);
            else
                return processes;
        }
    }
}
