#region

using System;
using System.Linq;
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
            var statusReport = ServerClient.StatusReportAsStringGet(PathServerDirectory);
            Console.WriteLine(statusReport);
        }

        private static void RemoteMonitor_MyStatusChanged(object sender, RemoteEventArgs e)
        {
            var statusReport = ServerClient.StatusReportAsStringGet(PathServerDirectory);
            Console.WriteLine(statusReport);

            HipChatClientDff.SendToHipChat(statusReport,
                !e.Info.ServerList.Any()
                    ? HipChatClientDff.BackgroundColor.Green
                    : HipChatClientDff.BackgroundColor.Gray, HipChatToken,
                HipChatRoom, HipChatUser);
        }
    }
}