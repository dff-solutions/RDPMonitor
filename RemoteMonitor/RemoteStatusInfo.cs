#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace RemoteMonitor
{
    public class RemoteStatusInfo
    {
        public readonly List<Server> ServerList = new List<Server>();

        public Status State { get; set; }
        public string Username { get; set; }

        public override int GetHashCode()
        {
            var s = State + " " + ServerList.OrderBy(x => x.Domain).Aggregate(string.Empty,
                (current, x) => current + (x + ", "));
            return s.GetHashCode();
        }
    }

    public class Server
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public DateTime EnteredServerTime { get; set; }
    }
}