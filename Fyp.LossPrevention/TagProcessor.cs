using Fyp.DataAccess.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace Fyp.LossPrevention
{
    public class TagProcessor
    {
        private readonly ApplicationDbContext _context;
        private readonly RESTUtil _util;
        private readonly Device _device;
        private HashSet<string> _soldTags;

        public TagProcessor(ApplicationDbContext context, RESTUtil util, Device device)
        {
            _context = context;
            _util = util;
            _device = device;
            RefreshSoldTags();
        }

        public void RefreshSoldTags()
        {
            _soldTags = new HashSet<string>(_context.SoldRFIDTags.Select(t => t.TagID));
        }

        public void ProcessTags(ConcurrentQueue<string> queue)
        {
            List<string> tagDataList = new List<string>();

            while (true)
            {
                while (queue.IsEmpty)
                {
                    Thread.Sleep(20);  // Adjust timing as needed for performance considerations
                }

                while (queue.TryDequeue(out string tag))
                {
                    _util.processTCPdata(tag, tagDataList);

                    foreach (var tagData in tagDataList)
                    {
                        if (!_soldTags.Contains(tagData))
                        {
                            Console.WriteLine($"Active tag {tagData} found. Triggering buzzer.");
                            try
                            {
                                _util.buzzer(_device, 5000, 5000, 0, true);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"An error occurred while trying to trigger the buzzer: {ex.Message}");
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
    }
}
