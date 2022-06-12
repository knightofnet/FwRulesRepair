using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FwRulesRepair.cst;
using NetFwTypeLib;
using NLog;
using UsefulCsharpCommonsUtils.lang;

namespace FwRulesRepair.utils
{
    public static class FwUtils
    {
        private static readonly Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static INetFwRule CreateRuleForRemoteAddresses(string name, NET_FW_RULE_DIRECTION_ direction, ProtocoleEnum protocole, bool isAllow, String description, List<string> adress)
        {
            INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FWRule"));

            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            firewallRule.Protocol = (int)protocole;
            firewallRule.RemoteAddresses = String.Join(",", adress);

            firewallRule.Action = isAllow ? NET_FW_ACTION_.NET_FW_ACTION_ALLOW : NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Description = description;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = name;
            firewallRule.Direction = direction;

            firewallPolicy.Rules.Add(firewallRule);

            return firewallRule;
        }



        public static INetFwRule CreateRuleForProgram(string ruleName, NET_FW_RULE_DIRECTION_ direction, ProtocoleEnum protocole, bool isAllow, string description, string programFilePath)
        {
            INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FWRule"));

            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            firewallRule.Protocol = (int)protocole;
            firewallRule.ApplicationName = programFilePath;
            firewallRule.Action = isAllow ? NET_FW_ACTION_.NET_FW_ACTION_ALLOW : NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            firewallRule.Description = description;
            firewallRule.Enabled = true;
            firewallRule.InterfaceTypes = "All";
            firewallRule.Name = ruleName;
            firewallRule.Direction = direction;

            firewallPolicy.Rules.Add(firewallRule);

            return firewallRule;
        }

        public static void UpdateRuleForRemoteAddresses(string getName, NET_FW_RULE_DIRECTION_ direction,
            ProtocoleEnum getProtocole, List<string> adress)
        {
            INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FWRule"));

            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

            firewallRule = firewallPolicy.Rules.Item(getName);
            if (firewallRule.Direction == direction &&
                firewallRule.Protocol == (int)getProtocole)
            {
                firewallRule.RemoteAddresses = String.Join(",", adress);
            }
        }

        public static INetFwRule GetRule(string getName, ProtocoleEnum getProtocole, NET_FW_RULE_DIRECTION_? direction = null)
        {
            try
            {
                String uniqueS = CommonsStringUtils.RandomString(8, ensureUnique: true);

                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FWRule"));

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));


                foreach (INetFwRule fwRule in firewallPolicy.Rules)
                {

                    if (fwRule.Name != null)
                    {

                        if (fwRule.Name.Equals(getName) && fwRule.Protocol == (int)getProtocole)
                        {
                            //   log.Debug("GetRule n:{0} p:{1} OK ({2})", getName, getProtocole, uniqueS);
                            if (!direction.HasValue ||
                                direction.Value == fwRule.Direction)
                            {
                                //log.Debug("GetRule n:{0} d:{1} OK ({2})", fwRule.Name, fwRule.Direction, fwRule.RemoteAddresses);
                                return fwRule;
                            }
                        }

                    }
                }


                //log.Debug("GetRule {0} {1} {2} => null" + uniqueS, getName, getDirection, getProtocole);
                return null;
            }
            catch (Exception e)
            {
                //ExceptionHandlingUtils.LogAndHideException(e, "GetRule");
                return null;
            }
        }

        public static bool RemoveRule(string getName, NET_FW_RULE_DIRECTION_? direction)
        {
            try
            {
                String uniqueS = CommonsStringUtils.RandomString(8, ensureUnique: true);

                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FWRule"));

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));


                foreach (INetFwRule fwRule in firewallPolicy.Rules)
                {

                    if (fwRule.Name != null)
                    {

                        if (fwRule.Name.Equals(getName))
                        {
                            //   log.Debug("GetRule n:{0} p:{1} OK ({2})", getName, getProtocole, uniqueS);
                            if (!direction.HasValue ||
                                direction.Value == fwRule.Direction)
                            {

                                firewallPolicy.Rules.Remove(fwRule.Name);
                                return true;
                            }
                        }

                    }
                }


                //log.Debug("GetRule {0} {1} {2} => null" + uniqueS, getName, getDirection, getProtocole);
                return false;
            }
            catch (Exception e)
            {
                //ExceptionHandlingUtils.LogAndHideException(e, "RemoveRule");
                return false;
            }
        }

        public static List<INetFwRule> GetRuleNameRegex(string regex, NET_FW_RULE_DIRECTION_? direction)
        {
            List<INetFwRule> retListRules = new List<INetFwRule>();

            try
            {

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                foreach (INetFwRule fwRule in firewallPolicy.Rules)
                {

                    if (!direction.HasValue ||
                        direction.Value == fwRule.Direction)
                    {
                        if (fwRule.Name != null && fwRule.Name.Matches(regex))
                        {
                            retListRules.Add(fwRule);
                        }
                    }
                }

                return retListRules;
            }
            catch (Exception e)
            {
               // ExceptionHandlingUtils.LogAndHideException(e, "GetRuleNameRegex");
                throw e;
            }
        }

        public static bool IsExistsRule(string getName, ProtocoleEnum getProtocole, NET_FW_RULE_DIRECTION_? direction)
        {
            try
            {
                String uniqueS = CommonsStringUtils.RandomString(8, ensureUnique: true);

                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FWRule"));

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                foreach (INetFwRule fwRule in firewallPolicy.Rules)
                {

                    if (fwRule.Name != null)
                    {

                        if (fwRule.Name.Equals(getName) && fwRule.Protocol == (int)getProtocole)
                        {
                            //log.Debug("IsExistRule n:{0} p:{1} OK ({2})", getName, getProtocole, uniqueS);
                            if (!direction.HasValue ||
                                direction.Value == fwRule.Direction)
                            {
                                // log.Debug("IsExistRule n:{0} d:{1} OK ({2})", getName, getDirection, uniqueS);
                                return true;
                            }
                        }

                    }
                }


                log.Debug("IsExistRule {0} {1} {2} " + uniqueS, getName, direction, getProtocole);
                return false;
                /*
                firewallRule = firewallPolicy.Rules.Item(getName);
                if (firewallRule.Direction == DirectionEnumToFwRuleDirection(getDirection) &&
                    firewallRule.Protocol == (int)getProtocole)
                {
                    return true;
                }

                return false;
                */
            }
            catch (Exception e)
            {
                //ExceptionHandlingUtils.LogAndHideException(e, "IsExistsRule");
                return false;
            }
        }

        public static bool IsExistsRuleForProgram(string programFilepath, NET_FW_RULE_DIRECTION_? direction)
        {
            try
            {

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                foreach (INetFwRule fwRule in firewallPolicy.Rules)
                {
                    if (fwRule.ApplicationName != null && fwRule.ApplicationName.Equals(programFilepath, StringComparison.CurrentCultureIgnoreCase))
                    {

                        if (!direction.HasValue ||
                            direction.Value == fwRule.Direction)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                //ExceptionHandlingUtils.LogAndHideException(e, "IsExistsRuleForProgram");

                return false;
            }
        }

        public static INetFwRule GetRuleForProgram(string programFilepath, NET_FW_RULE_DIRECTION_? direction)
        {
            try
            {

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                foreach (INetFwRule fwRule in firewallPolicy.Rules)
                {
                    if (fwRule.ApplicationName != null && fwRule.ApplicationName.Equals(programFilepath, StringComparison.CurrentCultureIgnoreCase))
                    {

                        if (!direction.HasValue ||
                            direction.Value == fwRule.Direction)
                        {
                            return fwRule;
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                //ExceptionHandlingUtils.LogAndHideException(e, "GetRuleForProgram");

                return null;
            }

        }

        internal static IEnumerable<INetFwRule> GetRulesWithProgram(NET_FW_RULE_DIRECTION_? direction)
        {
            List<INetFwRule> retList = new List<INetFwRule>();
            try
            {

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                foreach (INetFwRule fwRule in firewallPolicy.Rules)
                {
                    if (!String.IsNullOrWhiteSpace(fwRule.ApplicationName))
                    {

                        if (!direction.HasValue ||
                            direction.Value == fwRule.Direction)
                        {
                            retList.Add(fwRule);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                //ExceptionHandlingUtils.LogAndHideException(e, "GetRulesWithProgram");

            }

            return retList.ToArray();
        }

        internal static INetFwRule[] GetRulesForProgram(string programFilepath, NET_FW_RULE_DIRECTION_? direction)
        {
            List<INetFwRule> retList = new List<INetFwRule>();
            try
            {

                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));

                foreach (INetFwRule fwRule in firewallPolicy.Rules)
                {
                    if (fwRule.ApplicationName != null && fwRule.ApplicationName.Equals(programFilepath, StringComparison.CurrentCultureIgnoreCase))
                    {

                        if (!direction.HasValue ||
                            direction.Value == fwRule.Direction)
                        {
                            retList.Add(fwRule);
                        }
                    }
                }

            }
            catch (Exception e)
            {
               //ExceptionHandlingUtils.LogAndHideException(e, "GetRuleForProgram");

            }

            return retList.ToArray();
        }

    


    }
}
