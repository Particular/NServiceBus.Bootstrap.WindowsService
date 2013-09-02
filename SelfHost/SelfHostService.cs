using System;
using System.ServiceProcess;
using NServiceBus;
using NServiceBus.Installation.Environments;

class SelfHostService : ServiceBase
{
    IStartableBus bus;

    static void Main()
    {
        using (var service = new SelfHostService())
        {
            // so we can run interactive from Visual Studio or as a service
            if (Environment.UserInteractive)
            {
                service.OnStart(null);
                Console.WriteLine("\r\nPress any key to stop program\r\n");
                Console.Read();
                service.OnStop();
            }
            else
            {
                Run(service);
            }
        }
    }

    protected override void OnStart(string[] args)
    {
        LoggingConfig.ConfigureLogging();

        Configure.Serialization.Json();

        bus = Configure.With()
                        .DefaultBuilder()
                        .UnicastBus()
                        .CreateBus();
        bus.Start(() => Configure.Instance.ForInstallationOn<Windows>().Install());
    }

    protected override void OnStop()
    {
        if (bus != null)
        {
            bus.Shutdown();
        }
    }
}