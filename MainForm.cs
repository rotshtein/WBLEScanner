using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices;
using Windows.Devices.Bluetooth.Advertisement;
using System.Runtime;
using System.IO;

namespace BLE
{
    public partial class frmMain : Form
    {
        BluetoothLEAdvertisementWatcher watcher = null;
        Dictionary<ulong, DateTimeOffset> timeHash = new Dictionary<ulong, DateTimeOffset>();
        string _csvFile = null;
        public frmMain()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //watcher.Received += OnAdvertisemenetReceivedAsync;
            
        }

        private void OnAdvertisemenetReceivedAsync(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            // We can obtain various information about the advertisement we just received by accessing 
            // the properties of the EventArgs class

            // The timestamp of the event
            DateTimeOffset timestamp = eventArgs.Timestamp;

            // The type of advertisement
            BluetoothLEAdvertisementType advertisementType = eventArgs.AdvertisementType;
            
            // The received signal strength indicator (RSSI)
            Int16 rssi = eventArgs.RawSignalStrengthInDBm;

            ulong addr = eventArgs.BluetoothAddress;
            // The local name of the advertising device contained within the payload, if any
            string localName = eventArgs.Advertisement.LocalName;

            
            if (!string.IsNullOrEmpty(_csvFile))
            {
                double elapseMili = -1;
                if (timeHash.ContainsKey(addr))
                {
                    elapseMili = (timestamp - timeHash[addr]).TotalMilliseconds;
                    timeHash[addr] = timestamp;
                }
                else
                {
                    timeHash.Add(addr, timestamp);
                }
                File.AppendAllText(_csvFile, string.Format("{0,1},{1,12},{2,3},{3,15},{4},{5,8}, {6}", 1, addr.ToString("X"), rssi, localName, (Convert.ToInt32(elapseMili)).ToString(),timestamp.ToString("dd MMM HH:mm:ss"), Environment.NewLine));
            }
            // Check if there are any manufacturer-specific sections.
            // If there is, print the raw data of the first manufacturer section (if there are multiple).
            /*
            string manufacturerDataString = "";
            IList<BluetoothLEManufacturerData> manufacturerSections = eventArgs.Advertisement.ManufacturerData;
            if (manufacturerSections.Count > 0)
            {
                // Only print the first one of the list
                var manufacturerData = manufacturerSections[0];
                BluetoothLEManufacturerData m;
                
                var data = new byte[manufacturerData.Data.Length];
                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                {
                    reader.ReadBytes(data);
                }
                // Print the company ID + the raw data in hex format
                //manufacturerDataString = string.Format("0x{0}: {1}", manufacturerData.CompanyId.ToString("X"), BitConverter.ToString(data));
                manufacturerDataString = string.Format("0x{0}: ", manufacturerData.CompanyId.ToString("X"));
            }*/
           
        }

        private void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            
            watcher.Stop();
            watcher.Received -= OnAdvertisemenetReceivedAsync; //(TypedEventHandler<BluetoothLEAdvertisementWatcher, BluetoothLEAdvertisementReceivedEventArgs>);
            watcher.Stopped -= OnAdvertisementWatcherStopped;
            watcher = null;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _csvFile = textBox1.Text;
            if (string.IsNullOrEmpty(_csvFile))
            {
                DialogResult dialogResult = MessageBox.Show("The is no csv file name. Are you sure?", "Log file", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult != DialogResult.Yes)
                {
                    return;

                }
            }
            timeHash.Clear();
            btnStart.Enabled = false;
            textBox1.Enabled = false;
            watcher = new BluetoothLEAdvertisementWatcher();
            watcher.Received += OnAdvertisemenetReceivedAsync; //(TypedEventHandler<BluetoothLEAdvertisementWatcher, BluetoothLEAdvertisementReceivedEventArgs>);
            watcher.Stopped += OnAdvertisementWatcherStopped;
            watcher.Start();
            btnStop.Enabled = true;
        }

        private void textBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            ofd.AddExtension = true;
            ofd.CheckPathExists = true;
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.OverwritePrompt = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ((TextBox)sender).Text = ofd.FileName;
                _csvFile = ofd.FileName;
            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (watcher != null)
                watcher.Stop();
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            textBox1.Enabled = true;

        }
    }
}
