#region

using System;
using System.IO;
using System.Linq;
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
        private FileSystemWatcher _watcher;
        private readonly string _pathToServerDirectory;

        public RemoteMonitorWorker(string pathToServerDirectory)
        {
            _pathToServerDirectory = pathToServerDirectory;
            _timer = new Timer(GetRemoteStatus, new object(), 1000, 1000);

            _watcher = new FileSystemWatcher(pathToServerDirectory)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastAccess,
                Filter = "*.xml"
            };
            _watcher.Created += WatcherCreated;
            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void WatcherCreated(object sender, FileSystemEventArgs e)
        {
            //Bei Änderungen an der Eigenen Datei nichts machen!
            if (e.Name.Contains(UsernameGet())) return;
            OnStatusRemoteChanged(new RemoteEventArgs {Info = GetRemoteStatusFromServer()});
        }

        public event EventHandler<RemoteEventArgs> MyStatusChanged;
        public event EventHandler<RemoteEventArgs> StatusRemoteChanged;

        protected void OnMyStatusChanged(RemoteEventArgs e)
        {
            var handler = MyStatusChanged;
            if (handler != null) handler(this, e);
        }

        protected void OnStatusRemoteChanged(RemoteEventArgs e)
        {
            var handler = StatusRemoteChanged;
            if (handler != null) handler(this, e);
        }

        private void GetRemoteStatus(object state)
        {
            var returningInfo = GetRemoteStatusFromServer();
            if (returningInfo == null) return;

            if (_lastRemoteStatusInfo != null &&
                _lastRemoteStatusInfo != null & returningInfo.GetHashCode() != _lastRemoteStatusInfo.GetHashCode())
            {
                _lastRemoteStatusInfo = returningInfo;
                ServerClient.WriteStatusToServer(returningInfo, _pathToServerDirectory);
                OnMyStatusChanged(new RemoteEventArgs(returningInfo));
            }
            else _lastRemoteStatusInfo = returningInfo;
        }

        private static RemoteStatusInfo GetRemoteStatusFromServer()
        {
            var returningInfo = new RemoteStatusInfo {Username = UsernameGet()};

            if (string.IsNullOrEmpty(returningInfo.Username)) return null;

            var windows = new Windows();
            foreach (
                var w in
                    windows.lstWindows.Where(
                        x =>
                            x.winVisible & x.winTitle != "Remotedesktopverbindung" &
                            x.winTitle.Contains("Remotedesktopverbindung")))
            {
                var infoSplit =
                    w.winTitle.Substring(0, w.winTitle.IndexOf("- Remotedesktopverbindung")).Split("-".ToCharArray());
                if (infoSplit.Length >= 2)
                {
                    returningInfo.ServerList.Add(
                        infoSplit.Where(x => !x.Contains("Remotedesktopverbindung")).Aggregate(string.Empty,
                            (current, x) => current + (x + "-")).Trim().RemoveLast(2));
                }
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