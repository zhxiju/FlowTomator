﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlowTomator.Service
{
    public class Program
    {
        public const string ServiceName = "FlowTomator";

        public static Dictionary<string, string> Parameters { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            //NetworkCredential credential;
            //WindowsAuthentication.GetCredentialsVistaAndUp("", out credential);
            //return;

            if (!Environment.UserInteractive)
            {
                ServiceBase.Run(new FlowTomatorService());
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Parameters = Environment.GetCommandLineArgs()
                                    .Where(p => p.StartsWith("/"))
                                    .Select(p => p.TrimStart('/'))
                                    .Select(p => new { Parameter = p.Trim(), Separator = p.Trim().IndexOf(':') })
                                    .ToDictionary(p => p.Separator == -1 ? p.Parameter.ToLower() : p.Parameter.Substring(0, p.Separator).ToLower(), p => p.Separator == -1 ? null : p.Parameter.Substring(p.Separator + 1));
            
            // Install service if needed
            if (Parameters.ContainsKey("uninstall"))
            {
                if (ServiceManager.ServiceIsInstalled(ServiceName))
                {
                    ServiceManager.UninstallService(ServiceName);
                    Thread.Sleep(1000);
                }

                ServiceManager.InstallService(ServiceName, "FlowTomator", Assembly.GetExecutingAssembly().Location);
            }
            else if (Parameters.ContainsKey("install"))
            {
                if (!ServiceManager.ServiceIsInstalled(ServiceName))
                    ServiceManager.InstallService(ServiceName, "FlowTomator", Assembly.GetExecutingAssembly().Location);
            }

            // Start FlowTomator service UI
            Application.Run(new FlowTomatorApplication());
        }
    }
}