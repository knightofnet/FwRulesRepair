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
using FwRulesRepair.cst;
using NetFwTypeLib;

namespace FwRulesRepair.business
{
    class RepairWindowApp
    {
        private static Logger log = Logger.LastLoggerInstance;

        public INetFwRule Rule { get; private set; }

        public String AppName { get; private set; }

        public String SubDir { get; private set; }

        public static RepairWindowApp DetectIfMatch(INetFwRule fwRule)
        {
            RepairWindowApp ret = null;

            String appPath = fwRule.ApplicationName;

            if (Cst.WindowsAppRegex.IsMatch(appPath))
            {
                Match m = Cst.WindowsAppRegex.Match(appPath);

                ret = new RepairWindowApp();
                ret.Rule = fwRule;
                ret.AppName = m.Groups["name"].Value;
                ret.SubDir = m.Groups["subDir"].Value;


            }

            return ret;
        }

        public void Treat()
        {

            try
            {

                PackageManager f = new PackageManager();
                Package[] packagesFound = f.FindPackages()
                    .Where(r => r.Id.Name.Equals(AppName, StringComparison.OrdinalIgnoreCase)).ToArray();

                if (packagesFound.Any())
                {
                    String fp = Path.Combine(packagesFound[0].InstalledLocation.Path, SubDir);
                    if (File.Exists(fp))
                    {

                        Rule.ApplicationName = fp;
                    }
                }
            }
            catch (System.UnauthorizedAccessException ex)
            {
                log.Warn("Unauthorized access to update rule. Try running with admin rights");
#if DEBUG
                log.Warn(ex.Message);
                log.Error(ex.StackTrace);
#endif
            }
        }

    }
}
