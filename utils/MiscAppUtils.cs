using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AryxDevLibrary.utils;

namespace FwRulesRepair.utils
{
    public static class MiscAppUtils
    {

        public static bool IsValidFilepath(String filePath)
        {
            if (StringUtils.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            if (filePath.IndexOfAny(Path.GetInvalidPathChars()) != -1 || !Path.IsPathRooted(filePath))
            {
                return false;
            }





            return true;
        }

    }
}
