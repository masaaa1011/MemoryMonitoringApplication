using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MemoryMonitoringApplication.Logger
{
    public interface IWindowsEventLogger
    {
        void WriteLog(string message);
    }
    public class WindowsEventLogger : IWindowsEventLogger
    {
        public void WriteLog(string message)
        {
            try
            {
                var source = System.Reflection.Assembly.GetExecutingAssembly().Location.TrimEnd(
                        System.IO.Path.GetExtension(System.Reflection.Assembly.GetExecutingAssembly().Location).ToCharArray()
                    );
                if (!System.Diagnostics.EventLog.SourceExists(source))
                {
                    System.Diagnostics.EventLog.CreateEventSource(source, string.Empty);
                }
                var eventid = int.Parse(
                        ConfigurationManager.AppSettings.Get("EventId")
                    );
                var eventcategory = short.Parse(
                        ConfigurationManager.AppSettings.Get("EventCategory")
                    );
                System.Diagnostics.EventLog.WriteEntry(source, message, System.Diagnostics.EventLogEntryType.Error, eventid, eventcategory, null);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }


}
