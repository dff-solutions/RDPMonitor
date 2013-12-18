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

            var color=HipChatClient.BackgroundColor.gray;
            var statusReport = StatusReportAsStringGet(out color);

            Console.WriteLine(statusReport);
            SendToHipChat(statusReport, color);
        }

        private static void SendToHipChat(string statusReport, HipChatClient.BackgroundColor color)
        {
            if (_lastCommand == null || _lastCommand != statusReport)
            {
                _lastCommand = statusReport;
                HipChatClient.SendMessage("9826fa999d476cd7ae21aed8ff4063", "RemoteDesktop", "RemoteInfoBot",
                    statusReport, false, color, HipChatClient.MessageFormat.html);
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

        private static string StatusReportAsStringGet(out HipChatClient.BackgroundColor color)
        {
            var report = string.Empty;
            var remoteStatusInfos = StatusInfoGet();
            color = HipChatClient.BackgroundColor.gray;
            foreach (var info in remoteStatusInfos.Where(x=>x.ServerList!=null).SelectMany(x => x.ServerList))
            {
                report += info;
                var user = remoteStatusInfos.FirstOrDefault(x => x.ServerList.Contains(info)).Username;
                report += " (" + user + ")";
                report += "<br>";
            }
            if (string.IsNullOrEmpty(report))
            {
                color=HipChatClient.BackgroundColor.green;
                report += "Alles frei!";
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