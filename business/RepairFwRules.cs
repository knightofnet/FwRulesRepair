using System;
using System.Collections.Generic;
using System.IO;

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using AryxDevLibrary.utils.logger;
using FwRulesRepair.business.repairs;
using FwRulesRepair.cst;
using FwRulesRepair.utils;
using NetFwTypeLib;

namespace FwRulesRepair.business
{
    class RepairFwRules
    {
        private static Logger log = Logger.LastLoggerInstance;

        public void DoJob()
        {
            IEnumerable<INetFwRule> rules = FwUtils.GetRulesWithProgram(NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);

            foreach (INetFwRule rule in rules)
            {
                CureRule(rule);
            }
        }

        private static void CureRule(INetFwRule rule)
        {
            String appPath = rule.ApplicationName;
            bool isGoTreat = true;

            if (!MiscAppUtils.IsValidFilepath(appPath))
            {
                log.Debug("Invalid filepath: {0}", appPath);
                isGoTreat = false;
            }


            if (!isGoTreat) return;

            log.Info("Check rule for {0}", appPath);

            // WindowsApp
            AbstractCanRepair r = new RepairWindowApp();
            if (r.DetectIfMatch(rule))
            {
                log.Info("Is windowsApp. Try autoRepair...");
                r.Treat();
                return;
            }

            // Discord
            AbstractCanRepair rd = new RepairGeneric($@"discord\app-{PathUtils.VersionMajMinBuild}\discord.exe")
            {
                IsFwAppMustNotExist = false
            };
            if (rd.DetectIfMatch(rule))
            {
                log.Info("Is Discord. Try autoRepair...");
                rd.Treat();
                return;
            }




        }
    }
}
