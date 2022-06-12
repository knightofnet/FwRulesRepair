using System.IO;
using AryxDevLibrary.utils.logger;
using NetFwTypeLib;

namespace FwRulesRepair.business
{
    public abstract class AbstractCanRepair
    {
        private static Logger _log = Logger.LastLoggerInstance;

        public bool IsFwAppMustNotExist { get; set; }
        protected AbstractCanRepair(bool isFwAppMustNotExist)
        {
            IsFwAppMustNotExist = isFwAppMustNotExist;

        }



        public INetFwRule Rule { get; protected set; }

        public virtual bool DetectIfMatch(INetFwRule fwRule)
        {
            if (IsFwAppMustNotExist && File.Exists(fwRule.ApplicationName))
            {
                _log.Debug("Existing and correct filepath: {0}, nothing to repair", fwRule.ApplicationName);
                return false;
            }

            return true;
        }
        public abstract void Treat();
    }
}