using NServiceBus;

public class MessageStarter : IWantToRunWhenBusStartsAndStops
{
    public IBus Bus { get; set; }
    public void Start()
    {
        Bus.SendLocal(new MyMessage());
    }

    public void Stop()
    {
    }
}