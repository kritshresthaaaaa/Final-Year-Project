using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;
using Util;

namespace FypWeb.IService
{
    public class TagReaderService : ITagReaderService
    {
        private bool debug;
        private Device device;
        private RESTUtil util;
        private String address;
        private readonly IHubContext<TagHub> _tagHubContext;
        private CancellationTokenSource _cancellationTokenSource;
        public TagReaderService(IHubContext<TagHub> tagHubContext)
        {
            this.address = "192.168.1.1";
            this.debug = false;
            this._tagHubContext = tagHubContext;
            this.util = new RESTUtil(address, debug);
            this.device = this.util.parseDevice(false);
            this._cancellationTokenSource = new CancellationTokenSource();


        }
        public async Task RunContinuousRead(long inventoryTime, CancellationToken cancellationToken)
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
            while (!cancellationToken.IsCancellationRequested)
            {
                while (queue.IsEmpty)
                {
                    try
                    {
                        Thread.Sleep(20);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace);
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
                            if (tagCounts.ContainsKey(tagData))
                            {
                                tagCounts[tagData]++;
                            }
                            else
                            {
                                tagCounts[tagData] = 1;
                            }

                            // Use SignalR to send the tag data to all connected clients
                            await _tagHubContext.Clients.All.SendAsync("ReceiveTagData", tagData, tagCounts[tagData]);
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
        public void StopContinuousRead()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
