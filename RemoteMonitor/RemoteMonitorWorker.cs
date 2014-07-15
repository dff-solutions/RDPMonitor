#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using dff.Extensions;

#endregion

namespace RemoteMonitor
{
    public enum Status
    {
        RDPAktiv,
        RDPInaktiv,
        Error
    }

    public class RemoteMonitorWorker : IDisposable
    {
        private readonly Timer _timer;
        private RemoteStatusInfo _lastRemoteStatusInfo;
        private RemoteServerDb _lastServerDb;
        private FileSystemWatcher _watcher;
        private readonly string _pathToServerDirectory;
        private DateTime _lastRemoteChange;

        public RemoteServerDb ServerList { get { return _lastServerDb; }}

        public RemoteMonitorWorker(string pathToServerDirectory)
        {
            _pathToServerDirectory = pathToServerDirectory;
            AnalyseWindows(null);
            _timer = new Timer(AnalyseWindows, new object(), 1000, 1000);
            _watcher = new FileSystemWatcher(pathToServerDirectory)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.xml"
            };
            _watcher.Changed += WatcherCreated;
            _watcher.EnableRaisingEvents = true;
        }

        public RemoteMonitorWorker(string pathToServerDirectory, RemoteStatusInfo remoteStatusInfo)
        {
            _pathToServerDirectory = pathToServerDirectory;
            AnalyseWindowsConsole(MergeStatusInfo(remoteStatusInfo));
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void WatcherCreated(object sender, FileSystemEventArgs e)
        {
            //Bei Änderungen an der Eigenen Datei nichts machen!
            if (e.Name.Contains(UsernameGet()) ||
                new TimeSpan(DateTime.Now.Ticks - _lastRemoteChange.Ticks).TotalSeconds < 1) return;

            _lastRemoteChange = DateTime.Now;
            MergeServerDb(null);
            OnStatusRemoteChanged();
        }

        public event EventHandler<RemoteEventArgs> MyStatusChanged;
        public event EventHandler<EventArgs> StatusRemoteChanged;

        protected void OnMyStatusChanged(RemoteEventArgs e)
        {
            var handler = MyStatusChanged;
            if (handler != null) handler(this, e);
        }

        protected void OnStatusRemoteChanged()
        {
            var handler = StatusRemoteChanged;
            if (handler != null) handler(this, new EventArgs());
        }

        public void AnalyseWindowsConsole(RemoteStatusInfo remoteStatusInfo)
        {
            if (remoteStatusInfo == null) return;
            if (ServerList == null)
                MergeServerDb(remoteStatusInfo);

            _lastRemoteStatusInfo = remoteStatusInfo;
            ServerClient.WriteStatusToServer(remoteStatusInfo, _pathToServerDirectory);
            OnMyStatusChanged(new RemoteEventArgs(remoteStatusInfo));
        }

        private void AnalyseWindows(object state)
        {
            var info = AnalyseWindowsForRemoteSessions();
            if (info == null) return;

            if (ServerList == null)
                MergeServerDb(info);

            if (_lastRemoteStatusInfo != null &&
                _lastRemoteStatusInfo != null & info.GetHashCode() != _lastRemoteStatusInfo.GetHashCode())
            {
                MergeServerDb(info);
                _lastRemoteStatusInfo = info;
                ServerClient.WriteStatusToServer(info, _pathToServerDirectory);
                OnMyStatusChanged(new RemoteEventArgs(info));
            }
            else _lastRemoteStatusInfo = info;
        }

        private void MergeServerDb(RemoteStatusInfo ist)
        {
            var serverDb = ServerClient.ServerDbGet(_pathToServerDirectory) ?? new RemoteServerDb();
            if (ist != null)
            {
                foreach (var entry in ist.ServerList)
                {
                    if (serverDb.All(x => x.Domain != entry.Domain && entry.Name != string.Empty))
                    {
                        serverDb.Add(new RemoteServerDbItem(entry.Domain, entry.Name, ist.State, ist.Username));
                    }
                }
                foreach (var entry in serverDb)
                {
                    var entrys = ist.ServerList.FirstOrDefault(
                        x => x.Domain == entry.Domain);
                    if (entrys != null)
                    {
                        entry.Username = ist.Username;
                        entry.State = ist.State;
                    }
                    else
                        entry.State = Status.RDPInaktiv;
                }
                serverDb.Sort((x, y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal));
                _lastServerDb = serverDb;
                ServerClient.ServerDbSet(serverDb, _pathToServerDirectory);
            }
            else
            {
                _lastServerDb = serverDb;
            }
        }

        private RemoteStatusInfo MergeStatusInfo(RemoteStatusInfo editInfo)
        {
            var now = ServerClient.ReadAStatusToServer(_pathToServerDirectory, UsernameGet()) ?? new RemoteStatusInfo();

            if (string.IsNullOrEmpty(now.Username)) now.Username = UsernameGet();

            if (editInfo != null && editInfo.State != Status.RDPAktiv)
                now.ServerList.RemoveAll(x => x.Domain == editInfo.ServerList[0].Domain);
            else if (editInfo != null && editInfo.State == Status.RDPAktiv)
                now.ServerList.Add(editInfo.ServerList[0]);

            if (now.ServerList.Count > 0) now.State = Status.RDPAktiv;
            else now.State = Status.RDPInaktiv;

            return now;
        }

        private static RemoteStatusInfo AnalyseWindowsForRemoteSessions()
        {
            var returningInfo = new RemoteStatusInfo {Username = UsernameGet()};

            if (string.IsNullOrEmpty(returningInfo.Username)) return null;

            var processlist = Process.GetProcessesByName("mstsc");
            var infoSplit =
                processlist.Where(x => x.MainWindowTitle.Contains("Remotedesktopverbindung")).Select(x => new
                {
                    windowName = x.MainWindowTitle.Split(new[] {" - "}, StringSplitOptions.RemoveEmptyEntries)
                })
                    .Where(x => x.windowName.Length >= 2)
                    .Select(x => new
                    {
                        Ip = x.windowName[x.windowName.Length - 2].Split(':').Length >= 1
                            ? x.windowName[x.windowName.Length - 2].Split(':')[0]
                            : string.Empty,

                        Port = x.windowName[x.windowName.Length - 2].Split(':').Length >= 2
                            ? x.windowName[x.windowName.Length - 2].Split(':')[1]
                            : "3389",
                        Name = x.windowName.Take(x.windowName.Length - 2).ToArray()
                    }).Select(x => new
                    {
                        Domain = x.Ip + ":" + x.Port,
                        Name = x.Name.Aggregate(string.Empty,(p, n) => string.IsNullOrEmpty(p) ? n : p + " - " + n)
                    }).ToArray();

            foreach (var entry in infoSplit)
            {
                returningInfo.ServerList.Add(new Server
                {
                    EnteredServerTime = DateTime.Now,
                    Name = entry.Name,
                    Domain = entry.Domain
                });
            }
            returningInfo.State = returningInfo.ServerList.Any() ? Status.RDPAktiv : Status.RDPInaktiv;

            return returningInfo;

        }

        private static string UsernameGet()
        {
            var username = string.Empty;
            var identity = WindowsIdentity.GetCurrent();
            if (identity == null) return null;

            var principal = new WindowsPrincipal(identity);
            username = principal.Identity.Name;

            if (username.Contains("\\"))
                username = username.Substring(username.IndexOf("\\") + 1);
            return username;
        }
    }
}