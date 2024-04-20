using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        public RESTUtil Util
        {
            get { return _util; }
        }
        public Device Device
        {
            get { return _device; }
        }
        public string changeFirstBit(string epc)
        {
            // Change the first bit of the EPC
            string binEPC = this._utilities.HexStringToBinary(epc);
            string newBinEPC = binEPC[0] == '0' ? '1' + binEPC.Substring(1) : '0' + binEPC.Substring(1);
            return this._utilities.BinaryStringToHex(newBinEPC);
        }
    }
}


