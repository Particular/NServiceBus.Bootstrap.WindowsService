using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

class ProgramService : ServiceBase
{
    IEndpointInstance endpoint;

    static ILog logger = LogManager.GetLogger<ProgramService>();

    static void Main()
    {
        using (var service = new ProgramService())
        {
            // so we can run interactive from Visual Studio or as a windows service
            if (Environment.UserInteractive)
            {
                Console.CancelKeyPress += (sender, e) =>
                {
                    service.OnStop();
                };
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
        AsyncOnStart().GetAwaiter().GetResult();
    }

    async Task AsyncOnStart()
    {
        try
        {
            var endpointConfiguration = new EndpointConfiguration("SelfHostSample");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            //TODO: optionally choose a different error queue. Perhaps on a remote machine
            //http://docs.particular.net/nservicebus/errors/
            endpointConfiguration.SendFailedMessagesTo("error");
            //TODO: optionally choose a different audit queue. Perhaps on a remote machine
            //http://docs.particular.net/nservicebus/operations/auditing
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            //TODO: this if is here to prevent you from accidentally deploy to production without considering important actions
            if (Environment.UserInteractive && Debugger.IsAttached)
            {
                //TODO: For production use, please select a durable persistence.
                //http://docs.particular.net/nservicebus/persistence/
                endpointConfiguration.UsePersistence<InMemoryPersistence>();

                //TODO: For production use, please script your installation.
                endpointConfiguration.EnableInstallers();
            }
            endpoint = await Endpoint.Start(endpointConfiguration);
            PerformStartupOperations();
        }
        catch (Exception exception)
        {
            logger.Fatal("Failed to start", exception);
            Environment.FailFast("Failed to start", exception);
        }
    }

    void PerformStartupOperations()
    {
        //TODO: perform any startup operations
    }

    Task OnCriticalError(ICriticalErrorContext context)
    {
        //TODO: Decide if shutting down the process is the best response to a critical error
        //http://docs.particular.net/nservicebus/hosting/critical-errors
        var fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
        logger.Fatal(fatalMessage, context.Exception);
        Environment.FailFast(fatalMessage, context.Exception);
        return Task.FromResult(0);
    }

    protected override void OnStop()
    {
        endpoint?.Stop().GetAwaiter().GetResult();
    }
}