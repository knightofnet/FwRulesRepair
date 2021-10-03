using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FwRulesRepair.cst
{
    class Cst
    {

        public static readonly Regex WindowsAppRegex = new Regex(@"\\[wW]indows[aA]pps\\(?'name'.+?)_(?'version'.+?)_(?'pf'.*?)_(?'architecture'.*?)_(?'publisherid'.*?)\\(?'subDir'.+?)$");

    }
}
