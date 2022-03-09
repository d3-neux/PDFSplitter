using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PDFSplitter
{
    class PDFSplitter
    {

        private readonly Timer _timer;

        public PDFSplitter()
        {
            _timer = new Timer(1000 * 5) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;
        }


        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            string[] lines = new string[] { DateTime.Now.ToString() };

            //File.AppendAllLines(@"C:\Temp\Demos\Heartbeat.txt", lines);
            File.AppendAllLines(@"C:\Temp\Demos\Heartbeat.txt", SplitPDF());
        }


        public string[] SplitPDF()
        {
            List<string> log = new List<string>();

            // Get a fresh copy of the sample PDF file

            string pdfPath = @"C:\Temp\Demos\PDFSplitter\INHotFolder";


            string[] files = Directory.GetFiles(pdfPath, "*.pdf").OrderBy(f => f).ToArray();

            foreach (var file in files)
            {
                Console.WriteLine(file);
                // Open the file
                PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);

                log.Add(String.Format("{0} has {1} pages", file, inputDocument.PageCount));



                string name = Path.GetFileNameWithoutExtension(file);


                PdfDocument firstPage = new PdfDocument();
                firstPage.Version = inputDocument.Version;
                firstPage.Info.Title = String.Format("Page 1");
                firstPage.Info.Creator = inputDocument.Info.Creator;

                // Add the page and save it
                firstPage.AddPage(inputDocument.Pages[0]);
                firstPage.Save(@"C:\Temp\Demos\PDFSplitter\OutHotFolder\FirstPage\" + String.Format("{0} - Page {1}.pdf", name, 1));

                log.Add(String.Format("{0} has {1} pages", file, inputDocument.PageCount));



                if (inputDocument.PageCount > 1)
                {

                    PdfDocument attachments = new PdfDocument();
                    attachments.Version = inputDocument.Version;
                    attachments.Info.Title = String.Format("Attachments of {0}", file);
                    attachments.Info.Creator = inputDocument.Info.Creator;

                    for (int idx = 1; idx < inputDocument.PageCount; idx++)
                    {
                        attachments.AddPage(inputDocument.Pages[idx]);

                    }

                    attachments.Save(Path.Combine(@"C:\Temp\Demos\PDFSplitter\OutHotFolder\Attachments\", String.Format("Attachments of {0}.pdf", name)));
                    log.Add(String.Format("Attachments of {0}", file));
                }
            }

            return log.ToArray();
           

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
