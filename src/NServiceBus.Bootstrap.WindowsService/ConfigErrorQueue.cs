using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;

class ConfigErrorQueue : IProvideConfiguration<MessageForwardingInCaseOfFaultConfig>
{
    public MessageForwardingInCaseOfFaultConfig GetConfiguration()
    {
        //TODO: optionally choose a different error queue. Perhaps on a remote machine
        //http://docs.particular.net/nservicebus/errors/
        return new MessageForwardingInCaseOfFaultConfig
               {
                   ErrorQueue = "error"
               };
    }
}