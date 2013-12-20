#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

#endregion

namespace RemoteMonitor
{
    public class ServerClient
    {
        public static string StatusReportAsStringGet(string projectsDffIntrawebRemoteStatus)
        {
            var report = string.Empty;
            var remoteStatusInfos = StatusInfoGet(projectsDffIntrawebRemoteStatus);
            foreach (var info in remoteStatusInfos.Where(x => x.ServerList != null).SelectMany(x => x.ServerList))
            {
                report += info;
                var user = remoteStatusInfos.FirstOrDefault(x => x.ServerList.Contains(info)).Username;
                report += " (" + user + ")";
                report += "<br>";
            }
            if (string.IsNullOrEmpty(report))
            {
                report += "Alles frei!";
            }
            return report;
        }

        private static IEnumerable<RemoteStatusInfo> StatusInfoGet(string projectsDffIntrawebRemoteStatus)
        {
            var report = new List<RemoteStatusInfo>();
            var di = new DirectoryInfo(projectsDffIntrawebRemoteStatus);
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

        public static void WriteStatusToServer(RemoteStatusInfo e, string projectsDffIntrawebRemoteStatus)
        {
            try
            {
                var path = projectsDffIntrawebRemoteStatus + "\\" + e.Username + ".xml";

                TextWriter txtW = new StreamWriter(path);
                var seri = new XmlSerializer(e.GetType());
                seri.Serialize(txtW, e);
                txtW.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}