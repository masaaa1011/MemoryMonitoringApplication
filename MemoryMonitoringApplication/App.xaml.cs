using MemoryMonitoringApplication.ChainHandler;
using MemoryMonitoringApplication.Extentions;
using MemoryMonitoringApplication.Logger;
using MemoryMonitoringApplication.Restartor;
using MemoryMonitoringApplication.Worker;
using MemoryMonitoringDomein.ProcessMonitor;
using MemoryMonitoringDomein.ProcessMonitor.Factories;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Unity;
using Unity.Injection;
using Windows.Foundation.Collections;
using MessageBox = System.Windows.MessageBox;

namespace MemoryMonitoringApplication
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static string SemaphoreName => "MemoryMonitoringApplication";
        public static System.Threading.Semaphore _semaphore;
        private ILogger m_logger;
        private IUnityContainer m_container;
        private IWorkerService m_worker;
        private IRestartor m_restartor;
        private CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutDownIfApplicationWasNotFirstStarted();
            EnsureAppConfigValid();

            base.OnStartup(e);

            DependencyInjection();
            ActivateNotifyAction();
            StartUpNeededInstance();
            m_logger.WriteLog("アプリケーションを開始します");
        }
        /// <summary>
        /// 2重起動の防止
        /// </summary>
        private void ShutDownIfApplicationWasNotFirstStarted()
        {
            _semaphore = new System.Threading.Semaphore(1, 1, SemaphoreName, out var isCreatedFirstTime);
            if (!isCreatedFirstTime)
            {
                MessageBox.Show("監視くんは複数起動できません。ごめんなさい。", $"{System.Windows.Forms.Application.ProductName} Version {System.Windows.Forms.Application.ProductVersion}",
                MessageBoxButton.OK, MessageBoxImage.Hand);

                Shutdown();
                return;
            }
        }
        /// <summary>
        /// App.Configの設定値チェック
        /// </summary>
        private void EnsureAppConfigValid()
        {
            var isMonitoringTimehhmmValid = TimeSpan.TryParse(ConfigurationManager.AppSettings.Get("MonitoringTimehhmm"), out var monitoringTimehhmmResult);
            if (!isMonitoringTimehhmmValid)
                MessageBox.Show("実行時間は00:00～23:59で指定してください");

            var isDoUseWindowsNotifyValid = bool.TryParse(ConfigurationManager.AppSettings.Get("DoUseWindowsNotifyAndRestartProcess"), out var doUseWindowsNotifyResult);
            if (!isDoUseWindowsNotifyValid)
                MessageBox.Show("Windows通知の使用可否はtrue / falseで指定してください");

            if (!isMonitoringTimehhmmValid || !isDoUseWindowsNotifyValid)
                Shutdown();
        }
        /// <summary>
        /// Dependency Injection
        /// </summary>
        private void DependencyInjection()
        {
            m_container = new UnityContainer();

            m_container.RegisterType<ILogger, Log4NetLogger>();
            m_container.RegisterType<IWindowsEventLogger, WindowsEventLogger>();
            m_container.RegisterInstance<NotifyIcon>(DesignTaskTrayMenuContext());
            m_container.RegisterInstance<NameValueCollection>(((NameValueCollection)ConfigurationManager.GetSection("monitoringApplications")));
            m_container.RegisterType<IMonitorFactory, ProcessMonitorFactory>();
            m_container.RegisterType<IMonitorCenterFactory, ProcessMonitorCenterFactory>();
            m_container.RegisterType<IMonitorTimeStateMachine, MonitorTimeStateMachine>();
            m_container.RegisterType<IWorkerService, ProcessMonitoringWorker>();

            var chainsForNotify = new HandleWindowsEventLog(
                            m_container.Resolve<ILogger>(),
                            m_container.Resolve<IWindowsEventLogger>()
                        );
            if(bool.Parse(ConfigurationManager.AppSettings.Get("DoUseWindowsNotifyAndRestartProcess")))
                chainsForNotify.AddChain(new NotifyAbandonedApplicationWithProcessRestart(
                                    m_container.Resolve<ILogger>(),
                                    m_container.Resolve<IWindowsEventLogger>()
                                ));
            m_container.RegisterInstance<IMonitoringResultChainHandler>(chainsForNotify);

            var processRestart = new ProcessRestartor(
                    m_container.Resolve<ILogger>(),
                    new KillProcessChain(m_container.Resolve<ILogger>())
                    .SetChain( new StartProcessChain(m_container.Resolve<ILogger>())
                    )
                );
            m_container.RegisterInstance<IRestartor>(processRestart);
        }
        /// <summary>
        /// 実行サービスをDIコンテナから取得
        /// </summary>
        private void StartUpNeededInstance() 
        {
            m_logger = m_container.Resolve<ILogger>();
            m_worker = m_container.Resolve<IWorkerService>();
            m_worker.Execute(m_cancellationTokenSource.Token);

            m_restartor = m_container.Resolve<IRestartor>();
        }
        /// <summary>
        /// 終了ボタンイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, EventArgs e)
        {
            m_logger.WriteLog("アプリケーションを終了します");
            m_cancellationTokenSource.Cancel();
            m_container.Resolve<NotifyIcon>().Visible = false;
            Shutdown();
        }
        /// <summary>
        /// コンテキストメニューのアクティブ要素のトグル
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="target"></param>
        private void ToggleCheckedState(ToolStripMenuItem sender, ToolStrip target)
        {
            for (var i = 0; i < target.Items.Count; i++)
            {
                ((ToolStripMenuItem)target.Items[i]).Checked = false;
            }
            sender.Checked = true;
        }
        /// <summary>
        /// 監視開始イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Start_Monitoring_Click(object sender, EventArgs e)
        {
            ToggleCheckedState((ToolStripMenuItem)sender, ((ToolStripMenuItem)sender).Owner);
            m_worker.SendToMeStartSignal();
        }
        /// <summary>
        /// 監視終了イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Stop_Monitoring_Click(object sender, EventArgs e)
        {
            ToggleCheckedState((ToolStripMenuItem)sender, ((ToolStripMenuItem)sender).Owner);
            m_worker.SendToMeStopSignal();
        }
        /// <summary>
        /// Windowsの通知をオンした場合のクリックイベントを登録する
        /// </summary>
        public void ActivateNotifyAction()
        {
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
                if (!args.TryGetValue("application", out var applicationValue) || !args.TryGetValue("action", out var actionValue) || actionValue != "restart")
                    return;
                var monitorCenter = m_container.Resolve<IMonitorCenterFactory>().Create();
                var options = m_container.Resolve<NameValueCollection>().FetchMonitorOptions()
                                                                        .Where(w => w.ApplicationName == applicationValue)
                                                                        .ToList();
                var result = monitorCenter.Monitor(options).ToList();
                m_restartor.Restart(
                        result.Where(w => !w.IsSafety()).ToList()
                    );
            };
        }
        /// <summary>
        /// タスクトレイへとアプリケーションを登録する
        /// ①コンテキスメニューの登録
        /// 　a. 親メニューの登録(開始/停止)
        /// 　b. 子メニューの登録
        /// 　　(開始/停止)
        /// 　　　　→　監視の開始
        /// 　　　　→　監視の停止
        /// 　c. アプリケーション終了
        /// ②クリック時のイベントハンドラ登録
        /// ③アイコンメニュー画像の登録
        /// </summary>
        /// <returns></returns>
        public NotifyIcon DesignTaskTrayMenuContext() {
            var menuStartOrStop = new ToolStripMenuItem() 
            {
                Text = "開始/停止" ,
            };
            var menuMonitorStart = new ToolStripMenuItem()
            {
                Text = "監視の開始",
                CheckOnClick = true,
            };
            menuMonitorStart.Click += Start_Monitoring_Click;
            var menuMonitorStop = new ToolStripMenuItem()
            {
                Text = "監視の停止",
                CheckOnClick = true,
                Checked = true,
            };
            menuMonitorStop.Click += Stop_Monitoring_Click;
            menuStartOrStop.DropDownItems.Add(menuMonitorStart);
            menuStartOrStop.DropDownItems.Add(menuMonitorStop);

            var menu = new System.Windows.Forms.ContextMenuStrip();
            menu.Items.Add(menuStartOrStop);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("アプリケーション終了", null, Exit_Click);

            // NotifyIcon(タスクトレイのアイコン)の設定
            var notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Visible = true,
                Icon = new Icon(GetResourceStream(new Uri("assets/icon.ico", UriKind.Relative)).Stream),
                Text = "監視くん",
                ContextMenuStrip = menu,
            };
            // 左クリック時のイベント登録
            notifyIcon.MouseUp += (sender, e) =>
            {
                System.Threading.ThreadPool.GetMaxThreads(out var w, out var c);
                m_logger.WriteLog($"wocker{w} | completion{c}");
                if (e.Button == MouseButtons.Left)
                {
                    MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                    method.Invoke(notifyIcon, null);
                }
            };

            return notifyIcon;
        }
    }
}
