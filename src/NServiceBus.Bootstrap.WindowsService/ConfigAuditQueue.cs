using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

class ConfigAuditQueue : IProvideConfiguration<AuditConfig>
{
    public AuditConfig GetConfiguration()
    {
        //TODO: optionally choose a different audit queue. Perhaps on a remote machine
        return new AuditConfig
               {
                   QueueName = "audit"
               };
    }
}