using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AryxDevLibrary.utils.logger;
using FwRulesRepair.business;
using FwRulesRepair.utils;
using NetFwTypeLib;

namespace FwRulesRepair
{
    class Program
    {
        private static Logger _log = null;

        static void Main(string[] args)
        {
            /*
            string asDir = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            if (!String.IsNullOrWhiteSpace(asDir) && Directory.GetCurrentDirectory() != asDir)
            {
                Directory.SetCurrentDirectory(asDir);
            }
            */
            _log = new Logger( "log.log", Logger.LogLvl.DEBUG, Logger.LogLvl.DEBUG, "1 Mo",
                "main");


            RepairFwRules repInst = new RepairFwRules();
            repInst.DoJob();

#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
