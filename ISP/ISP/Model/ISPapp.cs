using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ISP.ViewModel.Base;

namespace ISP.Model
{
    public class ISPapp : ViewModelBase
    {
        public ISPapp()
        {
            programbarnumber = 0;
         
        }
        public bool IsNormalMode { get; set; }

        private double _programbarnumber;
        public double programbarnumber
        {
            get
            { 
                return _programbarnumber;
            }
            set
            {
                _programbarnumber = value;
                RaisePropertyChanged(nameof(programbarnumber));
            }
        }

        public string _logdata { get; set; }

        public void RunProgram()
        {
            if ( ! ISPmodel.Instance.Autodetect_Com)
            {
                if (ISPmodel.Instance.Dicbindata == null)
                    return;
             

                if (!string.IsNullOrEmpty(ISPmodel.Instance.BinPath) & ISPmodel.Instance.Dicbindata.Count >= 0)
                {
                    ISPmodel.Instance.gtBinfiledata(ISPmodel.Instance.Address, ISPmodel.Instance.BinPath);
                    //unknow processes
                    if (ISPmodel.Instance.ACK_lock)
                    {
                        
                        _logdata = string.Format("[set] detect target device...");
                        ISPmodel.Instance.logInfo(_logdata);
                        ISPmodel.Instance.IsTimerbusy = true;
                        program();


                    }
                    else
                    {
                        _logdata = string.Format("[set] Unlock with Port...");
                        ISPmodel.Instance.logInfo(_logdata);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Please check your Parameters or bin file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Question);

                }
            }   

        }

        public bool lockingDevice()
        {
            bool reACK = false;
            List<string> req = ISPextension.Instance.UINT32(Microchiptype.Instance.BL_GUARD);

            try
            {
                req.AddRange(ISPextension.Instance.UINT32(8));
                req.AddRange(new List<string>() { Microchiptype.Instance.BL_CMD_UNLOCK.ToString() });
                req.AddRange(ISPextension.Instance.UINT32(ISPmodel.Instance.UintADDR));
                req.AddRange(ISPextension.Instance.UINT32(Convert.ToUInt32(ISPmodel.Instance.BinFileSize)));
                ISPextension.Instance.sendFileAsync(req.ToArray());
                Task.Delay(300);
                reACK = true;
            }
            catch (Exception e)
            {
                _logdata = string.Format("lockingDevice process Error.");
                ISPmodel.Instance.logWarn(_logdata);
                System.Windows.MessageBox.Show(e.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                reACK = false;
            }
            return ISPmodel.Instance.ACK_lock;
        }

        public async void program()
        {
            int ucount = 0;
            programbarnumber = 0;

            double total = ISPmodel.Instance.Dicbindata.Count;//得到迴圈次數            

            ISPmodel.Instance.ControlledFlowDeliver();
            try
            {
                await Task.Run(() =>
                {
                  
                    for (uint index = 0; index < total; index++)
                    {
                        List<string> req = ISPextension.Instance.UINT32(Microchiptype.Instance.BL_GUARD);
                        req.AddRange(ISPextension.Instance.UINT32(ISPmodel.Instance.Erase_size + 4));
                        req.AddRange(new List<string>() { Microchiptype.Instance.BL_CMD_DATA.ToString() });
                        req.AddRange(ISPextension.Instance.UINT32(ISPmodel.Instance.UintADDR + (index * ISPmodel.Instance.Erase_size)));
                        req.AddRange(ISPmodel.Instance.Dicbindata[ucount]);
                        ISPextension.Instance.sendFileAsync(req.ToArray());
                        Thread.Sleep(50);// Task.Delay(50);//Thread.Sleep(200);
                        ucount++;
                        programbarnumber = Math.Round(((index +1) / (total)), 2) * 100;
                        RaisePropertyChanged(nameof(programbarnumber));

                    }
                   
                    verifycheck();
                    Rebooting();
             
                });
            }
            catch (InvalidCastException e)
            {
                _logdata = string.Format("programming process Error.");
                ISPmodel.Instance.logWarn(_logdata);
                System.Windows.MessageBox.Show(e.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void verifycheck()
        {
            ISPmodel.Instance.ControlledFlowVerify();
            try
            {
                Crc32 crc132 = new Crc32();

                ISPmodel.Instance.Uintcrc32 = crc132.CRC32Bytes(ISPmodel.Instance.Byte_bindata);
                _logdata = string.Format("get CRC data :{0}", ISPmodel.Instance.Uintcrc32.ToString());
                ISPmodel.Instance.logInfo(_logdata);
                List<string> req = ISPextension.Instance.UINT32(Microchiptype.Instance.BL_GUARD);
                req.AddRange(ISPextension.Instance.UINT32(4));
                req.AddRange(new List<string>() { Microchiptype.Instance.BL_CMD_VERIFY.ToString() });
                req.AddRange(ISPextension.Instance.UINT32(ISPmodel.Instance.Uintcrc32));
                ISPextension.Instance.sendFileAsync(req.ToArray());
                Thread.Sleep(200); //Task.Delay(50);// Thread.Sleep(200);

            }
            catch (InvalidCastException e)
            {
                _logdata = string.Format("verifycheck process Error.");
                ISPmodel.Instance.logWarn(_logdata);
                System.Windows.MessageBox.Show(e.Message, "Warm", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

        }

        public void Rebooting()
        {
            ISPmodel.Instance.ControlledFlowReboot();
            try
            {
                List<string> req = ISPextension.Instance.UINT32(Microchiptype.Instance.BL_GUARD);
                req.AddRange(ISPextension.Instance.UINT32(16));
                req.AddRange(new List<string>() { Microchiptype.Instance.BL_CMD_RESET.ToString() });
                req.AddRange(ISPextension.Instance.UINT32(0));
                req.AddRange(ISPextension.Instance.UINT32(0));
                req.AddRange(ISPextension.Instance.UINT32(0));
                req.AddRange(ISPextension.Instance.UINT32(0));
                ISPextension.Instance.sendFileAsync(req.ToArray());
                Thread.Sleep(200); //Task.Delay(50); //

                ISPmodel.Instance.IsTimerbusy = false; 
                ISPmodel.Instance.ACK_lock = false;

                System.Windows.MessageBox.Show("program done", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                ISPmodel.Instance.IsConnecttoDevice = "disconnect";
            }
            catch (InvalidCastException e)
            {
                System.Windows.MessageBox.Show(e.Message, "Warm", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            _logdata = string.Format("program done : {0}", "");
            ISPmodel.Instance.logInfo(_logdata);

        }
    }
}
