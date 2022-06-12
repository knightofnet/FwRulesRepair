using System.Collections.Generic;
using System.Diagnostics;
using NetFwTypeLib;

namespace FwRulesRepair.cst
{
    public class DirectionsEnum
    {

        public static readonly DirectionsEnum NULL = new DirectionsEnum(0, "null", "", null);
        public static readonly DirectionsEnum Inboud = new DirectionsEnum(1, "inbound", "%%14592", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN);
        public static readonly DirectionsEnum Outbound = new DirectionsEnum(2, "outbound", "%%14593", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);
        //public static readonly DirectionsEnum Both = new DirectionsEnum(3, new[] { "%%14593", "%%14592" });

        public static IEnumerable<DirectionsEnum> Values
        {
            get
            {
                yield return NULL;
                yield return Inboud;
                yield return Outbound;
               // yield return Both;


            }
        }


        public int Index { get; }
        public string CstEntry { get; }
        public string Libelle { get;  }

        public NET_FW_RULE_DIRECTION_? INetFwRuleDirection { get; }

        private DirectionsEnum(int index, string libelle, string cstEntry, NET_FW_RULE_DIRECTION_? inetDirection)
        {

            Index = index;
            CstEntry = cstEntry;
            Libelle = libelle;
            INetFwRuleDirection = inetDirection;

        }

    


        public static DirectionsEnum FromEntryDirection(EventLogEntry entry)
        {
            string val = entry.ReplacementStrings[2];
            if (val == null) return null;

            foreach (DirectionsEnum directionsEnum in Values)
            {
                if (directionsEnum.CstEntry.Equals(val))
                {
                    return directionsEnum;
                }
            }

            return null;
        }


        private sealed class IndexEqualityComparer : IEqualityComparer<DirectionsEnum>
        {
            public bool Equals(DirectionsEnum x, DirectionsEnum y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Index == y.Index;
            }

            public int GetHashCode(DirectionsEnum obj)
            {
                return obj.Index;
            }
        }

        public static IEqualityComparer<DirectionsEnum> IndexComparer { get; } = new IndexEqualityComparer();
    }
}
