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
using Microsoft.EntityFrameworkCore;
using Fyp.DataAccess.Data;

namespace Fyp.LossPrevention
{
    public class LossPrevention
    {
        private bool debug;
        private Device device;
        private RESTUtil util;
        private String address;
        private readonly ApplicationDbContext _context;
        public LossPrevention(String address, bool debug, ApplicationDbContext context)
        {
            this.address = address;
            this.debug = debug;
            this.util = new RESTUtil(address, debug);
            this.device = this.util.parseDevice(false);
            this._context = context;
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
            string connectionString = "Server=DESKTOP-K8TPSKD;Database=FypDB;Trusted_Connection=True;TrustServerCertificate=Yes";
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;
            using (var context = new ApplicationDbContext(options))
            {
                string address = "192.168.1.1";
                bool debug = false;
                long inventoryTime = 1000;

                Console.WriteLine("Parsed Arguments:");
                Console.WriteLine("\tAddress:\t" + address);
                Console.WriteLine("\tInventory Time:\t" + inventoryTime);
                Console.WriteLine("\tDebug:\t" + debug);

                LossPrevention app = new LossPrevention(address, debug, context);
                app.run(inventoryTime);
            }
        }

        public HashSet<string> FetchSoldTags()
        {
            return new HashSet<string>(_context.SoldRFIDTags.Select(t => t.TagID).ToList());
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
            List<string> tagDataList = new List<string>();
            HashSet<string> soldTags = FetchSoldTags();
            // Initialize a dictionary to keep track of tag counts
            Dictionary<String, int> tagCounts = new Dictionary<String, int>();

            Console.WriteLine("Device[" + device.id + "] Reading the 3177 port... ");
            TCPReader tcpReader = new TCPReader(this.address, queue, this.util);
            Thread tcpReaderThread = new Thread(new ThreadStart(tcpReader.run));
            tcpReaderThread.Start();
            Console.WriteLine("Done.");

      /*      Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Start();*/

            while (true)
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
                            if (!soldTags.Contains(tagData)) // Trigger buzzer if the tag is not sold
                            {
                                Console.WriteLine($"Active tag {tagData} found. Triggering buzzer.");
                                try
                                {
                                    util.buzzer(device, 5000, 5000, 0, true);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error triggering buzzer: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Sold tag {tagData} detected and ignored.");
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
