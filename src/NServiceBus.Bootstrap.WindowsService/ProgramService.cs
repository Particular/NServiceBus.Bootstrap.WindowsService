using System;
using System.Diagnostics;
using System.ServiceProcess;
using NServiceBus;

class ProgramService : ServiceBase
{
    IBus bus;

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
        var busConfiguration = new BusConfiguration();
        busConfiguration.EndpointName("SelfHostSample");
        busConfiguration.UseSerialization<JsonSerializer>();
        busConfiguration.UsePersistence<InMemoryPersistence>();

        if (Environment.UserInteractive && Debugger.IsAttached)
        {
            busConfiguration.EnableInstallers();
        }
        var startableBus = Bus.Create(busConfiguration);
        bus = startableBus.Start();
    }

    protected override void OnStop()
    {
        if (bus != null)
        {
            bus.Dispose();
        }
    }

}