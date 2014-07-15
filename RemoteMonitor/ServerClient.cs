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
                report += info.Name+" ["+info.Domain+"]";
                var user = remoteStatusInfos.FirstOrDefault(x => x.ServerList.Contains(info)).Username;
                report += " (" + user + ", seit "+info.EnteredServerTime.ToString("g")+")";
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
                if(fileInfo.Name == "db.xml") continue;
                try
                {
                    using (var fs = new FileStream(fileInfo.FullName, FileMode.Open))
                    {
                        var ser = new XmlSerializer(typeof(RemoteStatusInfo));
                        report.Add((RemoteStatusInfo)ser.Deserialize(fs));
                    }
                }
                catch (Exception e)
                {
                }
            }
            return report;
        }

        public static RemoteServerDb ServerDbGet(string projectsDffIntrawebRemoteStatus)
        {
            var di = new DirectoryInfo(projectsDffIntrawebRemoteStatus);
            if (!di.Exists) di.Create();
            foreach (var fileInfo in di.GetFiles("db.xml"))
            {
                try
                {
                    using (var fs = new FileStream(fileInfo.FullName, FileMode.Open))
                    {
                        var ser = new XmlSerializer(typeof(RemoteServerDb));
                        return (RemoteServerDb) ser.Deserialize(fs);
                    }
                }
                catch (Exception e)
                {
                }
            }
            return null;
        }

        public static void ServerDbSet(RemoteServerDb e, string projectsDffIntrawebRemoteStatus)
        {
            try
            {
                TextWriter txtW = new StreamWriter(projectsDffIntrawebRemoteStatus + "\\db.xml");
                var seri = new XmlSerializer(e.GetType());
                seri.Serialize(txtW, e);
                txtW.Close();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public static RemoteStatusInfo ReadAStatusToServer(string projectsDffIntrawebRemoteStatus, string username)
        {
            var report = new RemoteStatusInfo();
            var di = new DirectoryInfo(projectsDffIntrawebRemoteStatus);
            if (!di.Exists) di.Create();
            foreach (var fileInfo in di.GetFiles("*" + username + ".xml"))
            {
                try
                {
                    using (var fs = new FileStream(fileInfo.FullName, FileMode.Open))
                    {
                        var ser = new XmlSerializer(typeof (RemoteStatusInfo));
                        report = ((RemoteStatusInfo) ser.Deserialize(fs));
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