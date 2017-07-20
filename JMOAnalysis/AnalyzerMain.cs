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
    class AnalyzerMain
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static String t2JobFile = @"C:\JMOFiles\CombinedPhase2File.txt";
        static String t2JobsetFile = @"C:\JMOFiles\T2_Jobset.txt";
        static String t4JobFile = @"C:\JMOFiles\T4_Jobs.txt";
        static String t4JobsetFile = @"C:\JMOFiles\T4_Jobset.txt";
        static String t4JobPredFile = @"C:\JMOFiles\T4_Jobpred.txt";
        static String t4JobsetPredFile = @"C:\JMOFiles\T4_Jobsetpred.txt";
        static String t4TriggerFile = @"C:\JMOFiles\T4_Triggers.txt";
        static String t4BaseFile = @"C:\JMOFiles\US_CA_NSM_JMO_TRANCHE-4_CODE_v4.txt";

        static string stationDef = "DEFINE STATION ID=";
        static string jobsetDef = "DEFINE JOBSET ID=";
        static string jobDef = "DEFINE JOB ID=";
        static string jobsetPredDef = "DEFINE JOBSETPRED ID=";
        static string jobPredDef = "DEFINE JOBPRED ID=";
        static string jobResDef = "DEFINE JOBRES ID=";
        static string triggerDef = "DEFINE TRIGGER ID=";

        static StreamReader jobsFileReader;
        static StreamReader jobsetsFileReader;
        static StreamReader t2JobFileReader;
        static StreamReader t2JobsetFileReader;
        static StreamReader t4PredJobFileReader;
        static StreamReader t4PredJobsetFileReader;
        static StreamReader t4JobFileReader;
        static StreamReader t4JobsetFileReader;
        static StreamReader triggerFileReader;
        static StreamReader t4BaseFileReader;

        static List<String> jmoJobs = new List<String>();
        static List<String> jmoJobsets = new List<string>();
        static List<String> jmoTriggers = new List<string>();
        static List<string> jmoStations = new List<string>();
        static List<string> jmoResources = new List<string>();

        static List<string> uniqueJobList = new List<string>();
        static Dictionary<string, List<string>> jobMachineMap = new Dictionary<string, List<string>>();
        static Dictionary<string, List<string>> jobHierarchyMap = new Dictionary<string, List<string>>();

        static Dictionary<string, List<string>> jobPredecessorMap = new Dictionary<string, List<string>>();
        static List<string> jobPredecessorList = new List<string>();
        static Dictionary<string, List<string>> jobsetPredecessorMap = new Dictionary<string, List<string>>();
        static List<string> jobsetPredecessorList = new List<string>();

        static List<string> badPredecessorList = new List<string>();
        static string jobType = "";
        

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            AutoSysFunctions af = new AutoSysFunctions();
            af.connectToOracle("LUMOS","1512","SP3","aedbadmin","Test1234");
            

            Console.WriteLine("1. Analyze JMO Extract file");
            Console.WriteLine("2. Run JMO Conversion ");
            Console.WriteLine(" Enter your choice: ");
            string userChoice = Convert.ToString(Console.ReadLine());
            logger.Info("You selected option: " + userChoice);
            ReadCrossRef rc = new ReadCrossRef();
            //ReadCrossRef rc = new ReadCrossRef();
            //rc.createMergedSheet(@"C:\JMOFiles\T2_CrossRefv1.csv", @"C:\JMOFiles\T2_CrossRef_CA.csv", @"C:\JMOFiles\T2_CombinedCrossRef.xlsx");
            AnalyzerMain.readJobsetPredecessors();
            Console.WriteLine("Done reading jobset predecessor Info");
            AnalyzerMain.readJobPredecessors();
            Console.WriteLine("Done reading job predecessor info");



            logger.Info("Check if jobset predecessors exist");
            foreach(var kvPair in jobPredecessorMap)
            {
                logger.Debug("Getting values for " + kvPair.Key);
                List<string> keyValues = kvPair.Value;
                //logger.Info(keyValues);
                logger.Info("Number of elements in the List: " + keyValues.Count);
                if (kvPair.Key.Equals("(ns_gstp_dly_data_retention,nj_gstp_dr_dly_psn,0010)"))
                {
                    logger.Debug("Found anomaly");
                }
                foreach (var element in keyValues)
                {
                    String[] valueTuple = element.Split('@');
                    if(valueTuple.Length==3)
                    {
                        if(kvPair.Key.Equals("(ns_gstp_dly_data_retention,nj_gstp_dr_dly_psn,0010)"))
                        {
                            logger.Debug("Found anomaly");
                        }
                        jobType = "JOB";
                        string checkString = "DEFINE JOB ID=(" + valueTuple[1] + "," + valueTuple[0] + "," + valueTuple[2] + ")";
                        logger.Debug("Checking for " + checkString);
                        bool doesExist=AnalyzerMain.checkIfPredExistsInT4(checkString, jobType);
                        if(doesExist==false)
                        {
                            badPredecessorList.Add(checkString+"=="+kvPair.Key);
                        }
                    }
                    if(valueTuple.Length==2)
                    {
                        jobType = "TRIGGER";
                        string checkString = "DEFINE TRIGGER ID=(" + valueTuple[1] + "," + valueTuple[0] + ")";
                        logger.Debug("Checking for " + checkString + " in the Triggers File");
                        bool doesExist = AnalyzerMain.checkIfPredExistsInT4(checkString, jobType);
                        if (doesExist == false)
                        {
                            badPredecessorList.Add(checkString + "==" + kvPair.Key);
                        }
                    }
                    if(valueTuple.Length==1)
                    {
                        jobType = "JOBSET";
                        string checkString = "DEFINE JOBSET ID=" + valueTuple[0];
                        logger.Debug("Checking for " + checkString + " in the Jobsets File");
                        bool doesExist = AnalyzerMain.checkIfPredExistsInT4(checkString, jobType);
                        if (doesExist == false)
                        {
                            badPredecessorList.Add(checkString + "==" + kvPair.Key);
                        }
                    }
                }
            }
            foreach (var kvPair in jobsetPredecessorMap)
            {
                logger.Debug("Getting values for " + kvPair.Key);
                List<string> keyValues = kvPair.Value;
                //logger.Info(keyValues);
                logger.Info("Number of elements in the List: " + keyValues.Count);
                foreach (var element in keyValues)
                {
                    String[] valueTuple = element.Split('@');
                    if (valueTuple.Length == 3)
                    {
                        jobType = "JOB";
                        string checkString = "DEFINE JOB ID=(" + valueTuple[1] + "," + valueTuple[0] + "," + valueTuple[2] + ")";
                        logger.Debug("Checking for " + checkString);
                        bool doesExist = AnalyzerMain.checkIfPredExistsInT4(checkString, jobType);
                        if (doesExist == false)
                        {
                            badPredecessorList.Add(checkString + "==" + kvPair.Key);
                        }
                    }
                    if (valueTuple.Length == 2)
                    {
                        jobType = "TRIGGER";
                        string checkString = "DEFINE TRIGGER ID=(" + valueTuple[1] + "," + valueTuple[0] + ")";
                        logger.Debug("Checking for " + checkString + " in the Triggers File");
                        bool doesExist = AnalyzerMain.checkIfPredExistsInT4(checkString, jobType);
                        if (doesExist == false)
                        {
                            badPredecessorList.Add(checkString + "==" + kvPair.Key);
                        }
                    }
                    if (valueTuple.Length == 1)
                    {
                        jobType = "JOBSET";
                        string checkString = "DEFINE JOBSET ID=" + valueTuple[0];
                        logger.Debug("Checking for " + checkString + " in the Jobsets File");
                        bool doesExist = AnalyzerMain.checkIfPredExistsInT4(checkString, jobType);
                        if (doesExist == false)
                        {
                            badPredecessorList.Add(checkString + "==" + kvPair.Key);
                        }
                    }
                }
            }
            logger.Info("Total number of bad predecessors found: " + badPredecessorList.Count);
            StreamWriter badPredWriter = new StreamWriter(@"C:\JMOFiles\BadPredecessorList.txt");
            foreach (var badPredecessor in badPredecessorList)
            {
                logger.Info("Missing Predecessor: " + badPredecessor);
                badPredWriter.WriteLine(badPredecessor);
            }
            badPredWriter.Close();
            Console.WriteLine("Done Writing Bad Predecessor Information.");
            

        }
        
        
        public static bool checkIfPredExistsInT4(string checkString, string jobType)
        {
            bool doesExistInT4 = false;
            if (jobType.Equals("JOB"))
            {
                t4JobFileReader = new StreamReader(t4JobFile);
                string t4Line = "";
                while ((t4Line = t4JobFileReader.ReadLine()) != null)
                {
                    if (t4Line.Contains(checkString))
                    {
                        doesExistInT4 = true;
                        break;
                    }
                }
                t4JobFileReader.Close();
            }
            
            if(jobType.Equals("JOBSET"))
            {
                t4JobsetFileReader = new StreamReader(t4JobsetFile);
                string t4Line = "";
                while ((t4Line = t4JobsetFileReader.ReadLine()) != null)
                {
                    if (t4Line.Contains(checkString))
                    {
                        doesExistInT4 = true;
                        continue;
                    }
                }
                t4JobsetFileReader.Close();

            }
            if (jobType.Equals("TRIGGER"))
            {
                triggerFileReader = new StreamReader(t4TriggerFile);
                string t4Line = "";
                while ((t4Line = triggerFileReader.ReadLine()) != null)
                {
                    if (t4Line.Contains(checkString))
                    {
                        doesExistInT4 = true;
                        break;
                    }
                }
                triggerFileReader.Close();

            }
            if(doesExistInT4==false)
            {
                if(checkString.Equals("(ns_pbds_extracts_to_gstp,nj_load_Gstp_PbdsPsn_PsnValuation_omn,0077)"))
                {
                    logger.Debug("Hello me!");
                }
                doesExistInT4=checkIfPredExistsInT2(checkString, jobType);
            }
            return doesExistInT4;
        }

        public static bool checkIfPredExistsInT2(string checkString,string jobType)
        {
            bool doesExistInT2 = false;
            t2JobFileReader = new StreamReader(t2JobFile);
            string t2Line = "";
            while ((t2Line = t2JobFileReader.ReadLine())!=null)
            {
                if(checkString.Contains("(ns_pbds_extracts_to_gstp,nj_load_Gstp_PbdsPsn_PsnValuation_omn,0077)"))
                {
                    logger.Debug("Helo");
                }
                if(t2Line.Contains(checkString))
                {
                    doesExistInT2 = true;
                    break;
                }
            }
                return doesExistInT2;
        }
        public static void readJobPredecessors()
        {
            int startIndex = 0;
            int endIndex = 0;

            string currentJob = "";
            string predecessorJob = "";
            string predecessorJobset = "";
            string predecessorJobNum = "";
            string predecessorTrigger = "";
            string predecessorTriggerType = "";

            t4PredJobFileReader = new StreamReader(t4JobPredFile);
            string currentJobLine = "";
            while((currentJobLine = t4PredJobFileReader.ReadLine()) != null)
            {
                startIndex = currentJobLine.IndexOf(jobPredDef) + jobPredDef.Length;
                if(currentJobLine.Contains("PJOB"))
                {
                    endIndex = currentJobLine.IndexOf("PJOB");
                    currentJob = currentJobLine.Substring(startIndex, endIndex - startIndex).Trim();
                    startIndex = currentJobLine.IndexOf("PJOB") + "PJOB=".Length;
                    endIndex = currentJobLine.IndexOf("PSET");
                    predecessorJob = currentJobLine.Substring(startIndex, endIndex - startIndex).Trim();
                    startIndex = currentJobLine.IndexOf("PSET") + "PSET=".Length;
                    endIndex = currentJobLine.IndexOf("PJNO");
                    predecessorJobset= currentJobLine.Substring(startIndex, endIndex - startIndex).Trim();
                    startIndex = currentJobLine.IndexOf("PJNO=") + "PJNO=".Length;
                    if (currentJobLine.Contains("WORKDAY"))
                    {
                        endIndex = currentJobLine.IndexOf("WORKDAY");
                        predecessorJobNum = currentJobLine.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                    else
                    {
                        predecessorJobNum = currentJobLine.Substring(startIndex).Trim();
                    }
                    logger.Info("Current Job: " + currentJob);
                    logger.Info("Predecessor Job: " + predecessorJob);
                    logger.Info("Predecessor Jobset: " + predecessorJobset);
                    logger.Info("Predecessor Job Number: " + predecessorJobNum);
                    if (!jobPredecessorMap.ContainsKey(currentJob))
                    {
                        jobPredecessorList.Add(predecessorJob + "@" + predecessorJobset + "@" + predecessorJobNum);
                        jobPredecessorMap[currentJob] = jobPredecessorList;
                    }
                    else
                    {
                        jobPredecessorList = jobPredecessorMap[currentJob];
                        jobPredecessorList.Add(predecessorJob + "@" + predecessorJobset + "@" + predecessorJobNum);
                        jobPredecessorMap[currentJob] = jobPredecessorList;
                    }
                    jobPredecessorList = new List<string>();

                }
                if((currentJobLine.Contains("PSET")) && (!currentJobLine.Contains("PJOB")))
                {
                    startIndex = currentJobLine.IndexOf(jobPredDef) + jobPredDef.Length;
                    endIndex = currentJobLine.IndexOf("PSET");
                    currentJob = currentJobLine.Substring(startIndex, endIndex - startIndex).Trim();
                    startIndex = currentJobLine.IndexOf("PSET=") + "PSET=".Length;
                    if(currentJobLine.Contains("WORKDAY"))
                    {
                        endIndex = currentJobLine.IndexOf("WORKDAY");
                        predecessorJobset = currentJobLine.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                    else
                    {
                        predecessorJobset = currentJobLine.Substring(startIndex).Trim();
                    }
                    if (!jobPredecessorMap.ContainsKey(currentJob))
                    {
                        jobPredecessorList.Add(predecessorJobset);
                        jobPredecessorMap[currentJob] = jobPredecessorList;
                    }
                    else
                    {
                        jobPredecessorList = jobPredecessorMap[currentJob];
                        jobPredecessorList.Add(predecessorJobset);
                        jobPredecessorMap[currentJob] = jobPredecessorList;
                    }
                    jobPredecessorList = new List<string>();
                }

                if(currentJobLine.Contains("TRID"))
                {
                    logger.Info("Job depends on a trigger");
                    startIndex = currentJobLine.IndexOf(jobPredDef) + jobPredDef.Length;
                    if(currentJobLine.Contains("WORKDAY"))
                    {
                        endIndex = currentJobLine.IndexOf("WORKDAY");
                        currentJob = currentJobLine.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                    else if(currentJobLine.Contains("TREV"))
                    {
                        endIndex = currentJob.IndexOf("TREV");
                        currentJob = currentJobLine.Substring(startIndex, endIndex - startIndex).Trim();
                    }
                    startIndex = currentJobLine.IndexOf("TREV") + "TREV=".Length;
                    endIndex = currentJobLine.IndexOf("TRID");
                    predecessorTriggerType = currentJobLine.Substring(startIndex, endIndex - startIndex).Trim();
                    startIndex = currentJobLine.IndexOf("TRID")+"TRID=".Length;
                    predecessorTrigger = currentJobLine.Substring(startIndex);
                    if (!jobPredecessorMap.ContainsKey(currentJob))
                    {
                        jobPredecessorList.Add(predecessorTriggerType+"@"+predecessorTrigger);
                        jobPredecessorMap[currentJob] = jobPredecessorList;
                    }
                    else
                    {
                        jobPredecessorList = jobPredecessorMap[currentJob];
                        jobPredecessorList.Add(predecessorTriggerType + "@" + predecessorTrigger);
                        jobPredecessorMap[currentJob] = jobPredecessorList;
                    }
                    jobPredecessorList = new List<string>();
                }

                

            }
            t4PredJobFileReader.Close();
        }
        public static void readJobsetPredecessors()
        {
            int startIndex = 0;
            int endIndex = 0;

            string currentJobset = null;
            string predecessorJob = null;
            string predecessorJobset = null;
            string predecessorJobNum = null;
            string predecessorTrigger = null;
            string predecessorTriggerType = null;

            t4PredJobsetFileReader = new StreamReader(@t4JobsetPredFile);
            string currentJobsetPredLine = "";
            while ((currentJobsetPredLine = t4PredJobsetFileReader.ReadLine()) != null)
            {
                if(currentJobsetPredLine.Contains(jobsetPredDef))
                {
                    startIndex = currentJobsetPredLine.IndexOf(jobsetPredDef)+jobsetPredDef.Length;
                    if(currentJobsetPredLine.Contains("PJOB"))
                    {
                        endIndex = currentJobsetPredLine.IndexOf("PJOB");
                        currentJobset = currentJobsetPredLine.Substring(startIndex, endIndex-startIndex).Trim();
                        startIndex = currentJobsetPredLine.IndexOf("PJOB=") + "PJOB=".Length;
                        endIndex = currentJobsetPredLine.IndexOf("PSET");
                        predecessorJob = currentJobsetPredLine.Substring(startIndex, endIndex - startIndex).Trim();
                        startIndex = currentJobsetPredLine.IndexOf("PSET=") + "PSET=".Length;
                        endIndex = currentJobsetPredLine.IndexOf("PJNO");
                        predecessorJobset = currentJobsetPredLine.Substring(startIndex, endIndex - startIndex).Trim();
                        startIndex = currentJobsetPredLine.IndexOf("PJNO=") + "PJNO=".Length;
                        if (currentJobsetPredLine.Contains("WORKDAY"))
                        {
                            endIndex = currentJobsetPredLine.IndexOf("WORKDAY");
                            predecessorJobNum = currentJobsetPredLine.Substring(startIndex, endIndex - startIndex).Trim();
                        }
                        else
                        {
                            predecessorJobNum = currentJobsetPredLine.Substring(startIndex).Trim();

                        }

                        logger.Info("Current Jobset: " + currentJobset);
                        logger.Info("Predecessor Job:  " + predecessorJob);
                        logger.Info("Predecessor Jobset: " + predecessorJobset);
                        logger.Info("Predecessor Job Number: " + predecessorJobNum);
                        if (!jobsetPredecessorMap.ContainsKey(currentJobset))
                        {
                            jobsetPredecessorList.Add(predecessorJob + "@" + predecessorJobset + "@" + predecessorJobNum);
                            jobsetPredecessorMap[currentJobset] = jobsetPredecessorList;
                        }
                        else
                        {
                            jobsetPredecessorList = jobsetPredecessorMap[currentJobset];
                            jobsetPredecessorList.Add(predecessorJob + "@" + predecessorJobset + "@" + predecessorJobNum);
                            jobsetPredecessorMap[currentJobset] = jobsetPredecessorList;
                        }
                        jobsetPredecessorList = new List<string>();    

                    }

                    if((currentJobsetPredLine.Contains("PSET")) && (!currentJobsetPredLine.Contains("PJOB")))
                    {
                        logger.Info("Jobset has another jobset as predecessor.");
                        startIndex = currentJobsetPredLine.IndexOf(jobsetPredDef) + jobsetPredDef.Length;
                        endIndex = currentJobsetPredLine.IndexOf("PSET");
                        currentJobset = currentJobsetPredLine.Substring(startIndex, endIndex - startIndex).Trim();
                        if(currentJobsetPredLine.Contains("WORKDAY"))
                        {
                            startIndex = currentJobsetPredLine.IndexOf("PSET=") + "PSET=".Length;
                            endIndex = currentJobsetPredLine.IndexOf("WORKDAY");
                            predecessorJobset = currentJobsetPredLine.Substring(startIndex, endIndex - startIndex).Trim();
                        }
                        else
                        {
                            startIndex = currentJobsetPredLine.IndexOf("PSET=") + "PSET=".Length;
                            predecessorJobset = currentJobsetPredLine.Substring(startIndex).Trim();

                        }
                        if(!jobsetPredecessorMap.ContainsKey(currentJobset))
                        {
                            jobsetPredecessorList.Add(predecessorJobset);
                            jobsetPredecessorMap[currentJobset] = jobsetPredecessorList;
                        }
                        else
                        {
                            jobsetPredecessorList = jobsetPredecessorMap[currentJobset];
                            jobsetPredecessorList.Add(predecessorJobset);
                            jobsetPredecessorMap[currentJobset] = jobsetPredecessorList;
                        }
                        jobsetPredecessorList = new List<string>();
                    }
                    if(currentJobsetPredLine.Contains("TRID"))
                    {
                        logger.Info("Jobset depends on a trigger");
                        startIndex = currentJobsetPredLine.IndexOf(jobsetPredDef) + jobsetPredDef.Length;
                        endIndex = currentJobsetPredLine.IndexOf("WORKDAY");
                        currentJobset = currentJobsetPredLine.Substring(startIndex, endIndex - startIndex).Trim();
                        startIndex = currentJobsetPredLine.IndexOf("TREV")+"TREV=".Length;
                        endIndex = currentJobsetPredLine.IndexOf("TRID");
                        predecessorTriggerType = currentJobsetPredLine.Substring(startIndex, endIndex - startIndex).Trim();
                        startIndex = currentJobsetPredLine.IndexOf("TRID") + "TRID=".Length;
                        predecessorTrigger = currentJobsetPredLine.Substring(startIndex).Trim();
                        if (!jobsetPredecessorMap.ContainsKey(currentJobset))
                        {
                            jobsetPredecessorList.Add(predecessorTriggerType+"@"+predecessorTrigger);
                            jobsetPredecessorMap[currentJobset] = jobsetPredecessorList;
                        }
                        else
                        {
                            jobsetPredecessorList = jobsetPredecessorMap[currentJobset];
                            jobsetPredecessorList.Add(predecessorTriggerType + "@" + predecessorTrigger);
                            jobsetPredecessorMap[currentJobset] = jobsetPredecessorList;
                        }
                        jobsetPredecessorList = new List<string>();

                    }
                }
            }
            logger.Info(jobsetPredecessorMap);
            t4PredJobsetFileReader.Close();
        }
    }
}
