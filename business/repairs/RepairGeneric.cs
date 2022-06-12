using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using FwRulesRepair.utils;
using NetFwTypeLib;

namespace FwRulesRepair.business.repairs
{
    public class RepairGeneric : AbstractCanRepair
    {
        private static Logger _log = Logger.LastLoggerInstance;
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
                _log.Error("No mask finally");
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
                _log.Error("{0} doesnt exist anymore", subDir);
            }

            List<FileInfo> list = FileUtils.GetFilesRecursively(dirD).Where(r => PathUtils.IsMatchMask(r.FullName, FilePathMask)).ToList();
            foreach (FileInfo matching in list)
            {
                _log.Debug(matching.FullName);
            }
        }
    }
}