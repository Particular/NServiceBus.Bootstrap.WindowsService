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
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            if (Environment.UserInteractive && Debugger.IsAttached)
            {
                endpointConfiguration.UsePersistence<InMemoryPersistence>();
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
        endpoint.SendLocal(new MyMessage());
    }

    Task OnCriticalError(ICriticalErrorContext context)
    {
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