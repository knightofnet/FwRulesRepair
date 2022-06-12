using System;
using System.Diagnostics;
using FwRulesRepair.cst;
using NLog;

namespace FwRulesRepair.utils
{
    public static class EventEntryExtension
    {
        private static readonly Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static DirectionsEnum GetDirection(this EventLogEntry entry)
        {
            if (entry.InstanceId != 5157L && entry.InstanceId != 5152L)
            {
                //log.Error($" > KO entry instance Id != 5157 ({entry.InstanceId})");
                return DirectionsEnum.NULL;
            }
            //log.Debug($" > OK entry instance Id == 5157/2 ({entry.InstanceId})");

            DirectionsEnum dir = DirectionsEnum.FromEntryDirection(entry);
            if (dir != null)
            {
                //log.Debug($" > OK entry dir ({dir.Libelle})");
                return dir;
            }

            //log.Error($" > KO entry dir Null ({entry.ReplacementStrings[2]})");
            return DirectionsEnum.NULL;
        }

        public static ProtocoleEnum GetProtocole(this EventLogEntry entry)
        {
            if (entry.InstanceId != 5157 && entry.InstanceId != 5152)
            {

                return ProtocoleEnum.NULL;
            }

            int intProtocole = 0;
            if (Int32.TryParse(entry.ReplacementStrings[7], out intProtocole))
            {
                switch (intProtocole)
                {
                    case 6:
                        return ProtocoleEnum.TCP;
                    case 17:
                        return ProtocoleEnum.UDP;
                    default:
                        return ProtocoleEnum.NULL;
                }
            }

            return ProtocoleEnum.NULL;
        }

        public static String GetRemoteAddress(this EventLogEntry entry)
        {
            if (entry.InstanceId != 5157 && entry.InstanceId != 5152)
            {
                return null;
            }

            return entry.ReplacementStrings[5];
        }
    }
}
