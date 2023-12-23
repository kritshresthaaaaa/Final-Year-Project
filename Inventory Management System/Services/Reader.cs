using Util;
namespace Inventory_Management_System.Services

{
    public class Reader
    {
        private bool debug;
        private Device device;
        private RESTUtil util;
        private HexUtil utilities;
        private String address;

        public Reader(String address, bool debug)
        {
            this.address = "192.168.1.1";
            this.debug = false;
            this.util = new RESTUtil(address, debug);
            this.utilities = new HexUtil();
    
        }
        public void readAndWriteEPC()
        {
            // Connect to the device
            this.util.connect(this.device, false);


            string readMode = this.util.getReadMode(this.device, false);

            if (readMode != "AUTONOMOUS")
            {
                this.util.setDeviceMode(this.device, "Autonomous", false);
            }
            // Make an inventory and collect all the EPCs
            this.util.startStopDevice(this.device, true, false);

            Thread.Sleep(2000);
            List<string> detectedEPCs = this.util.getSequentialInventory(this.device, true, false);
            Thread.Sleep(2000);


            this.util.startStopDevice(this.device, false, false);


            // Iterate through detected EPCs
            foreach (string epc in detectedEPCs)
            {
                // Generate a new EPC by changing the first bit
                string newEPC = changeFirstBit(epc);

                // Write the new EPC to the tag
                this.util.CommissionTagOp(this.device, epc, newEPC, "", "", "", 1, false);

                // Print information about the tag and the new EPC
                Console.WriteLine($"Tag: {epc}, New EPC: {newEPC}");
            }
        }
        private string changeFirstBit(string epc)
        {
            // Change the first bit of the EPC
            string binEPC = this.utilities.HexStringToBinary(epc);
            string newBinEPC = binEPC[0] == '0' ? '1' + binEPC.Substring(1) : '0' + binEPC.Substring(1);
            return this.utilities.BinaryStringToHex(newBinEPC);
        }


    }
}
