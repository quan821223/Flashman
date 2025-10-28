using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ISP
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var assemblyName = new AssemblyName(args.Name);

            var path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
                path = $@"{assemblyName.CultureInfo}\{path}";
            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
        /*------- 貼心分隔線 ------*/
        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly; 
        }
        public string PATHMAP { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            /*------- 貼心分隔線 ------*/
            var args = e.Args;
            if (args.Length == 1)
            {
                ((App.Current) as App).PATHMAP = args[0];

            }

        }
    }
}
