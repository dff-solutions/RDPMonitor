using System.Collections.Generic;
using System.Linq;

namespace RemoteMonitor
{
    public class RemoteStatusInfo
    {
        public List<string> ServerList;
        public Status State { get; set; }
        public string Username { get; set; }

        public override int GetHashCode()
        {
            return (State + " " + ServerList.OrderBy(x=>x).Aggregate(string.Empty,
                (current, x) => current + (x + ", "))).GetHashCode();
        }
    }
}