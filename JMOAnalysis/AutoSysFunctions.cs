using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using System.IO;

namespace JMOAnalysis
{
    class AutoSysFunctions
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static void connectToOracle(string dbHost, string dbPort, string dbSID, string userName, string password)
        {
            String connectionString="host="+dbHost+";database="
        }
        public static string getAutoSysJobCommand(string autosysJobName)
        {
            string jobCommand = "";

            return jobCommand;
        }
        public static string getAutoSysJobCondition(string autosysJobName)
        {
            string jobCondition = "";


            return jobCondition;

        }

        
    }
}
