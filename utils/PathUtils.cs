using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FwRulesRepair.utils
{
    public static class PathUtils
    {
        public const string VersionMajMinBuild = "[version.major-minor-build]";
        public const string VersionMajMinBuildRev = "[version.major-minor-build-rev]";

        private static readonly Dictionary<String, String> MaskDictionary = new Dictionary<string, string>()
        {
            {VersionMajMinBuild, @"(?'major'\d{1,})[\.-_](?'minor'\d{1,})[\.-_](?'build'\d{1,})"},
            {VersionMajMinBuildRev, @"(?'major'\d{1,})[\.-_](?'minor'\d{1,})[\.-_](?'build'\d{1,})[\.-_](?'rev'\d{1,})"},
        };

        public static Version ExtractVersionFromString(String str, String versionType = VersionMajMinBuild)
        {
            var entry = MaskDictionary[versionType];
            Regex r = new Regex(entry);
            Match m = r.Match(str);
            if (m.Success)
            {
                Version v = null;

                if (versionType.Equals(VersionMajMinBuild))
                {
                    v = new Version(Int32.Parse(m.Groups["major"].Value), Int32.Parse(m.Groups["minor"].Value),
                        Int32.Parse(m.Groups["build"].Value));
                } else if (versionType.Equals(VersionMajMinBuildRev))
                {
                    v = new Version(Int32.Parse(m.Groups["major"].Value), Int32.Parse(m.Groups["minor"].Value),
                        Int32.Parse(m.Groups["build"].Value), Int32.Parse(m.Groups["rev"].Value));
                }

                return v;
            }

            return null;
        }

        public static int IndexOfMask(string filepath, string mask)
        {
            try
            {
                String regexParth = MaskDictionary[mask];
                //String regex = StringWithMaskToRegex(filepath, mask, regexParth);

                Regex r = new Regex(regexParth);
                Match m = r.Match(filepath);

                if (m.Success)
                {
                    return m.Groups[0].Index;

                }
            }
            catch (Exception ex)
            {
                
                return -1;
            }

            return -1;
        }

        public static bool IsMatchMask(String filePath, String filePathWithMask)
        {
            return IsMatchMask(filePath, filePathWithMask, out _);
        }
        
        public static bool IsMatchMask(String filePath, String filePathWithMask, out String mask)
        {
            mask = null;
            foreach (KeyValuePair<string, string> regexByMask in MaskDictionary)
            {
                if (filePathWithMask.Contains(regexByMask.Key))
                {
                    String regex = StringWithMaskToRegex(filePathWithMask, regexByMask.Key, regexByMask.Value);
                    if (Regex.IsMatch(filePath, regex))
                    {
                        mask = regexByMask.Key;
                        return true;
                    }
                }
            }


            return false;
        }

        public static String StringWithMaskToRegex(String chaine, String mask, String regexStr)
        {
            //String chaine = @"discord\app-[version.major-minor-release]\discord.exe";
            //String filePathWithMask = "[version.major-minor-release]";
            //String regexStr = @"(?'major'\d{1,})[\.-_](?'minor'\d{1,})[\.-_](?'release'\d{1,})";

            StringBuilder tmp = new StringBuilder(chaine.Length);

            int startIx = 0;

            int ix = chaine.IndexOf(mask, startIx, StringComparison.Ordinal);
            while (ix > -1)
            {
                String subStr = chaine.Substring(startIx, ix - startIx);
                subStr = Regex.Escape(subStr);
                tmp.Append(subStr);

                tmp.Append(regexStr);

                startIx = ix + mask.Length;
                ix = chaine.IndexOf(mask, startIx, StringComparison.Ordinal);

                if (ix == -1)
                {
                    subStr = chaine.Substring(startIx);
                    tmp.Append(Regex.Escape(subStr));
                }

            }

            return tmp.ToString();
        }

 
    }
}
