using ISP.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ISP.Model;
using NLog;
using System.Windows;
using ISP.View;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Reflection;
using System.Windows.Media.Media3D;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Forms;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using System.IO;
using System.Net;


namespace ISP.ViewModel
{
    public class ISPViewModel : Base.ViewModelBase, IDisposable
    {
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public string Title => "Drag & Drop Sample Application";

        public ReactivePropertySlim<string> DropFile { get; }

        public ReactiveCommand<System.Windows.DragEventArgs> FileDropCommand { get; private set; }
        public ISPViewModel()
        {
            // Software Version to Mainform
            SWversion = "5.1.0";

            // Load initialization needed parameters from ini file.
            iSPconfig = new ISPconfig();
            ISPmodel.Instance.logger = logger;

            // Declare key variables to UI After Loading ini.
            tb_address = ISPmodel.Instance.Address;
            tb_timeout = ISPmodel.Instance.Timeout;
            cb_Ports = ISPmodel.Instance.getCommPorts();

            // Correct currently Ports from PC If the Port exits or not. 
            SelectedPort = cb_Ports.Count == 0 ? null : cb_Ports[0];
            Selectdeivcecase = ISPmodel.Instance.Device_List[0];
            IsNormalMode = true;
            IsLockProgram = true;
            //IsunLockProgram = false;

            ISPextension.Instance.Comport = new System.IO.Ports.SerialPort();
            ISPextension.Instance.setPortParams(SelectedPort == null ? null : SelectedPort.ComDeviceID);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += new EventHandler(timer_Tick);
            timer.IsEnabled = true;

            // declare the processes state.
            IsNormalMode = true;
            IsLockProgram = true;
            ISPmodel.Instance.IsTimerbusy = false;
            iSPapp = new ISPapp();
            IsConnecttoDevice = "disconnect";
            DropFile = new ReactivePropertySlim<string>().AddTo(Disposable);
            FileDropCommand = new ReactiveCommand<System.Windows.DragEventArgs>().AddTo(Disposable);

            FileDropCommand.Subscribe(e =>
            {
                if (e != null)
                {
                    OnFileDrop(e);
                }
            });
        }
        private void OnFileDrop(System.Windows.DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                return;

            var dropFiles = e.Data.GetData(System.Windows.DataFormats.FileDrop) as string[];

            if (dropFiles == null)
                return;

            if (File.Exists(dropFiles[0]))
            {
                ISPmodel.Instance.gtBinfiledata(ISPmodel.Instance.Address, dropFiles[0] );
                tb_binpath = dropFiles[0];
                DropFile.Value = dropFiles[0];
            }
            else
            {
                DropFile.Value = "ドロップされたものはファイルではありません";
            }
        }
        public void Dispose()
        {
            Disposable.Dispose();
        }

        public Model.ISPapp iSPapp { get; set; }
        public DispatcherTimer timer { get; set; }  
        public string SWversion { get; set; }
        Window sub_remind_windows { get; set; }
        ISPconfig iSPconfig;   
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public List<ISPmodel> cb_Ports { get; private set; }
        public bool IsOpenedsubwin { get; set; }
        public string tb_Interface { get; set; }
        public string tb_address { get; set; }
        public DialogResult Result { get; set; }
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {

                bool IsPortOpen = false, IsLocking = false;
                /// 1). Check port status
                /// 2). Check the device's status could be programmed.
                /// 3). Notify a msg to User, than sent a request msg about programming device.
                /// 
                if (!ISPmodel.Instance.IsTimerbusy)
                {
                    IsNormalMode = true;
                    if (SelectedPort == null)
                        return;
                    if (SelectedPort.ComDeviceID.ToLower().Contains("com"))
                    {

                        if (ISPextension.Instance.Comport != null)
                        {
                            if (ISPextension.Instance.Comport.PortName != SelectedPort.ComDeviceID)
                            {
                             
                                ISPextension.Instance.setPortParams(SelectedPort.ComDeviceID);
                               
                            }
                        }
                       
                        if (ISPextension.Instance.Comport.IsOpen)
                        {

                            ISPmodel.Instance.ControlledFlowLock();
                            if(!ISPmodel.Instance.ACK_lock)
                                IsLocking = iSPapp.lockingDevice();
                      
                        }
                        else
                        {
                            if (!ISPextension.Instance.Comport.IsOpen && !string.IsNullOrEmpty(tb_binpath))
                                ISPextension.Instance.Comport.Open();
                        }
              
                        if (!string.IsNullOrEmpty(ISPmodel.Instance.BinPath))
                        {
                            if (ISPmodel.Instance.ACK_lock)
                            {
                                //string msg = string.Format("Connection succeeded. continually next processes?\n" +
                                //    " Port :        {0}\n" +
                                //    " Address :     {1}\n"  +
                                //    " Device type : {2}\n", tb_Interface, tb_address, tb_device);
                                //Result = System.Windows.Forms.MessageBox.Show(msg, "check", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                                //if (Result == System.Windows.Forms.DialogResult.OK)
                                //    iSPapp.RunProgram();
                            }

                        }
                    
                        OnPropertyChanged(nameof(IsConnecttoDevice));
                    }
                }
                else
                {
                    IsNormalMode = false;
                }

            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }        
        public string IsConnecttoDevice
        {
            get
            {
                return ISPmodel.Instance.IsConnecttoDevice;
            }
            set
            {
                ISPmodel.Instance.IsConnecttoDevice = value;
                ISPmodel.Instance.IsConnecttoDevice = ISPmodel.Instance.ACK_lock ? "connect" : "disconnect";
                OnPropertyChanged(nameof(IsConnecttoDevice));
            }
        }
        public string[] cb_devices   // ISPmodel.Instance.DeviceType_List
        {
            get { return ISPmodel.Instance.Device_List; }
            set{
                ISPmodel.Instance.Device_List = value;
            }
        }
        public bool IsNormalMode
        {
            get { return ISPmodel.Instance.IsNormalMode; }
            set { 
                ISPmodel.Instance.IsNormalMode = value;
                OnPropertyChanged(nameof(IsNormalMode));
            }
        }
        public bool IsLockProgram
        {
            get { return ISPmodel.Instance.IsLockProgram; }
            set {  
                ISPmodel.Instance.IsLockProgram = value;
                OnPropertyChanged(nameof(IsLockProgram));
            }
        }
        public ISPmodel SelectedPort 
        {
            get { return ISPmodel.Instance.SelectPort; }
            set {

                IsLockProgram = false;
                ISPmodel.Instance.SelectPort = value;
                tb_Interface = ISPmodel.Instance.SelectPort == null ? "null" : ISPmodel.Instance.SelectPort.ComDeviceID;
                ISPmodel.Instance.IsConnecttoDevice = "disconnect";
                ISPmodel.Instance.ACK_lock = false; 
            }
        }
        public string Selectdeivcecase
        {
            get { return ISPmodel.Instance.Selectdeivcecase; }
            set { ISPmodel.Instance.Selectdeivcecase = value; }
        }
        public string tb_binpath
        {
            get { return ISPmodel.Instance.BinPath; }
            set {
                ISPmodel.Instance.BinPath = value;
             
                OnPropertyChanged(nameof(tb_binpath));
            }
        }
        public string tb_showbin //strdata
        {
            get { return ISPmodel.Instance.Strdata; }
            set { 
                ISPmodel.Instance.Strdata = value;
                OnPropertyChanged(nameof(tb_showbin));
            }
        }

        public string tb_device
        {
            get { return ISPmodel.Instance.DeviceName; }
            set { ISPmodel.Instance.DeviceName = value; }
        }

        public string tb_timeout
        {
            get { return ISPmodel.Instance.Timeout; }
            set { ISPmodel.Instance.Timeout = value; }
        }

        public string tb_crc
        {
            get { return ISPmodel.Instance.CRCcheck; }
            set { ISPmodel.Instance.CRCcheck = value;
                OnPropertyChanged(nameof(tb_crc));
            }
        }
   

        private ICommand _bt_setting, _bt_OpenFile, _bt_Program ;
        
        /// <summary>
        /// refresh Port list
        /// </summary>
        public ICommand bt_research {  get  { return new RelayCommand( param => refreshPorslists()); } }

        /// <summary>
        /// button of open bin file
        /// </summary>
        public ICommand bt_OpenFile
        {
            get
            {
                _bt_OpenFile = new RelayCommand(
                    param => DropFile.Value = ISPmodel.Instance.gtSeekInterface());
                OnPropertyChanged(nameof(IsLockProgram));
                return _bt_OpenFile;
            }
        }

        /// <summary>
        /// button of set
        /// </summary>
        public ICommand bt_setting { get { return new RelayCommand(  param => ISPmodel.Instance.nullfuntion()); } }

        /// <summary>
        /// button of Bin 
        /// </summary>
        public ICommand bt_Binfileshow { get  {  return new RelayCommand(param => showbinfile()); } }

        /// <summary>
        /// button of Program 
        /// </summary>
        public ICommand bt_Program {  get {   return _bt_Program = new RelayCommand( param => RunprogramFlow(), param => IsNormalMode); }  }

        /// <summary>
        /// button of sub-window
        /// </summary>
        public ICommand bt_winReminder { get { return new RelayCommand( param => stsubwinstate());  }  }
        public  void RunprogramFlow()
        {
            string msg = string.Format("Would you like to proceed to update TXB version?\n" +
                " Port :        {0}\n" +
                " Address :     {1}\n" +
                " Device type : {2}\n", tb_Interface, tb_address, tb_device);
            Result = System.Windows.Forms.MessageBox.Show(msg, "Check", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (Result == System.Windows.Forms.DialogResult.OK)
                iSPapp.RunProgram();
        }

        public ICommand bt_SwitchDeviceMode { get { return new RelayCommand(o => evt_SwitchDeviceMode()); } }
        public void evt_SwitchDeviceMode() {

            try
            {
                if (ISPextension.Instance.Comport.IsOpen)
                {
                    byte[] sendData = new byte[] { 0xFA, 0x62, 0x6F, 0x6F, 0x74, 0x6C, 0x6F, 0x61, 0x64, 0x65, 0x72, 0x5F, 0x6D, 0x6F, 0x64, 0x65, 0x0D, 0x0A };

                    ISPextension.Instance.Comport.BaseStream.WriteAsync(sendData, 0, sendData.Count()).ConfigureAwait(false);
                    string _logdata = $"Send data : " + ". . . . . .";
                    ISPmodel.Instance.logInfo(_logdata);
                }
             


            }
            catch (Exception e)
            {
                string _logdata = "Send data : " + ". . . . . .";// + byteToHexStr(sendData);
                ISPmodel.Instance.logWarn(_logdata);
                System.Windows.MessageBox.Show(e.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        /// <summary>
        /// event of pop windows
        /// </summary>
        public void showbinfile()
        {

            if (!string.IsNullOrEmpty(ISPmodel.Instance.BinPath))
            {
           
                if (IsOpenedsubwin == false)
                {
                    sub_remind_windows = new DeviceWindow(this);
                    sub_remind_windows.Show();
               
                    IsOpenedsubwin = true;
                }

                ISPmodel.Instance.Strdata = null;
                tb_showbin = ISPmodel.Instance.Strdata;
                ISPmodel.Instance.gtBinfiledata(tb_address, ISPmodel.Instance.BinPath);
                OnPropertyChanged(nameof(tb_showbin));
            }

        }

        /// <summary>
        /// event status of sub-windows
        /// </summary>
        public void stsubwinstate()
        {
            IsOpenedsubwin = false;
        }

        /// <summary>
        /// reload port list 
        /// </summary>
        public void refreshPorslists()
        {
            cb_Ports = ISPmodel.Instance.getCommPorts();
            OnPropertyChanged(nameof(cb_Ports));
        }

  

    }
}
