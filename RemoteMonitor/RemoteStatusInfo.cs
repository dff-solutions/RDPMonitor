#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace RemoteMonitor
{
    public class RemoteStatusInfo
    {
        public readonly List<string> ServerList= new List<string>();

        public RemoteStatusInfo()
        {}
        
        public Status State { get; set; }
        public string Username { get; set; }

        public override int GetHashCode()
        {
            var s = State + " " + ServerList.OrderBy(x => x).Aggregate(string.Empty,
                (current, x) => current + (x + ", "));
            return s.GetHashCode();
        }
    }
}