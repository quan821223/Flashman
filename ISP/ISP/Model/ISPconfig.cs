using ISP.ViewModel.Base;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using Wpf.Ui.Mvvm.Interfaces;

namespace ISP.Model
{

    public class ISPconfig : ViewModelBase
    {

        public static Logger logger;
        private string CONFIGPATH = System.Windows.Forms.Application.StartupPath + @"\util\system_config.ini";
        public Dictionary<string, Microchiptype> Microchiplist;
        public ISPconfig ()
        {
            // load the config data to model file.
            Evt_Readini();
            
        }

        private void Evt_Readini()
        {
            string[] classtype = { "system", "devices", "config" };
            string[] devicestype = { "microchip", "nxp" };
            string[] microchip = { "SAMC2X", "SAMD1X" };
            Microchiplist = new Dictionary<string, Microchiptype>();
            if (!string.IsNullOrWhiteSpace(CONFIGPATH))
            {
                CONFIGPATH = string.Format(AppPath + @"\util\{0}.ini", "system_config");

            }
            IniManager iniManager = new IniManager(CONFIGPATH);
            // system
            ISPmodel.Instance.System_Mode = iniManager.ReadIniFile(classtype[0], "mode", "default");
         
            ISPmodel.Instance.Autodetect_Com = iniManager.ReadIniFile(classtype[0], "AutodetectCom", "default").Contains("1")?true:false;
            // devices
            string type1 = iniManager.ReadIniFile(classtype[1], "type1", "default");
            string type2 = iniManager.ReadIniFile(classtype[1], "type2", "default");
            // devices_microchip
            ISPmodel.Instance.DeviceName_List = iniManager.ReadIniFile(classtype[1] + "_" + devicestype[0], "Device1", "default").Replace(" ", string.Empty).Split(',');
            ISPmodel.Instance.Erase_size_List = iniManager.ReadIniFile(classtype[1] + "_" + devicestype[0], "ERASE_SIZE", "default").Replace(" ", string.Empty).Split(',');
            ISPmodel.Instance.Bootloader_size_List = iniManager.ReadIniFile(classtype[1] + "_" + devicestype[0], "BOOTLOADER_SIZE", "default").Replace(" ", string.Empty).Split(',');
            // config_microchip
            ISPmodel.Instance.Address = iniManager.ReadIniFile(devicestype[0] + "_" + classtype[2], "Address", "default");
            ISPmodel.Instance.Timeout = iniManager.ReadIniFile(devicestype[0] + "_" + classtype[2], "Timeout", "default");

            //給前面介面指定
            //ISPmodel.Instance.DeviceName = ISPmodel.Instance.DeviceName_List[0];
            //ISPmodel.Instance.Erase_size = Convert.ToUInt32( ISPmodel.Instance.Erase_size_List[0]);
            //ISPmodel.Instance.Bootloader_size = Convert.ToUInt32( ISPmodel.Instance.Bootloader_size_List[0]);
            // correct microchip devices set.
            devicecase();
         
        }
        public void devicecase()
        {
            ISPmodel.Instance.Device_List = new string[ISPmodel.Instance.DeviceName_List.Length];
            foreach (var device in ISPmodel.Instance.DeviceName_List.Select((value, index) => new { value, index }))
            {
                Microchiplist.Add(device.value.ToLower(), new Microchiptype()
                {
                    name = device.value.ToLower(),
                    ERASE_SIZE = Convert.ToInt32(ISPmodel.Instance.Erase_size_List[device.index]),
                    BOOTLOADER_SIZE = Convert.ToInt32(ISPmodel.Instance.Bootloader_size_List[device.index]),
                    Deviceslist = string.Format("{0}/{1}/{2}",
                    device.value.ToLower(),
                    Convert.ToInt32(ISPmodel.Instance.Erase_size_List[device.index]),
                    Convert.ToInt32(ISPmodel.Instance.Bootloader_size_List[device.index]))
                });
                ISPmodel.Instance.Device_List[device.index] = string.Format("{0}/{1}/{2}",
                    device.value.ToLower(),
                    Convert.ToInt32(ISPmodel.Instance.Erase_size_List[device.index]),
                    Convert.ToInt32(ISPmodel.Instance.Bootloader_size_List[device.index]));
            }
        }

        public void writeini()
        {
            string path = AppPath + @"\util\system_config.ini";
            string[] classtype = { "system", "devices", "config" };
            string[] devicestype = { "microchip", "nxp" };
            string[] microchip = { "SAMC2X", "SAMD1X" };

            if (!string.IsNullOrWhiteSpace(path))
            {
                path = string.Format(AppPath + @"\util\{0}.ini", "system_config");

            }
            if (!string.IsNullOrEmpty(path))
            {
                IniManager iniManager = new IniManager(path);

                // system
                iniManager.WriteIniFile(classtype[0], "mode", "normal");
                iniManager.WriteIniFile(classtype[0], "AutodetectCom", "0");

                // devices
                iniManager.WriteIniFile(classtype[1], "type1", "microchip");
                iniManager.WriteIniFile(classtype[1], "type2", "nxp");
                // devices_microchip
                iniManager.WriteIniFile(classtype[1] + "_" + devicestype[0], "Device1", "SAMC2X, SAMD1X");
                iniManager.WriteIniFile(classtype[1] + "_" + devicestype[0], "ERASE_SIZE", "256, 256");
                iniManager.WriteIniFile(classtype[1] + "_" + devicestype[0], "BOOTLOADER_SIZE", "2048, 2048");
                // config_microchip
                iniManager.WriteIniFile(devicestype[0] + "_" + classtype[2], "Address", "0x800");
                iniManager.WriteIniFile(devicestype[0] + "_" + classtype[2], "Timeout", "1");

            }

        }

    }

    public class Microchiptype :SingletonBase<Microchiptype> 
    {
        public string name { get; set; }
        public int ERASE_SIZE { get; set; }
        public int BOOTLOADER_SIZE { get; set; }
        public int DEVICE_Index { get; set; }
        public string DEVICE_ID { get; set; }
        public UInt32 BL_CMD_ENTERBTL = 0xC0;
        public UInt32 BL_CMD_UNLOCK = 0xA0;
        public UInt32 BL_CMD_DATA = 0xA1;
        public UInt32 BL_CMD_VERIFY = 0xa2;
        public UInt32 BL_CMD_RESET = 0xa3;
        public UInt32 BL_CMD_BKSWAP_RESET = 0xa4;

        public UInt32 BL_RESP_OK = 0x50;
        public UInt32 BL_RESP_ERROR = 0x51;
        public UInt32 BL_RESP_INVALID = 0x52;
        public UInt32 BL_RESP_CRC_OK = 0x53;
        public UInt32 BL_RESP_CRC_FAIL = 0x54;
        public UInt32 BL_GUARD = 0x5048434D;

        public string Deviceslist { get; set; }
    }

    /// <summary>
    ///  Write the config ini.
    /// </summary>
    public class IniManager
    {
        private string filePath;
        private StringBuilder lpReturnedString;
        private int bufferSize;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string lpString, string lpFileName);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        public IniManager(string iniPath)
        {
            filePath = iniPath;
            bufferSize = 512;
            lpReturnedString = new StringBuilder(bufferSize);
        }

        // read ini date depend on section and key
        public string ReadIniFile(string section, string key, string defaultValue)
        {
            lpReturnedString.Clear();
            GetPrivateProfileString(section, key, defaultValue, lpReturnedString, bufferSize, filePath);
            return lpReturnedString.ToString();
        }

        // write ini data depend on section and key
        public void WriteIniFile(string section, string key, Object value)
        {
            WritePrivateProfileString(section, key, value.ToString(), filePath);
        }
    }
}
