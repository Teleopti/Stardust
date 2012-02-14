using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using log4net;

namespace Teleopti.LogConsole
{
    class Program
    {
        
        private static readonly ILog log = LogManager.GetLogger("Performance");
        private const string INFO = "{INFO}";
        private const string DEBUG = "{DEBUG}";
        private const string WARN = "{WARN}";
        private const string ERROR = "{ERROR}";
        private const int port = 18000;

        static void Main(string[] args)
        {
            if(!Socket.OSSupportsIPv6) 
                Console.Error.WriteLine("Your system does not support IPv6\r\n" +
                "Check you have IPv6 enabled and have changed machine.config");


            UdpClient client;
            string logLine;

            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                client = new UdpClient(port, AddressFamily.InterNetworkV6);
                while (true)
                {
                    Byte[] buffer = client.Receive(ref sender);
                    logLine = Encoding.Default.GetString(buffer);
                    if (logLine.Contains(INFO))
                    {
                        log.Info(logLine.Replace(INFO, "[I] "));
                    }
                    else if (logLine.Contains(DEBUG))
                    {
                        log.Debug(logLine.Replace(DEBUG, "[D] "));
                    }
                    else if (logLine.Contains(ERROR))
                    {
                        log.Error(logLine.Replace(ERROR, "[E] "));
                    }
                    else if (logLine.Contains(WARN))
                    {
                        log.Warn(logLine.Replace(WARN, "[W] "));
                    }
                    else
                    {
                        log.Warn(logLine);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("\nPress any key to close...");
                Console.ReadLine();
            }
        }
    }
}
