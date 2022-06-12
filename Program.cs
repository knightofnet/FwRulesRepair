using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using FwRulesRepair.business;
using FwRulesRepair.utils;
using NetFwTypeLib;
using NLog;

namespace FwRulesRepair
{
    class Program
    {
        private static Logger _log = null;

        static void Main(string[] args)
        {
            InitLogger();

            /*
            string asDir = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            if (!String.IsNullOrWhiteSpace(asDir) && Directory.GetCurrentDirectory() != asDir)
            {
                Directory.SetCurrentDirectory(asDir);
            }
            */
         


            RepairFwRules repInst = new RepairFwRules();
            repInst.DoJob();

#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void InitLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "file.log" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            LogManager.Configuration = config;

            _log = NLog.LogManager.GetCurrentClassLogger();
        }
    }
}
