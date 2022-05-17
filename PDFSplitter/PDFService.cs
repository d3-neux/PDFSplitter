using NLog;
using System;
using Topshelf;

namespace PDFSplitter
{
    class PDFService
    {

        public static readonly ILogger logger = LogManager.GetCurrentClassLogger();


        static void Main(string[] args)
        {
            logger.Info("Starting Service...");

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
