using NServiceBus;
using NServiceBus.Serilog;

public class EndpointConfig : IConfigureThisEndpoint, AsA_Server, IWantCustomInitialization
{
    public void Init()
    {
        LoggingConfig.ConfigureLogging();

        SerilogConfigurator.Configure();

        Configure.Serialization.Json();

        Configure.With()
                 .DefaultBuilder()
                 .InMemorySagaPersister()
            .UseInMemoryTimeoutPersister()
            .InMemorySubscriptionStorage();
    }
}