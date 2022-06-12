using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FwRulesRepair.utils;
using NetFwTypeLib;
using NLog;
using UsefulCsharpCommonsUtils.file;
using UsefulCsharpCommonsUtils.file.dir;

namespace FwRulesRepair.business.repairs
{
    public class RepairGeneric : AbstractCanRepair
    {
        private static readonly Logger log = NLog.LogManager.GetCurrentClassLogger();
        public INetFwRule Rule { get; private set; }
        public string FilePathMask { get; private set; }
        public string MaskDetected { get; private set; }

        public Version OriginaleVersion { get; private set; }

        public RepairGeneric(string filepathMask) : base(true)
        {
            FilePathMask = filepathMask;
        }

        public override bool DetectIfMatch(INetFwRule fwRule)
        {
            if (!base.DetectIfMatch(fwRule))
            {
                return false;
            }

            string appPath = fwRule.ApplicationName;

            String mask;
            if (PathUtils.IsMatchMask(appPath, FilePathMask, out mask))
            {
                MaskDetected = mask;
                OriginaleVersion = PathUtils.ExtractVersionFromString(appPath, mask);
                Rule = fwRule;
                return true;
            }

            return false;

        }

        public override void Treat()
        {
            int ixMatch = PathUtils.IndexOfMask(Rule.ApplicationName, MaskDetected);
            if (ixMatch == -1)
            {
                log.Error("No mask finally");
                return;
            }

            String subDir = Rule.ApplicationName.Substring(0, ixMatch);
            if (!subDir.Substring(0, ixMatch).EndsWith(@"\"))
            {
                subDir = Directory.GetParent(subDir).FullName;
            }
 
            
            DirectoryInfo dirD = new DirectoryInfo(subDir);
            if (!dirD.Exists)
            {
                log.Error("{0} doesnt exist anymore", subDir);
            }

            List<FileInfo> list = Dir.Children(dirD, true).OfType<FileInfo>().Where(r => PathUtils.IsMatchMask(r.FullName, FilePathMask)).ToList();
            foreach (FileInfo matching in list)
            {
                log.Debug(matching.FullName);
            }
        }
    }
}