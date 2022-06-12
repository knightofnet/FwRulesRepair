using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FwRulesRepair.cst;

namespace FwRulesRepair.dto
{
    class VsChangePotential
    {
        public string RuleAppPath { get; set; }

        public Version RuleAppVersion { get; private set; }
        private string strRuleAppVersion;

        public HashSet<Version> OtherVersions { get; }

        public bool IsWithVersion { get; private set; }

        public VsChangePotential()
        {
            OtherVersions = new HashSet<Version>();
        }

        public string NewRuleAppPath { get; private set; }

        public void AddOtherVersion(string vsStr)
        {
            Match m = null;
            if ((m = Cst.VersionRegex.Match(vsStr)).Success)
            {
                OtherVersions.Add(new Version(m.Groups["vs"].Value));

                var maxOtherVs = GetMaxOtherVersion();

                NewRuleAppPath = RuleAppPath.Replace(strRuleAppVersion, maxOtherVs.ToString());
            }
        }

        private Version GetMaxOtherVersion()
        {
            Version retVs = null;


            foreach (Version otherVersion in OtherVersions)
            {
                if (retVs == null || otherVersion > retVs)
                {
                    retVs = otherVersion;
                }
            }

            return retVs;
        }

        public void SetRuleAppVersion(string vsStr)
        {
            Match m = null;
            if ((m = Cst.VersionRegex.Match(vsStr)).Success)
            {
                IsWithVersion = true;
                string version = m.Groups["vs"].Value;
                RuleAppVersion = new Version(version);
                
                strRuleAppVersion = version;
            }
        }




        public override string ToString()
        {
            return $"RuleAppPath: {RuleAppPath}, RuleAppVersion: {RuleAppVersion}, OtherVersions: {string.Join(",", OtherVersions.Select(r=>r.ToString()))}, NewRuleAppPath: {NewRuleAppPath}";
        }

        private sealed class RuleAppPathEqualityComparer : IEqualityComparer<VsChangePotential>
        {
            public bool Equals(VsChangePotential x, VsChangePotential y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.RuleAppPath == y.RuleAppPath;
            }

            public int GetHashCode(VsChangePotential obj)
            {
                return (obj.RuleAppPath != null ? obj.RuleAppPath.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<VsChangePotential> RuleAppPathComparer { get; } = new RuleAppPathEqualityComparer();
    }
}
