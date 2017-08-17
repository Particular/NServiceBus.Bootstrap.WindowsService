﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

[DesignerCategory("Code")]
class ProgramService : ServiceBase
{
    IEndpointInstance endpoint;

    static ILog logger;

    static ProgramService()
    {
        //TODO: optionally choose a custom logging library
        //https://docs.particular.net/nservicebus/logging/#custom-logging
        // LogManager.Use<TheLoggingFactory>();
        logger = LogManager.GetLogger<ProgramService>();
    }

    static void Main()
    {
        using (var service = new ProgramService())
        {
            // to run interactive from a console or as a windows service
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
            //TODO: choose production transport
            endpointConfiguration.UseTransport<LearningTransport>();
            //TODO: For production use select a durable persistence.
            // https://docs.particular.net/persistence/
            endpointConfiguration.UsePersistence<LearningPersistence>();
            //TODO: optionally choose a different serializer
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            // https://docs.particular.net/nservicebus/serialization/
            //TODO: optionally choose a different error queue. Perhaps on a remote machine
            // https://docs.particular.net/nservicebus/recoverability/
            endpointConfiguration.SendFailedMessagesTo("error");
            //TODO: optionally choose a different audit queue. Perhaps on a remote machine
            // https://docs.particular.net/nservicebus/operations/auditing
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

            //TODO: this if is here to prevent accidentally deploying to production without considering important actions
            if (Environment.UserInteractive && Debugger.IsAttached)
            {
                //TODO: For production use select a durable persistence.
                // https://docs.particular.net/persistence/
                endpointConfiguration.UsePersistence<InMemoryPersistence>();

                //TODO: For production use script the installation.
                endpointConfiguration.EnableInstallers();
            }
            endpoint = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
            PerformStartupOperations();
        }
        catch (Exception exception)
        {
            Exit("Failed to start", exception);
        }
    }

    void Exit(string failedToStart, Exception exception)
    {
        logger.Fatal(failedToStart, exception);
        //TODO: When using an external logging framework it is important to flush any pending entries prior to calling FailFast
        // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
        Environment.FailFast(failedToStart, exception);
    }

    void PerformStartupOperations()
    {
        //TODO: perform any startup operations
    }

    Task OnCriticalError(ICriticalErrorContext context)
    {
        //TODO: Decide if shutting down the process is the best response to a critical error
        // https://docs.particular.net/nservicebus/hosting/critical-errors
        var fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
        Exit(fatalMessage, context.Exception);
        return Task.FromResult(0);
    }

    protected override void OnStop()
    {
        endpoint?.Stop().GetAwaiter().GetResult();
        //TODO: perform any shutdown operations
    }
}