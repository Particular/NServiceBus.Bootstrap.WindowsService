using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

class ConfigAuditQueue : IProvideConfiguration<AuditConfig>
{
    public AuditConfig GetConfiguration()
    {
        //TODO: optionally choose a different audit queue. Perhaps on a remote machine
        //http://docs.particular.net/nservicebus/operations/auditing
        return new AuditConfig
               {
                   QueueName = "audit"
               };
    }
}