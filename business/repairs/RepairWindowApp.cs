using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

using FwRulesRepair.cst;
using NetFwTypeLib;
using NLog;

namespace FwRulesRepair.business.repairs
{
    class RepairWindowApp : AbstractCanRepair
    {
       private static readonly Logger log = NLog.LogManager.GetCurrentClassLogger();



        public String AppName { get; private set; }

        public String SubDir { get; private set; }

        public RepairWindowApp() : base(true)
        {

        }

        public override bool DetectIfMatch(INetFwRule fwRule)
        {
            if (!base.DetectIfMatch(fwRule))
            {
                return false;
            }

            bool ret = false;

            String appPath = fwRule.ApplicationName;

            if (Cst.WindowsAppRegex.IsMatch(appPath))
            {
                Match m = Cst.WindowsAppRegex.Match(appPath);

                Rule = fwRule;
                AppName = m.Groups["name"].Value;
                SubDir = m.Groups["subDir"].Value;

                ret = true;
            }

            return ret;
        }

        public override void Treat()
        {

            try
            {

                PackageManager f = new PackageManager();
                Package[] packagesFound = f.FindPackages()
                    .Where(r => r.Id.Name.Equals(AppName, StringComparison.OrdinalIgnoreCase)).ToArray();

                if (packagesFound.Any())
                {
                    Package pf = null;
                    foreach (Package p in packagesFound)
                    {
                        try
                        {
                            log.Error(p.InstalledLocation.ToString());
                            if (p.InstalledLocation != null && Directory.Exists(p.InstalledLocation.Path))
                            {
                                pf = p;
                                break;
                            }
                        }
                        catch (FileNotFoundException fex)
                        {
                            continue;
                        }
                    }

                    if (pf == null) return;

                    String fp = Path.Combine(pf.InstalledLocation.Path, SubDir);
                    if (File.Exists(fp))
                    {

                        Rule.ApplicationName = fp;
                    }
                }
                else
                {
                    log.Warn("Package not found. Unable to repair");
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
