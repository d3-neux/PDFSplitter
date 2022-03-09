using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace PDFSplitter
{
    class PDFService
    {

        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(x =>
            {
                x.Service<PDFSplitter>(s =>
                {
                    s.ConstructUsing(heartbeat => new PDFSplitter());
                    s.WhenStarted(heartbeat => heartbeat.Start());
                    s.WhenStopped(heartbeat => heartbeat.Stop());
                });

                x.RunAsLocalSystem();

                x.SetServiceName("Inteli8PDFSplitter");
                x.SetDisplayName("Inteli8 PDFSplitter");
                x.SetDescription("Splits the first page of each PDF and sends them to a directory");

            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }




    }
}
