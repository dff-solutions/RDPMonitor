using System;

namespace RemoteMonitor
{
    public class RemoteEventArgs : EventArgs
    {
        public RemoteEventArgs()
        {
        }

        public RemoteEventArgs(RemoteStatusInfo info)
        {
            Info = info;
        }

        public RemoteStatusInfo Info { get; set; }
    }
}