using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FwRulesRepair.cst;
using FwRulesRepair.utils;

namespace FwRulesRepair.dto
{
    class EntryAdv : IEquatable<EntryAdv>
    {

        public string AppPath { get; }
        public DirectionsEnum Direction { get; set; }

        public EntryAdv(EventLogEntry eventLogEntry)
        {
            AppPath = DevicePathMapper.FromDevicePath(eventLogEntry.ReplacementStrings[1]);

        }


        public bool Equals(EntryAdv other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return AppPath == other.AppPath && Equals(Direction, other.Direction);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntryAdv)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((AppPath != null ? AppPath.GetHashCode() : 0) * 397) ^ (Direction != null ? Direction.GetHashCode() : 0);
            }
        }

        public static bool operator ==(EntryAdv left, EntryAdv right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntryAdv left, EntryAdv right)
        {
            return !Equals(left, right);
        }
    }
}
