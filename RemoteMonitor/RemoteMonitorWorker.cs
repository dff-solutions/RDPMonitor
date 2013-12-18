#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;

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

        public RemoteMonitorWorker()
        {
            _timer = new Timer(GetStatus, new object(), 1000, 1000);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        public event EventHandler<RemoteEventArgs> StatusChanged;

        protected void OnStatusChanged(RemoteEventArgs e)
        {
            var handler = StatusChanged;
            if (handler != null) handler(this, e);
        }

        private void GetStatus(object state)
        {
            var returningInfo = new RemoteStatusInfo {ServerList = new List<string>()};

            var identity = WindowsIdentity.GetCurrent();
            if (identity == null) return;

            var principal = new WindowsPrincipal(identity);
            returningInfo.Username = principal.Identity.Name;

            if (returningInfo.Username.Contains("\\"))
                returningInfo.Username = returningInfo.Username.Substring(returningInfo.Username.IndexOf("\\") + 1);


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
                            (current, x) => current + (x + "-")).Trim());
                }
            }

            returningInfo.State = returningInfo.ServerList.Any() ? Status.RDPAktiv : Status.RDPInaktiv;

            if (_lastRemoteStatusInfo != null &&
                _lastRemoteStatusInfo != null & returningInfo.GetHashCode() != _lastRemoteStatusInfo.GetHashCode())
                OnStatusChanged(new RemoteEventArgs(returningInfo));

            _lastRemoteStatusInfo = returningInfo;
        }
    }
}