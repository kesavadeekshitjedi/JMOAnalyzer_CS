using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel=Microsoft.Office.Interop.Excel;
using log4net;
using log4net.Config;

namespace JMOAnalysis
{
    class ReadCrossRef
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyzerMain).FullName);
        public void getConvertedJobName(String csvFileName, String searchString)
        {
            logger.Info("Reading file: " + csvFileName);
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(@"C:\JMOFiles\" + csvFileName);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            for (int i = 1; i <= xlRange.Rows.Count; i++)
            {
                for (int j = 0; i <= xlRange.Columns.Count; j++)
                {
                    logger.Info("Reading cell : (" + i + "," + j + ")");

                }
            }
        }

        public void createMergedSheet(String sourceFile, String sourceFile2, String targetFile)
        {
            string jmoJobset = "";
            string jmoJob = "";
            string jmoJobNumber = "";
            
            string jmoJobType = "";
            string caJobsetName = "";
            string caJobName = "";
            string jpmJobsetName = "";
            string jpmJobName = "";
            string jmoTrigger = "";
            string caTrigger = "";
            string jpmTrigger = "";

            Excel.Application xlSourceApp = new Excel.Application();
            Excel._Workbook xlSourceWorkbook = xlSourceApp.Workbooks.Open(sourceFile);
            Excel._Worksheet xlSourceWorksheet = xlSourceWorkbook.Sheets[1];
            Excel.Range xlSourceRange = xlSourceWorksheet.UsedRange;
            int sourceRows = xlSourceRange.Rows.Count;
            int sourceCols = xlSourceRange.Columns.Count;

            Excel.Application xlSourceApp2 = new Excel.Application();
            Excel._Workbook xlSourceWorkbook2 = xlSourceApp2.Workbooks.Open(sourceFile2);
            Excel._Worksheet xlSourceWorksheet2 = xlSourceWorkbook2.Sheets[1];
            Excel.Range xlSourceRange2 = xlSourceWorksheet2.UsedRange;
            int sourceRows2 = xlSourceRange2.Rows.Count;
            int sourceCols2 = xlSourceRange2.Columns.Count;


            Excel.Application xlTargetApp = new Excel.Application();
            Excel._Workbook xlTargetWorkbook = xlTargetApp.Workbooks.Add("");
            Excel._Worksheet xlTargetWorksheet = xlTargetWorkbook.Sheets[1];
            Excel.Range xlTargetRange = xlTargetWorksheet.UsedRange;
            int TargetRows = xlTargetRange.Rows.Count;
            int TargetCols = xlTargetRange.Columns.Count;
            xlTargetApp.Visible = true;
            xlTargetWorksheet.Cells[1, 1] = "JMO Jobset Name";
            xlTargetWorksheet.Cells[1, 2] = "JMO Job Name";
            xlTargetWorksheet.Cells[1, 3] = "JMO Job Number";
            xlTargetWorksheet.Cells[1, 4] = "CA Job Name";
            xlTargetWorksheet.Cells[1, 5] = "GTI Job Name";
            xlTargetWorksheet.Cells[1, 6] = "Job Type";

            xlTargetRange = xlTargetWorksheet.get_Range("A1", "F1");

            xlTargetRange.EntireColumn.AutoFit();
            

            

            logger.Info("Read source Cross Reference File...");
            for (int i =2; i < sourceRows; i++)
            {
                jmoJobType = xlSourceRange.Cells[i, 2].Value2.ToString();
                

                if(jmoJobType.Equals("BOX"))
                {
                    jmoJobset = xlSourceRange.Cells[i, 3].Value2.ToString();
                    caJobsetName = xlSourceRange.Cells[i, 4].Value2.ToString();
                    jpmJobsetName = xlSourceRange.Cells[i, 5].Value2.ToString();
                    for (int i2=2;i2<=sourceRows2;i2++)
                    {
                        
                        if(xlSourceRange2.Cells[i,1].Value2.ToString()==jmoJobset)
                        {
                            //caJobsetName = xlSourceRange2.Cells[i, 4].Value2.ToString();

                            logger.Info("Match found for " + jmoJobset);
                            xlTargetWorksheet.Cells[i, 1] = jmoJobset;
                            
                            xlTargetWorksheet.Cells[i, 4] = caJobsetName;
                            xlTargetWorksheet.Cells[i, 5] = jpmJobsetName;
                            xlTargetWorksheet.Cells[i, 6] = jmoJobType;
                            break;
                        }
                        
                    }
                }
                if(jmoJobType.Equals("CMD"))
                {
                    jmoJob = xlSourceRange.Cells[i, 3].Value2.ToString();
                    caJobName= xlSourceRange.Cells[i, 4].Value2.ToString();
                    jpmJobName= xlSourceRange.Cells[i, 5].Value2.ToString();
                    jmoJobNumber=xlSourceRange2.Cells[i,3].Value2.ToString();
                    for (int i2 = 2; i2 <= sourceRows2; i2++)
                    {
                        string tempJobString = xlSourceRange2.Cells[i, 2].Value2.ToString().Trim();
                        logger.Debug(tempJobString + " << Temp Job String");
                        logger.Info(tempJobString.Equals(jmoJob));
                        if (tempJobString.Equals(jmoJob))
                        {
                            //caJobsetName = xlSourceRange2.Cells[i, 4].Value2.ToString();

                            logger.Info("Match found for " + jmoJob);
                            xlTargetWorksheet.Cells[i, 2] = jmoJob;
                            xlTargetWorksheet.Cells[i, 4] = caJobName;
                            xlTargetWorksheet.Cells[i, 5] = jpmJobName;
                            xlTargetWorksheet.Cells[i, 3] = jmoJobNumber;
                            xlTargetWorksheet.Cells[i, 6] = jmoJobType;
                            break;
                        }
                    }
                }
                if (jmoJobType.Equals("FT"))
                {
                    jmoTrigger = xlSourceRange.Cells[1, 3].Value2.ToString();
                    caTrigger = xlSourceRange.Cells[1, 4].Value2.ToString();
                    jpmTrigger = xlSourceRange.Cells[1, 5].Value2.ToString();
                }
                xlTargetRange.Columns.AutoFit();
                xlTargetWorksheet.get_Range("A1", "F1").Font.Bold = true;
                xlTargetWorksheet.get_Range("A1", "F1").VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                logger.Info("JMO Jobset: " + jmoJobset);
                
            }
            xlTargetWorkbook.SaveAs(targetFile, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            xlTargetWorkbook.Close();


        }
    }
}
