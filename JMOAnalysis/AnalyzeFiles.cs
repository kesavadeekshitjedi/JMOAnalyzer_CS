using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using log4net;
using log4net.Config;

namespace JMOAnalysis
{
    class AnalyzeFiles
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        string stationDef = "DEFINE STATION ID=";
        string jobsetDef = "DEFINE JOBSET ID=";
        string jobDef = "DEFINE JOB ID=";
        string jobsetPredDef = "DEFINE JOBSETPRED ID=";
        string jobPredDef = "DEFINE JOBPRED ID=";
        string jobResDef = "DEFINE JOBRES ID=";
        string triggerDef = "DEFINE TRIGGER ID=";
        string resourceDef = "DEFINE RESOURCE ID=";
        StreamReader t4BaseFileReader;
        string t4BaseFile = @"C:\JMOFiles\US_CA_NSM_JMO_TRANCHE-4_CODE_v4.txt";
        public static void createJMOObjectReport()
        {
            AnalyzeFiles az = new AnalyzeFiles();

            List<string> jobList = new List<string>();
            List<string> jobsetList = new List<string>();
            List<string> stationList = new List<string>();
            List<string> resourcesList = new List<string>();
            List<string> missingStationsList = new List<string>();
            List<string> missingResourcesList = new List<string>();
            
            string tempString = "";
            string[] tempStringTuple;
            int startIndex = 0;
            int endIndex = 0;
            string tempJobName = "";
            string tempJobNumber = "";
            string tempJobsetName = "";
            string tempStationName = "";
            string tempResourceName = "";
            string tempTriggerName = "";


            logger.Info("Reading JMO Extract to create Object Report");
            // Print the following:
            // Total Number of Jobsets:
            // Total Number of Jobs:
            // Total Number of Triggers:
            // Total Number of stations:
            // Total number of Resources:
            // Total number of Missing Stations:
            // Total Number of Missing Resources:
            // Total Number of Missing Predecessors:
            // Jobs per Machine as follows:
            // Machine1 (count)
            // Jobset hierarcy as follows:
            // Jobset (count)
            //    Job1
            //    Job2
            az.t4BaseFileReader = new StreamReader(az.t4BaseFile);
            string currentT4Line = "";
            while ((currentT4Line = az.t4BaseFileReader.ReadLine()) != null)
            {
                logger.Debug(currentT4Line);
                if (currentT4Line.Contains(az.jobDef))
                {
                    logger.Debug("Job Definition Line found:");
                    startIndex = currentT4Line.IndexOf(az.jobDef) + az.jobDef.Length;
                    endIndex = currentT4Line.IndexOf("FAILCOND");
                    tempString = currentT4Line.Substring(startIndex, endIndex - startIndex).Trim();
                    tempStringTuple = currentT4Line.Split(',');
                    tempJobName = tempStringTuple[1].Trim() + "," + tempStringTuple[2].Replace(")", "").Trim();
                    logger.Info("Adding  to jobList :" + tempJobName);
                    jobList.Add(tempJobName);

                }
                if (currentT4Line.Contains(az.jobsetDef))
                {
                    logger.Debug("Jobset Definition Line found:");
                    startIndex = currentT4Line.IndexOf(az.jobsetDef) + az.jobsetDef.Length;
                    endIndex = currentT4Line.IndexOf("FAILCOND");
                    tempString = currentT4Line.Substring(startIndex, endIndex - startIndex).Trim();

                    logger.Info("Adding  to jobsetList :" + tempJobsetName);
                    jobsetList.Add(tempJobName);

                }
                if (currentT4Line.Contains(az.triggerDef))
                {
                    logger.Debug("Trigger Definition Line found:");
                    startIndex = currentT4Line.IndexOf(az.triggerDef) + az.triggerDef.Length;
                    if (currentT4Line.Contains("DESCRIPTION"))
                    {
                        endIndex = currentT4Line.IndexOf("DESCRIPTION");
                    }
                    else if (currentT4Line.Contains("CRITKEYS"))
                    {
                        endIndex = currentT4Line.IndexOf("CRITKEYS");
                    }
                    else if (currentT4Line.Contains("STATION"))
                    {
                        endIndex = currentT4Line.IndexOf("STATION");
                    }
                    tempString = currentT4Line.Substring(startIndex, endIndex - startIndex).Trim();

                    logger.Info("Adding  to jobsetList :" + tempJobsetName);
                    jobsetList.Add(tempJobName);

                }
                if(currentT4Line.Contains(az.stationDef))
                {
                    logger.Debug("Station Definition Found");
                    startIndex = currentT4Line.IndexOf(az.stationDef) + az.stationDef.Length;
                    endIndex = currentT4Line.IndexOf("NODE");
                    string[] tempStationTuple = currentT4Line.Substring(startIndex, endIndex - startIndex).Split(',');
                    stationList.Add(tempStationTuple[0].Replace("(", "") + ":" + tempStationTuple[1].Replace(")", ""));

                }
                if(currentT4Line.Contains(az.jobResDef))
                {
                    logger.Debug("Resource Definition found");
                    startIndex = currentT4Line.IndexOf(az.resourceDef) + az.resourceDef.Length;
                    endIndex = currentT4Line.IndexOf("DESCRIPTION");
                    string[] tempResTuple = currentT4Line.Substring(startIndex, endIndex - startIndex).Split(',');
                    resourcesList.Add(tempResTuple[0].Replace("(", "") + ":" + tempResTuple[1].Replace(")", ""));
                }
            }

        }
    }
}
