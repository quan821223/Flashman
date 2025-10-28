using ISP.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace ISP.Model
{
    internal class ISPextension : SingletonBase<ISPextension>
    {
        public string _logdata { get; set; }
        public string PortName { get; set; }
        public int CombaudRate { get; set; }
        public int DataBitValue { get; set; }
        public Parity ParityValue { get; set; }
        public StopBits StopBitsValue { get; set; }
        public byte[] Recvdata { get; set; }

        public Dictionary<string, SerialPort> DicSerialPort = new Dictionary<string, SerialPort>(); 

        public SerialPort Comport { get; set; }
        public bool SendConnectReq(string PortName)
        {
            bool IsConnect = false;
            try
            {
                setPortParams(PortName);
                Comport.Open();
                IsConnect = true;
            }
            catch
            {
                IsConnect = false;
            }
            return true;
        }

        public bool setPortParams(string pspecifiedPortname)
        {   
            CombaudRate = 115200;
            ParityValue = Parity.None;
            StopBitsValue = StopBits.One;
            DataBitValue = 8;
            Recvdata = new byte[0];

            try
            {
                if (pspecifiedPortname == null)
                    return false;

                // Comport.PortName = pspecifiedPortname;
                if (Comport.IsOpen)
                {
                    Comport.Close();
                }

                Comport.PortName = pspecifiedPortname;// PortName;//curCOMname;
                Comport.BaudRate = CombaudRate;
                Comport.Parity = ParityValue;
                Comport.DataBits = DataBitValue;
                Comport.StopBits = StopBitsValue;
                //Comport.DtrEnable = false;
                //Comport.RtsEnable = false;
                Comport.ReadTimeout = 1000;
                Comport.WriteTimeout = 1000;

                _logdata = string.Format("[set] set COM params : PortName={0},BaudRate={1}",
                Comport.PortName, Comport.PortName);
                ISPmodel.Instance.logInfo(_logdata);
                Comport.DataReceived += new SerialDataReceivedEventHandler(ComportDataReceived);
                Comport.ErrorReceived += new SerialErrorReceivedEventHandler(ComportERRORDataReceived);

                return true;
            }
            catch (Exception ex)
            {
                _logdata = string.Format("[set] set COM params Error");
                ISPmodel.Instance.logError(_logdata);
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void ComportDataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            if ((SerialPort)sender == null)
            {
                return;
            }
            else
            {
                int _BytesToRead = Comport.BytesToRead;
                Recvdata = new byte[_BytesToRead];
                byte[] _RecvData = new byte[_BytesToRead];
                Comport.Read(_RecvData, 0, _BytesToRead);
                Recvdata = _RecvData;
                //data.Append(SendModel.byteToHexStr(Recvdata));
                if (Recvdata.Length > 0)
                {
                    uint Uintdata = Convert.ToUInt32(Recvdata[0]);
                    _logdata = string.Format("com get data :{0}", Uintdata.ToString());
                    ISPmodel.Instance.logInfo(_logdata);

                    if (ISPmodel.Instance.Flow_lock)
                    {
                        ISPmodel.Instance.ACK_lock = Uintdata.ToString() == "80" ? true : false;

                        if (ISPmodel.Instance.ACK_lock)
                        {                             
                            Uintdata = Convert.ToUInt32(Recvdata[0]);
                            ISPmodel.Instance.IsConnecttoDevice = "connect";
                        }
                        else
                        {
                            ISPmodel.Instance.IsConnecttoDevice = "disconnect";
                            _logdata = string.Format("LockingDevice get not correct data from deives.");
                            ISPmodel.Instance.logWarn(_logdata);
                        }                        
                    }
                    if (ISPmodel.Instance.Flow_Verify)
                        ISPmodel.Instance.ACK_Verify = Uintdata.ToString() == "83" ? true : false;
                    if (ISPmodel.Instance.Flow_Reboot)
                        ISPmodel.Instance.ACK_Reboot = Uintdata.ToString() == "80" ? true : false;
                    if (ISPmodel.Instance.Flow_Automode)
                    {
                        ISPmodel.Instance.ACK_lock = Uintdata.ToString() == "80" ? true : false;
                    }
                }
            }
        }

        public void ComportERRORDataReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            string _data, output = null;
            output = Comport.ReadExisting();
            _data = string.Format("get ERROR data :{0}", output.ToString());
            ISPmodel.Instance.logger.Log(LogLevel.Info, string.Concat(_data));
        }

        public void sendFileAsync(string[] pbytedata)
        {
            int SendCount = 0;
            if (!Comport.IsOpen)
                Comport.Open();
            byte[] sendData = new byte[pbytedata.Length];
            try
            {
                foreach (var tmp in pbytedata)
                {
                    sendData[SendCount++] = Convert.ToByte(tmp);
                }
                SendCount = sendData.Length;
                Comport.BaseStream.WriteAsync(sendData, 0, SendCount).ConfigureAwait(false);
                _logdata = "Send data : " +  ". . . . . .";//+  SendModel.byteToHexStr(sendData);
                ISPmodel.Instance.logInfo(_logdata);

            }
            catch (Exception e)
            {
                _logdata = "Send data : " +  ". . . . . .";// + byteToHexStr(sendData);
                ISPmodel.Instance.logWarn(_logdata);
                System.Windows.MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    //returnStr = Convert.ToString(bytes[i], 16);
                    returnStr += bytes[i].ToString("X2");
                    returnStr += bytes[i].ToString(" ");
                }
            }
            return returnStr.Trim();
        }

        public byte[] AddByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 0);
            newArray[bArray.Length] = newByte;
            return newArray;
        }

        public List<string> UINT32(UInt32 pValue)
        {
            List<string> retDATA = new List<string>();
            retDATA.Add((pValue >> 0 & 0xFF).ToString());
            retDATA.Add((pValue >> 8 & 0xFF).ToString());
            retDATA.Add((pValue >> 16 & 0xFF).ToString());
            retDATA.Add((pValue >> 24 & 0xFF).ToString());
            return retDATA;
        }



    }
}
