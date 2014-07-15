#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace RemoteMonitor
{
    public class RemoteServerDb : List<RemoteServerDbItem>
    {
        public RemoteServerDb()
        {
        }

        public override int GetHashCode()
        {
            var s = this.OrderBy(x => x.Domain).Aggregate(string.Empty,
                (current, x) => current + (x + ", "));
            return s.GetHashCode();
        }
    }

    public class RemoteServerDbItem
    {
        public RemoteServerDbItem(string dom, string nam, Status sta, string use)
        {
            Domain = dom;
            Name = nam;
            State = sta;
            Username = use;
        }

        public RemoteServerDbItem()
        {
        }

        public string Name { get; set; }
        public string Domain { get; set; }
        public Status State { get; set; }
        public string Username { get; set; }
    }
}