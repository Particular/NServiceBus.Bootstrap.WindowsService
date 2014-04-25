using Autofac;

class ContainerFactory
{
    public static IContainer BuildContainer()
    {
        var containerBuilder = new ContainerBuilder();
        //TODO: register services
        return containerBuilder.Build();
    }
}
