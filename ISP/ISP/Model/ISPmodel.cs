using Microsoft.Win32;
using NLog;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace ISP.Model
{
    public class ISPmodel : SingletonBase<ISPmodel>
    {
        public bool IsLockProgram { get; set; }
        public string IsConnecttoDevice { get; set; }
        public bool IsTimerbusy { get; set; }
        public bool IsNormalMode { get; set; }
        public Logger logger { get; set; }
        public bool Flow_lock { get; set; }
        public bool ACK_lock { get; set; }
        public bool Flow_Deliver { get; set; }
        public bool Flow_Reboot { get; set; }
        public bool ACK_Reboot { get; set; }
        public bool Flow_Verify { get; set; }
        public bool ACK_Verify { get; set; }
        public bool Flow_Automode { get; set; }
        public string _logdata { get; set; }
        public string BinPath { get; set; }
        public string System_Mode { get; set; }
        public bool Autodetect_Com { get; set; }
        public string DeviceName { get; set; }
        public UInt32 Erase_size { get; set; }
        public UInt32 Bootloader_size { get; set; }
        public string[] Device_List { get; set; }
        public string[] DeviceName_List { get; set; }
        public string[] Erase_size_List { get; set; }
        public string[] Bootloader_size_List { get; set; }
        public string Timeout { get; set; }
        public byte[] Byte_bindata { get; set; }
        public string[] ArrBindatas { get; set; }
        public int BinFileSize { get; set; }
        public string CRCcheck { get; set; }
        public uint Uintcrc32 { get; set; }
        public string Strdata { get; set; }
        public UInt32 UintADDR { get; set; }

        public void ControlledFlowLock()
        {
            Flow_lock = true;
            Flow_Deliver = false;
            Flow_Reboot= false;
            Flow_Verify = false;
            _logdata = string.Format("[set] ... ControlledFlowLock ...");
            ISPmodel.Instance.logInfo(_logdata);

        }
        public void ControlledFlowVerify()
        {
            Flow_lock = false;
            Flow_Deliver = false;
            Flow_Reboot = false;
            Flow_Verify = true;
            _logdata = string.Format("[set] ... ControlledFlowVerify ...");
            ISPmodel.Instance.logInfo(_logdata);

        }

        public void ControlledFlowReboot()
        {
            Flow_lock = false;
            Flow_Deliver = false;
            Flow_Reboot = true;
            Flow_Verify = false;
            _logdata = string.Format("[set] ... ControlledFlowReboot ...");
            ISPmodel.Instance.logInfo(_logdata);
        }

        public void ControlledFlowDeliver()
        {
            Flow_lock = false;
            Flow_Deliver = true;
            Flow_Reboot = false;
            Flow_Verify = false;
            _logdata = string.Format("[set] ... ControlledFlowDeliver ...");
            ISPmodel.Instance.logInfo(_logdata);

        }

        public Dictionary<int, List<string>> Dicbindata { get; set; }
        public string _Address;
        public string Address 
        {
            get
            {
                return _Address;
            }
            set
            {     
                int intHEX = Convert.ToInt32(value, 16);
                if (intHEX >= 2048)
                {
                    _Address = value;
                    UintADDR = (UInt32)intHEX;
                }
                else
                {
                    _Address = "0x800";
                    UintADDR = 2048;
                    //System.Windows.MessageBox.Show("address number is not a valid value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void nullfuntion()
        { 
        
        }
        public string ComDeviceID { get; set; }
        public ISPmodel SelectPort { get; set; }
        private string _Selectdeivcecase;
        public string Selectdeivcecase 
        {
            get
            {
                return _Selectdeivcecase;
            }
            set
            {
                _Selectdeivcecase = value;
                string[] arr =  _Selectdeivcecase.Replace(" ", string.Empty).Split('/').ToArray();
                ISPmodel.Instance.DeviceName = arr[0];
                ISPmodel.Instance.Erase_size = Convert.ToUInt32(arr[1]);
                ISPmodel.Instance.Bootloader_size = Convert.ToUInt32(arr[2]);
                if( ! string.IsNullOrEmpty(BinPath))
                    gtBinfiledata(Address, BinPath);
                else
                    IsLockProgram = false;
            }
        }

        /// <summary>
        /// Get the comport list 
        /// </summary>
        /// <returns></returns>
        public List<ISPmodel> getCommPorts()
        {
            //create set to include comports
            List<ISPmodel> devices = new List<ISPmodel>();
           
            ///add the reference function
            using (ManagementClass i_Entity = new ManagementClass("Win32_PnPEntity"))
            {
                foreach (ManagementObject i_Inst in i_Entity.GetInstances())
                {
                    Object o_Guid = i_Inst.GetPropertyValue("ClassGuid");
                    if (o_Guid == null || o_Guid.ToString().ToUpper() != "{4D36E978-E325-11CE-BFC1-08002BE10318}")
                        continue; // Skip all devices except device class "PORTS"

                    String s_Caption = i_Inst.GetPropertyValue("Caption").ToString();
                    String s_Manufact = i_Inst.GetPropertyValue("Manufacturer").ToString();
                    if (s_Manufact == "FTDI")
                    {
                        String s_DeviceID = i_Inst.GetPropertyValue("PnpDeviceID").ToString();
                        String s_RegPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + s_DeviceID + "\\Device Parameters";
                        String s_PortName = Registry.GetValue(s_RegPath, "PortName", "").ToString();

                        if (s_PortName.ToLower().Contains("com"))
                            devices.Add(new ISPmodel() { ComDeviceID = s_PortName });
                    }                       

                }
                i_Entity.Dispose();
            }
            return devices;
        }

        /// <summary>
        /// look for the BIN/INI file
        /// </summary>
        public string gtSeekInterface()
        {
            using (var openFileDialog1 = new System.Windows.Forms.OpenFileDialog())
            {
                try
                {

                    // 設定OpenFileDialog屬性
                    openFileDialog1.Title = "Open file (.bin)";
                    openFileDialog1.Filter = "Files|*.bin;";
                    openFileDialog1.FilterIndex = 1;
                    openFileDialog1.Multiselect = true;
                    IsLockProgram = false;
                    if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        BinPath = openFileDialog1.FileName; //取得檔名

                        gtBinfiledata(Address, BinPath);

                        _logdata = string.Format("[set] get bin file name :{0}", BinPath);
                        logInfo(_logdata);

                        return BinPath;
                    }
                    else
                    {
                        Strdata = null;
                        Dicbindata = new Dictionary<int, List<string>>();
                        return "";
                    }

                }
                catch (Exception ex) { System.Windows.MessageBox.Show(ex.StackTrace); }
         
             
            }
            return "";
        }      

        public  void gtBinfiledata(string paddress, string pBinPath)
        {
            try
            {
                //if (binfilePath.EndsWith(".bin"))
                //{
                Address = paddress;

                CRCcheck = null;
                int modnum = 0;
                int bytecount = 0;
                int count = 0;
                int countforshow = 0;
                int dicid = 0;
                string strdata = null;
                Strdata = null;
                int addr = 0;
                StringBuilder str = new StringBuilder();

                Dicbindata = new Dictionary<int, List<string>>();

                /*Open Bin file*/
                FileStream myFile = File.Open(pBinPath, FileMode.Open, FileAccess.ReadWrite);

                /*Initializes a new instance*/
                BinaryReader myReader = new BinaryReader(myFile);

                /*Retrieve the length of Bin file*/
                Int32 iLength = System.Convert.ToInt32(myFile.Length);

                /*Read to array*/
                Byte_bindata = myReader.ReadBytes(((int)myFile.Length > 131072 ? (int)myFile.Length : 131072));
                bytecount = Byte_bindata.Length;

                /* to full up Empty bits, write 0xFF*/
                modnum = (int)Erase_size - (bytecount % (int)Erase_size);
                for (int i = 0; i < modnum; i++)
                {
                    Byte_bindata = ISPextension.Instance.AddByteToArray(Byte_bindata, 0xFF);

                }

                try
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < Byte_bindata.Length; i++)
                    {
                        builder.Append(Byte_bindata[i].ToString() + " ");
                    }
                    // remove the end of array, it is empty.
                    builder = builder.Remove(builder.Length - 1, 1);

                    ArrBindatas = builder.ToString().Split(' ').ToArray();

                    foreach (var _data in ArrBindatas)
                    {
                        strdata += _data + " ";

                        ///
                        if (countforshow % Erase_size == 0)
                        {
                            countforshow = 0;
                            if (addr > 0)
                                str.Append("\r\n");

                            List<string> req = ISPextension.Instance.UINT32(UintADDR + ((uint)addr * Erase_size));
                            string[] strreq = req.ToArray();
                            int intlower = Convert.ToInt32(strreq[0] + strreq[1]);
                            int intupper = Convert.ToInt32(strreq[3] + strreq[2]);
                            str.Append(intlower.ToString("X2") + intupper.ToString("X2") + "      ");
                            addr++;
                        }
                        str.Append(Convert.ToByte(_data).ToString("X2") + " ");
                        countforshow++;

                        count++;

                        if (count == (int)Erase_size)
                        {
                            List<string> result = strdata.Trim().Split(' ').ToList();
                            Dicbindata.Add(dicid, result);
                            count = 0;
                            dicid++;
                            strdata = null;
                        }
                    }
                    Strdata = str.ToString();
                    Crc32 crc32 = new Crc32();
                    Uintcrc32 = crc32.CRC32Bytes(Byte_bindata);

                    string[] arrcrc = ISPextension.Instance.UINT32(Uintcrc32).ToArray();
                    foreach (var word in arrcrc)
                        CRCcheck += Convert.ToInt32(word).ToString("x2").ToUpper() + " ";
                    _logdata = string.Format("Cal CRC  :{0} ({1})", CRCcheck, Uintcrc32.ToString());
                    logger.Log(LogLevel.Info, string.Concat(_logdata));

                }
                catch (InvalidCastException e)
                {
                    System.Windows.MessageBox.Show(e.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                BinFileSize = Byte_bindata.Length;
                myReader.Close();
                myFile.Close();
                //}
                //else
                //{

                //}
            }
            catch (ArgumentException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            catch (IOException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (OutOfMemoryException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FormatException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (OverflowException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IndexOutOfRangeException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ObjectDisposedException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (NotFiniteNumberException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            { 
                System.Windows.MessageBox.Show($"Error: {ex.Message}");
                System.Windows.MessageBox.Show($"Error: {ex.StackTrace}");

            }

        }    
        public void logDebug(string str)
        {

            logger.Log(LogLevel.Debug, string.Concat(str));
        }

        public void logWarn(string str)
        {
            logger.Log(LogLevel.Warn, string.Concat(str));
        }

        public void logError(string str)
        {
            logger.Log(LogLevel.Error, string.Concat(str));
        }

        public void logInfo(string str)
        {
            logger.Log(LogLevel.Info, string.Concat(str));
        }
    }


}
