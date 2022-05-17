using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Timers;

namespace PDFSplitter
{
    class PDFSplitter
    {

        private readonly Timer _timer;

        private static readonly string pathFirst    = ConfigurationManager.AppSettings["PATH_FIRST_PAGE"].ToString();
        private static readonly string pathAtt      = ConfigurationManager.AppSettings["PATH_ATT"].ToString();
        private static readonly string pathProc     = ConfigurationManager.AppSettings["PATH_PRO"].ToString();
        private static readonly string pathIn       = ConfigurationManager.AppSettings["PATH_IN"].ToString();
        private static readonly string pathExc      = ConfigurationManager.AppSettings["PATH_EXC"].ToString();

        private static int instanceNumber = 0;


        public PDFSplitter()
        {
            //PDFService.logger.Info($"{DateTime.Now.ToString("hh:mm:ss:ffffff")}: Starting timer");

            CreateDirectories();

            PDFService.logger.Info($"Starting timer");
            _timer = new Timer(1000 * 5);
            _timer.Elapsed += TimerElapsed;

           
        }


        public void CreateDirectories()
        {
            if (!Directory.Exists(pathFirst)) Directory.CreateDirectory(pathFirst); 
            if (!Directory.Exists(pathAtt)) Directory.CreateDirectory(pathAtt); 
            if (!Directory.Exists(pathProc)) Directory.CreateDirectory(pathProc); 
            if (!Directory.Exists(pathIn)) Directory.CreateDirectory(pathIn); 
            if (!Directory.Exists(pathExc)) Directory.CreateDirectory(pathExc); 

        }


        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            instanceNumber++;

            PDFService.logger.Info($"Instance #{instanceNumber} started");
            
            SplitPDF();
           
            PDFService.logger.Info($"Instance #{instanceNumber} finished!!!\n\n\n");

        }


        

        public void SplitPDF()
        {

            _timer.AutoReset = false;
            _timer.Stop();

            PDFService.logger.Info($"Event triggered, searching files");

            //creates a new folder for the current execution
            var processingPath = Path.Combine(pathProc, Guid.NewGuid().ToString());

            var files = moveAndGetFilesFromTempPath(processingPath);


            if (files != null && exportFiles(files) )
            {
                if (Directory.GetFiles(processingPath, "*.pdf").Length == 0)
                {
                    Directory.Delete(processingPath);
                }
                else
                {
                    //mover a inhotfolder todos los archivos no procesados

                }

                PDFService.logger.Info($"Files splitted and exported to hotfolders");
            }

            _timer.AutoReset = true;
            _timer.Start();


        }


        public string[] moveAndGetFilesFromTempPath(string processingPath)
        {
            //move files to processing directory

            string[] files = Directory.GetFiles(pathIn, "*.pdf").ToArray();

            if (files.Length == 0)
            {

                PDFService.logger.Info($"No files in folder");
                return null;
            }


            PDFService.logger.Info($"{files.Length} found in {pathIn}");
            PDFService.logger.Info($"Splitting PDFS {processingPath}");


            if (!Directory.Exists(processingPath))
            {
                Directory.CreateDirectory(processingPath);
            }


            PDFService.logger.Info($"Moving PDFS to {processingPath}");
            foreach (var file in files)
            {
                
                File.Move(file, Path.Combine(processingPath, Path.GetFileName(file)));
            }

            return Directory.GetFiles(processingPath, "*.pdf").ToArray(); ;
        }



        private bool exportFiles(string  [] files)
        {

            ///
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];

                PDFService.logger.Info($"Splitting {file} #{i + 1}");
                // Open the file

                try
                {

                    PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                    PDFService.logger.Info($"{file} has {inputDocument.PageCount} pages");

                    string originalName = Path.GetFileNameWithoutExtension(file);

                    //save pdf
                    if (savePDF(inputDocument, originalName))
                        File.Delete(file);
                }
                catch(Exception e)
                {
                    var newPath = Path.Combine(pathExc, DateTime.Now.ToString("yyyy-MM-dd"));


                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }


                    var filePath = Path.Combine(newPath, Path.GetFileName(file));

                    if (File.Exists(filePath))
                    {
                        filePath = Path.Combine(newPath, Guid.NewGuid().ToString() + " - " + Path.GetFileName(file));

                    }

                    File.Move(file, filePath); 
                }

            }

            return true;
        }

        private bool savePDF(PdfDocument inputDocument, string originalName)
        {
            string attPagesName = Path.Combine(pathAtt, originalName + " - Attachments.pdf");
            string firstPageName = Path.Combine(pathFirst, originalName + " - Page 1.pdf");

            //SAVING ATT FIRST

            if (inputDocument.PageCount > 1)
            {

                PdfDocument attachments = new PdfDocument();
                attachments.Version = inputDocument.Version;
                attachments.Info.Title = String.Format("Attachments of {0}", originalName);
                attachments.Info.Creator = inputDocument.Info.Creator;

                for (int idx = 1; idx < inputDocument.PageCount; idx++)
                {
                    attachments.AddPage(inputDocument.Pages[idx]);
                }

                attachments.Save(attPagesName);
                PDFService.logger.Info($"Attachments saved as {attPagesName}");
            }
            else
            {
                PDFService.logger.Info($"{originalName} has only one page");
            }

            //SAVING FIRST PAGE AT THE END

            PdfDocument firstPage = new PdfDocument();
            firstPage.Version = inputDocument.Version;
            firstPage.Info.Title = String.Format("Page 1");
            firstPage.Info.Creator = inputDocument.Info.Creator;
            // Add the page and save it
            firstPage.AddPage(inputDocument.Pages[0]);
            firstPage.Save(firstPageName);

            PDFService.logger.Info($"First page saved as {firstPageName}");

            return true;
        }

        static void CopyFiles(DirectoryInfo source, DirectoryInfo destination, bool overwrite, string searchPattern)
        {
            FileInfo[] files = source.GetFiles(searchPattern);

            //this section is what's really important for your application.
            foreach (FileInfo file in files)
            {
                file.CopyTo(destination.FullName + "\\" + file.Name, overwrite);
            }
        }


        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

    }
}
