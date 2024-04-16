using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Util;
using CommandLine;
using CommandLine.Text;
using System.Xml;

namespace ADRDAsynch
{
    public class LossPrevention
    {
        private bool debug;
        private Device device;
        private RESTUtil util;
        private String address;

        public LossPrevention(String address, bool debug)
        {
            this.address = address;
            this.debug = debug;
            this.util = new RESTUtil(address, debug);
            this.device = this.util.parseDevice(false);
        }

        class Options
        {

            [Option('a', "address", Required = true,
              HelpText = "IP address of the device.")]
            public string IPaddress
            {
                get;
                set;
            }

            [Option('t', "inventory time", Required = false, DefaultValue = 1000,
              HelpText = "Inventory Time.")]
            public long inventoryTime
            {
                get;
                set;
            }

            [Option('d', "debug", Required = false, DefaultValue = false,
              HelpText = "Prints the debug messages to standard output.")]
            public bool Debug
            {
                get;
                set;
            }

            [ParserState]
            public IParserState LastParserState
            {
                get;
                set;
            }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static void Main(string[] args)
        {

            var options = new Options();
            String address = null;
            bool debug = false;
            long inventoryTime = 1000;
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                address = "192.168.1.1";
                inventoryTime = options.inventoryTime;
                debug = options.Debug;
                Console.WriteLine("Parsed Arguments:");
                Console.WriteLine("\tAddress:\t" + address);
                Console.WriteLine("\tInventory Time:\t" + inventoryTime);
                Console.WriteLine("\tDebug:\t" + debug);

                LossPrevention app = new LossPrevention(address, false);
                app.run(inventoryTime);
            }
            else
            {
                Console.WriteLine(options.GetUsage());
                Console.ReadLine();
            }
        }

        public void run(long inventoryTime)
        {
            try
            {
                this.util.setDeviceMode(device, "Autonomous", false);
                util.startStopDevice(device, true, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            ConcurrentQueue<String> queue = new ConcurrentQueue<String>();
            List<String> tagDataList = new List<String>();
            // Initialize a dictionary to keep track of tag counts
            Dictionary<String, int> tagCounts = new Dictionary<String, int>();

            Console.WriteLine("Device[" + device.id + "] Reading the 3177 port... ");
            TCPReader tcpReader = new TCPReader(this.address, queue, this.util);
            Thread tcpReaderThread = new Thread(new ThreadStart(tcpReader.run));
            tcpReaderThread.Start();
            Console.WriteLine("Done.");

            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Start();

            while (stopwatch2.ElapsedMilliseconds < inventoryTime)
            {
                while (queue.IsEmpty)
                {
                    try
                    {
                        Thread.Sleep(20);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Thread sleeping failure: " + exc.Source);
                    }
                }

                if (!queue.IsEmpty)
                {
                    string tag;
                    while (queue.TryDequeue(out tag))
                    {
                        string dTag = tag;
                        this.util.processTCPdata(dTag, tagDataList);

                        foreach (String tagData in tagDataList)
                        {
                            // Check if the current tag matches the specific tag ID you're interested in
                            if (tagData.Equals("6200470fc1906026deb7010b"))
                            {
                                Console.WriteLine($"Specific tag {tagData} found. Triggering buzzer.");

                                // Attempt to trigger the buzzer
                                try
                                {
                                    // Assuming `util` is an instance of RESTUtil and `device` is a properly initialized Device instance
                                    // Adjust the totalDuration, timeOn, timeOff, and displayResponse parameters as needed
                                    util.buzzer(device, 5000, 5000, 0, true);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"An error occurred while trying to trigger the buzzer: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Tag {tagData} read.");
                            }
                        }

                        tagDataList.Clear();
                    }
                }
            }
            tcpReader.Shutdown();

            try
            {
                util.startStopDevice(device, false, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
