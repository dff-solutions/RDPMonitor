#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HipChat;
using RemoteMonitor;

#endregion

namespace RemoteMonitorConsole
{
    internal class Program
    {
        private const string PathServerDirectory = "\\\\192.168.2.22\\projects\\dff\\intraweb\\remote_status";
        private const string HipChatToken = "9826fa999d476cd7ae21aed8ff4063";
        private const string HipChatRoom = "RemoteDesktop";
        private const string HipChatUser = "RemoteInfoBot";

        private static void Main()
        {
            var remoteMonitor = new RemoteMonitorWorker(PathServerDirectory);
            remoteMonitor.MyStatusChanged += RemoteMonitor_MyStatusChanged;
            remoteMonitor.StatusRemoteChanged += RemoteMonitor_StatusRemoteChanged;
            Console.ReadLine();
            remoteMonitor.Dispose();
        }

        private static void RemoteMonitor_StatusRemoteChanged(object sender, RemoteEventArgs e)
        {
            var statusReport = StatusReportAsStringGet();
            Console.WriteLine(statusReport);
        }

        private static void RemoteMonitor_MyStatusChanged(object sender, RemoteEventArgs e)
        {
            WriteStatusToServer(e);

            HipChatClient.BackgroundColor color;
            var statusReport = StatusReportAsStringGet(out color);

            Console.WriteLine(statusReport);
            SendToHipChat(statusReport, color);
        }

        private static async void SendToHipChat(string statusReport, HipChatClient.BackgroundColor color)
        {
            await Task.Run(() => HipChatClient.SendMessage(HipChatToken, HipChatRoom, HipChatUser,
                statusReport, false, color, HipChatClient.MessageFormat.html));
        }

        private static string StatusReportAsStringGet()
        {
            HipChatClient.BackgroundColor color;
            return StatusReportAsStringGet(out color);
        }

        private static string StatusReportAsStringGet(out HipChatClient.BackgroundColor color)
        {
            var report = string.Empty;
            var remoteStatusInfos = StatusInfoGet();
            color = HipChatClient.BackgroundColor.gray;
            foreach (var info in remoteStatusInfos.Where(x => x.ServerList != null).SelectMany(x => x.ServerList))
            {
                report += info;
                var user = remoteStatusInfos.FirstOrDefault(x => x.ServerList.Contains(info)).Username;
                report += " (" + user + ")";
                report += "<br>";
            }
            if (string.IsNullOrEmpty(report))
            {
                color = HipChatClient.BackgroundColor.green;
                report += "Alles frei!";
            }
            return report;
        }

        private static IEnumerable<RemoteStatusInfo> StatusInfoGet()
        {
            var report = new List<RemoteStatusInfo>();
            var di = new DirectoryInfo(PathServerDirectory);
            if (!di.Exists) di.Create();
            foreach (var fileInfo in di.GetFiles("*.xml"))
            {
                try
                {
                    using (var fs = new FileStream(fileInfo.FullName, FileMode.Open))
                    {
                        var ser = new XmlSerializer(typeof (RemoteStatusInfo));
                        report.Add((RemoteStatusInfo) ser.Deserialize(fs));
                    }
                }
                catch (Exception e)
                {
                }
            }
            return report;
        }

        private static void WriteStatusToServer(RemoteEventArgs e)
        {
            try
            {
                var path = PathServerDirectory + "\\" + e.Info.Username + ".xml";

                TextWriter txtW = new StreamWriter(path);
                var seri = new XmlSerializer(e.Info.GetType());
                seri.Serialize(txtW, e.Info);
                txtW.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}