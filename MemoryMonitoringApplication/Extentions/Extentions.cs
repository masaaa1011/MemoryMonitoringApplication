using MemoryMonitoringDomein.ProcessMonitor.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryMonitoringApplication.Extentions
{
    public static class AppSettingsExtentions
    {
        /// <summary>
        /// monitoringApplications　からkey and valueで設定を取得する
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<IMonitoringOptions> FetchMonitorOptions(this NameValueCollection source)
        {
            var options = new List<IMonitoringOptions>();
            source.AllKeys.ToList().ForEach(f =>
            {
                var opt = new { key = f, value = source.Get(f) };
                if (!string.IsNullOrEmpty(opt.key) && !string.IsNullOrEmpty(opt.value) && long.TryParse(opt.value, out var value))
                    options.Add(
                        new ProcessMonitoringOptions { ApplicationName = opt.key, ThresholdUnitOfMB = value }
                        );
            });

            return options;
        }
    }
}
