using System;
using System.Collections.Generic;
using System.Threading;
using Fyp.DataAccess.Data;
using Util; 

namespace FypWeb.Areas.Admin
{
    public class Reader
    {
        private readonly RESTUtil _util;
        private readonly Device _device;
        private readonly HexUtil _utilities;
        private readonly bool _debug;
        private readonly string _address;


        public Reader(string address, bool debug)
        {
            _address = address;
            _debug = debug;
            _util = new RESTUtil(_address, _debug);
            _device = _util.parseDevice(false);
            _utilities = new HexUtil();
        }

        public void ConnectToDevice()
        {
            _util.connect(_device, false);
        }

        public void SetDeviceReadMode()
        {
            string readMode = _util.getReadMode(_device, false);
            if (readMode != "AUTONOMOUS")
            {
                _util.setDeviceMode(_device, "Autonomous", false);
            }
        }

        public void StartDevice()
        {
            _util.startStopDevice(_device, true, false);
    
        }

        public void StopDevice()
        {
            _util.startStopDevice(_device, false, false);
        }

        public List<string> GetDetectedEPCs()
        {
          
            List<string> detectedEPCs = _util.getSequentialInventory(_device, true, false);
          
            return detectedEPCs;
        }
        public async Task<List<string>> ReadContinuousTags(CancellationToken cancellationToken)
        {
            this.ConnectToDevice();
            this.SetDeviceReadMode();
            this.StartDevice();
            HashSet<string> detectedEPCs = new HashSet<string>();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Get newly detected EPCs
                    var newEPCs = GetDetectedEPCs();

                    // Add to the set to ensure uniqueness
                    foreach (var epc in newEPCs)
                    {
                        detectedEPCs.Add(epc);
                    }

                    // Consider implementing logic to update UI or database here, if needed

                    // Adjust the delay as needed for your application
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    // Log the error
                    Console.WriteLine($"Error scanning RFID tags: {ex.Message}");
                    // Optional: Decide if you want to break the loop or just log the error and continue
                }
            }

            return detectedEPCs.ToList();
        }



    }
}
