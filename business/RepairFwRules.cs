using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

using FwRulesRepair.business.repairs;
using FwRulesRepair.cst;
using FwRulesRepair.dto;
using FwRulesRepair.utils;
using NetFwTypeLib;
using NLog;
using UsefulCsharpCommonsUtils.lang.ext;

namespace FwRulesRepair.business
{
    class RepairFwRules
    {
        private static readonly Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public void DoJob()
        {

            Task<HashSet<EntryAdv>> taskGetEntryAdv = Task.Factory.StartNew(GetEntryAdvAsync);


            Task<List<INetFwRule>> taskGetRulesOut =
                Task.Factory.StartNew(() =>
                {
                    var innerRules = FwUtils.GetRulesWithProgram(DirectionsEnum.Outbound.INetFwRuleDirection);

                    List<INetFwRule> retList = new List<INetFwRule>();

                    foreach (INetFwRule fwRule in innerRules)
                    {
                        if (!MiscAppUtils.IsValidFilepath(fwRule.ApplicationName))
                        {
                            // log.Debug("Invalid filepath: {0}", fwRule.ApplicationName);

                        }
                        else
                        {
                            //log.Debug("Valid filepath: {0}", fwRule.ApplicationName);
                            retList.Add(fwRule);
                        }
                    }

                    return retList;

                });

            Task<List<INetFwRule>> taskGetRulesIn =
                Task.Factory.StartNew(() =>
                {
                    var innerRules = FwUtils.GetRulesWithProgram(DirectionsEnum.Inboud.INetFwRuleDirection);

                    List<INetFwRule> retList = new List<INetFwRule>();

                    foreach (INetFwRule fwRule in innerRules)
                    {
                        if (!MiscAppUtils.IsValidFilepath(fwRule.ApplicationName))
                        {
                            // log.Debug("Invalid filepath: {0}", fwRule.ApplicationName);

                        }
                        else
                        {
                            //log.Debug("Valid filepath: {0}", fwRule.ApplicationName);
                            retList.Add(fwRule);
                        }
                    }

                    return retList;

                });

            Task.WaitAll(taskGetRulesOut, taskGetRulesIn, taskGetEntryAdv);

            HashSet<EntryAdv> listEntry = taskGetEntryAdv.Result;
            List<INetFwRule> rulesOut = taskGetRulesOut.Result;
            List<INetFwRule> rulesIn = taskGetRulesIn.Result;


            Log.Info("These app have requested been blocked by the Fw :");
            foreach (EntryAdv entryAdv in listEntry)
            {
                Log.Info($"- Direction {entryAdv.Direction.Libelle}: {entryAdv.AppPath}");
            }


            Console.ReadLine();

            VersionCure(rulesOut, listEntry, DirectionsEnum.Outbound);
            VersionCure(rulesIn, listEntry, DirectionsEnum.Inboud);

            /*
            IEnumerable<INetFwRule> rules = FwUtils.GetRulesWithProgram(NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT);

            foreach (INetFwRule rule in rules)
            {
                CureRule(rule);
            }

            */
        }

        private static void VersionCure(List<INetFwRule> rules, HashSet<EntryAdv> listEntry, DirectionsEnum dir)
        {

            Log.Info($"Traitment direction {dir.Libelle}");

            HashSet<VsChangePotential> finalList = new HashSet<VsChangePotential>();

            foreach (INetFwRule netFwRule in rules)
            {
                string appPath = netFwRule.ApplicationName;
                if (Cst.VersionRegex.IsMatch(appPath)) // !File.Exists(appPath) )
                {
                    //log.Info($"Règle avec application invalide: {appPath}");

                    VsChangePotential vsChangePotential = finalList.FirstOrDefault(o => o.RuleAppPath.Equals(appPath)) ??
                                          new VsChangePotential();
                    vsChangePotential.RuleAppPath = appPath;


                    foreach (EntryAdv entryAdv in listEntry.Where(d => d.Direction == dir))
                    {
                        string appPathEntry = entryAdv.AppPath;

                        string[] appPathA = appPath.ToLower().Split('\\');
                        string[] appPathEntryA = appPathEntry.ToLower().Split('\\');

                        string[] diffSitePathEntry = appPathEntryA.Except(appPathA).ToArray();

                        if (diffSitePathEntry.Length == 1
                            && appPathA.Last().Trim() == appPathEntryA.Last().Trim())
                        {
                            Log.Info("Règle avec application invalide: ");
                            Log.Info($"> RegleAppPath => {appPath}");
                            Log.Info($"> EventAppPath => {appPathEntry}");
                            Log.Info($"> Difference => {diffSitePathEntry[0]}");


                            vsChangePotential.SetRuleAppVersion(appPathA.Except(appPathEntryA).ToArray()[0]);

                            vsChangePotential.AddOtherVersion(diffSitePathEntry[0]);
                        }
                        else
                        {
                            //log.Info($"diff> {string.Join(",", diff)}");
                        }

                        if (vsChangePotential.IsWithVersion)
                        {
                            finalList.Add(vsChangePotential);
                        }
                    }
                }
            }

            Log.Debug(string.Join(@"\n\n", finalList.Select(r => r.ToString())));


            if (finalList.Any())
            {
                foreach (VsChangePotential vsChangePotential in finalList.Where(r => File.Exists(r.NewRuleAppPath)))
                {
                    INetFwRule retApp = FwUtils.GetRuleForProgram(vsChangePotential.RuleAppPath,
                        dir.INetFwRuleDirection);

                    if (retApp != null && !FwUtils.IsExistsRuleForProgram(vsChangePotential.NewRuleAppPath,
                            dir.INetFwRuleDirection))
                    {
                        retApp.ApplicationName = vsChangePotential.NewRuleAppPath;
                        Log.Info($"{retApp.Name} cure with {vsChangePotential.NewRuleAppPath}");
                    }
                }
            }


            Log.Info($"FIN - Traitment direction {dir.Libelle}");
        }

        private HashSet<EntryAdv> GetEntryAdvAsync()
        {
            EventLog journal = new EventLog("Security");

            var entries = journal.Entries.Cast<EventLogEntry>().Where(r => FilterEntry(r, new long[] { 5157 })).ToList();


            //log.Info($"LL: {journal.Entries.Cast<EventLogEntry>()}");

            HashSet<EntryAdv> listAdv = new HashSet<EntryAdv>();
            foreach (EventLogEntry eventLogEntry in entries)
            {
                // log.Debug($"Event: {eventLogEntry.TimeGenerated}, {string.Join(",", eventLogEntry.ReplacementStrings)}");
                EntryAdv a = new EntryAdv(eventLogEntry)
                {
                    Direction = eventLogEntry.GetDirection()
                };

                if (a.AppPath != null && listAdv.Add(a))
                {
                    //log.Debug($"Add {a.AppPath}");
                }
            }

            return listAdv;
        }

        public bool FilterEntry(EventLogEntry entry, long[] entryId, String exeName = null)
        {

            //log.Debug($"# event {entry.TimeGenerated}, {entry.InstanceId}");

            if (!entryId.Contains(entry.InstanceId))
            {
                // log.Error("rejectec Type");
                return false;
            }

            DateTime dtMin = DateTime.Now.AddHours(-10);
            if (entry.TimeGenerated.IsBefore(dtMin))
            {
                // log.Error("rejectec Time");
                return false;
            }

            /*
            if (entry.GetDirection() != DirectionsEnum.Outbound)
            {
                //log.Error($"rejectec Direction {entry.GetDirection().Libelle}");
                return false;
            }
            */

            if (exeName != null && !entry.ReplacementStrings[1].EndsWith(exeName, StringComparison.CurrentCultureIgnoreCase))
            {
                // log.Error("rejectec App");
                return false;
            }

            return true;
        }





        private static void CureRule(INetFwRule rule)
        {
            String appPath = rule.ApplicationName;
            bool isGoTreat = true;

            if (!MiscAppUtils.IsValidFilepath(appPath))
            {
                Log.Debug("Invalid filepath: {0}", appPath);
                isGoTreat = false;
            }


            if (!isGoTreat) return;

            Log.Info("Check rule for {0}", appPath);

            // WindowsApp
            AbstractCanRepair r = new RepairWindowApp();
            if (r.DetectIfMatch(rule))
            {
                Log.Info("Is windowsApp. Try autoRepair...");
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
                Log.Info("Is Discord. Try autoRepair...");
                rd.Treat();
                return;
            }




        }
    }
}
