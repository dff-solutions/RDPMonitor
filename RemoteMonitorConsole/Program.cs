#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using dff.Extensions;
using HipChat;
using RemoteMonitor;

#endregion

namespace RemoteMonitorConsole
{
    internal class Program
    {
        private static string _lastCommand;

        private static void Main()
        {
            var remoteMonitor = new RemoteMonitorWorker();
            remoteMonitor.StatusChanged += RemoteMonitor_StatusChanged;
            Console.ReadLine();
        }

        private static void RemoteMonitor_StatusChanged(object sender, RemoteEventArgs e)
        {
            WriteStatusToServer(e);

            var statusReport = StatusReportAsStringGet();

            Console.WriteLine(statusReport);
            SendToHipChat(statusReport);
        }

        private static void SendToHipChat(string statusReport)
        {
            if (_lastCommand == null || _lastCommand != statusReport)
            {
                _lastCommand = statusReport;
                HipChatClient.SendMessage("9826fa999d476cd7ae21aed8ff4063", "RemoteDesktop", "RemoteInfoBot",
                    statusReport, false, HipChatClient.BackgroundColor.gray, HipChatClient.MessageFormat.html);
            }
        }

        private static IEnumerable<RemoteStatusInfo> StatusInfoGet()
        {
            var report = new List<RemoteStatusInfo>();
            var di = new DirectoryInfo("\\\\dffgoe\\projects\\dff\\intraweb\\remote_status");
            if (!di.Exists) di.Create();
            foreach (var fileInfo in di.GetFiles("*.xml"))
            {
                using (var fs = new FileStream(fileInfo.FullName, FileMode.Open))
                {
                    var ser = new XmlSerializer(typeof(RemoteStatusInfo));
                    report.Add((RemoteStatusInfo)ser.Deserialize(fs));
                }
                
            }
            return report;
        }

        private static string StatusReportAsStringGet()
        {
            var report = string.Empty;
            foreach (var info in StatusInfoGet())
            {
                report += info.Username + " / Status: " + info.State;
                if (info.ServerList.Any())
                {
                    report += " / Server: " +
                              info.ServerList.Aggregate(string.Empty, (current, x) => current + (x + ", "))
                                  .RemoveLast(2);
                }
                report += "<br>";
            }
            return report ;
        }

        private static void WriteStatusToServer(RemoteEventArgs e)
        {
            var path = "\\\\dffgoe\\projects\\dff\\intraweb\\remote_status\\" + e.Info.Username + ".xml";
            if (File.Exists(path))
                File.Delete(path);

            TextWriter txtW = new StreamWriter(path);
            var seri = new XmlSerializer(e.Info.GetType());
            seri.Serialize(txtW, e.Info);
            txtW.Close();
        }
    }
}