using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace µHttpNet.AspNetHost
{
    class Program
    {
        class Configuration
        {
            static readonly Regex OptionPattern = new Regex("--(?<option>.+)=(?<value>.*)");

            public string Host = "localhost";
            public int Port = 80;
            public string SitePath = string.Empty;
            public string VirtualDirectory = string.Empty;

            public List<string> Errors = new List<string>();

            void AddError(string format, params object[] args) { Errors.Add(string.Format(format, args)); }

            public static Configuration Parse(string[] args) {
                var configuration = new Configuration();
                
                if(args.Length > 0)
                    configuration.SitePath = args[0];
                if(args.Length > 1)
                    configuration.VirtualDirectory = args[1];

                for(var i = 2; i < args.Length; ++i) {
                    var m = OptionPattern.Match(args[i]);
                    if(m.Success) {
                        var option = m.Groups["option"].Value;
                        var value = m.Groups["value"].Value;
                        switch(option) {
                            case "port": configuration.Port = int.Parse(value); break;
                            case "host": configuration.Host = value; break;
                        }
                    }
                }

                if(!Directory.Exists(configuration.SitePath))
                    configuration.AddError("Invalid site path '{0}'", configuration.SitePath);
                if(!configuration.VirtualDirectory.StartsWith("/"))
                    configuration.AddError("Invalid virtual directory, must start with '/'");
                return configuration;
            }
        }

        readonly Configuration config;
        BasicAspNetHost host;

        Program(Configuration config) {
            this.config = config;
        }

        public void Start() {
            var hostPath = new Uri(typeof(BasicAspNetHost).Assembly.CodeBase).LocalPath;
            File.Copy(hostPath, Path.Combine(Path.Combine(config.SitePath, "Bin"), Path.GetFileName(hostPath)), true);
            var vPath = new Uri(string.Format("http://{0}:{1}{2}/", config.Host, config.Port, config.VirtualDirectory));
            host = BasicAspNetHost.CreateHost(config.SitePath, vPath);
            host.Start();
        }

        public void Stop() {
            host.Stop();
        }

        static int Main(string[] args) {
            var config = Configuration.Parse(args);
            if(config.Errors.Count > 0) {
                foreach(var error in config.Errors)
                    Console.WriteLine(error);
                Console.WriteLine();
                return DisplayUsage();
            }
            var host = new Program(config);
            if(Environment.UserInteractive) {
                host.Start();
                Console.WriteLine("<press any key to quit>");
                Console.ReadKey();
                host.Stop();
            } else {
                var service = new AspNetHostService(host);
                ServiceBase.Run(service);
            }
                return 0;
        }

        private static int DisplayUsage() {
            using (var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("µHttpNet.AspNetHost.Usage.txt")))
                Console.Error.WriteLine(reader.ReadToEnd().Replace("$(Program)", Path.GetFileNameWithoutExtension(new Uri(typeof(Program).Assembly.Location).LocalPath)));
            return -1;
        }

        class AspNetHostService : ServiceBase
        {
            readonly Program host;

            public AspNetHostService(Program host) {
                this.host = host;
            }

            protected override void OnStart(string[] args) {
                host.Start();
            }

            protected override void OnStop() {
                host.Stop();
            }
        }
    }
}
