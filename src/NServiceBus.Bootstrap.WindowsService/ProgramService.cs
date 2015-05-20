using System;
using System.Diagnostics;
using System.ServiceProcess;
using NServiceBus;
using NServiceBus.Logging;

class ProgramService : ServiceBase
{
    IBus bus;

    static ILog logger = LogManager.GetLogger<ProgramService>();

    static void Main()
    {
        using (var service = new ProgramService())
        {
            // so we can run interactive from Visual Studio or as a windows service
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
        try
        {
            var busConfiguration = new BusConfiguration();
            busConfiguration.EndpointName("SelfHostSample");
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.DefineCriticalErrorAction(OnCriticalError);

            if (Environment.UserInteractive && Debugger.IsAttached)
            {
                //TODO: For production use, please select a durable persistence.
                //http://docs.particular.net/nservicebus/persistence/
                busConfiguration.UsePersistence<InMemoryPersistence>();

                //TODO: For production use, please script your installation.
                busConfiguration.EnableInstallers();
            }
            var startableBus = Bus.Create(busConfiguration);
            bus = startableBus.Start();
        }
        catch (Exception exception)
        {
            OnCriticalError("Failed to start the bus.", exception);
        }
    }

    void OnCriticalError(string errorMessage, Exception exception)
    {
        //TODO: Decide if shutting down the process is the best response to a critical error
        //http://docs.particular.net/nservicebus/hosting/critical-errors
        var fatalMessage = string.Format("The following critical error was encountered:\n{0}\nProcess is shutting down.", errorMessage);
        logger.Fatal(fatalMessage, exception);
        Environment.FailFast(fatalMessage, exception);
    }

    protected override void OnStop()
    {
        if (bus != null)
        {
            bus.Dispose();
        }
    }

}