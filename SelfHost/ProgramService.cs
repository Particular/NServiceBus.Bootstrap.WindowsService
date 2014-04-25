using System;
using System.Diagnostics;
using System.ServiceProcess;
using Autofac;
using NServiceBus;
using NServiceBus.Installation.Environments;

class ProgramService : ServiceBase
{
    IStartableBus bus;
    IContainer container;

    static void Main()
    {
        using (var service = new ProgramService())
        {
            // so we can run interactive from Visual Studio or as a service
            if (Environment.UserInteractive)
            {
                service.OnStart(null);
                Console.WriteLine("\r\nPress enter key to stop program\r\n");
                Console.Read();
                service.OnStop();
                return;
            }
            Run(service);
        }
    }

    protected override void OnStart(string[] args)
    {
        Configure.GetEndpointNameAction = () => "SelfHostSample";
        LoggingConfig.ConfigureLogging();

        Configure.Serialization.Json();

        container = ContainerFactory.BuildContainer();

        bus = Configure.With()
            .AutofacBuilder(container)
            .InMemorySagaPersister()
            .UseInMemoryTimeoutPersister()
            .InMemorySubscriptionStorage()
            .UnicastBus()
            .CreateBus();
        bus.Start(Startup);
    }

    static void Startup()
    {
        //Only create queues when a user is debugging
        if (Environment.UserInteractive && Debugger.IsAttached)
        {
            Configure.Instance.ForInstallationOn<Windows>().Install();
        }
    }

    protected override void OnStop()
    {
        if (bus != null)
        {
            bus.Shutdown();
        }
        if (container != null)
        {
            container.Dispose();
        }
    }

}