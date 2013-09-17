Self hosting NServiceBus
====================

So NServiceBus comes with a very functional host exe that abstracts much of the hosting complexity. Its many features include installation, uninstallation and configuring the windows service. It provides these features though a reasonable amount of custom code and the use of some powerful libraries like TopShelf. Since the NServiceBus Host is a general solution with dependencies there are some drawback associated with using it

## Drawback of the NServiceBus Host

*The numbers and comparisons are taken from projects included in this repository*

### Performance

In the larger scheme of things these numbers could be argued to be irrelevant, especially in the context of a real solution. However I am including them to show that a self hosted solution is in fact a little more performant. This is no surprise since a self host is a specific solution to a problem and hence can be more specialised.

#### Startup time

NServiceBus Host 5200 ms

Self Host 2783 ms

#### Memory Usage

NServiceBus Host 37.9 MB

Self Host 21.3 MB   

### Deployment Size

The Self Host does not need `NServiceBus.Host` (exe or pdb) however it does require a few more lines of setup code (see "How the code differs" below). The end result is that the self host approach is approximately 2.7MB smaller when self hosting

### Debugging

The NServiceBus Host is a non-trivial piece of software, especially when you include its dependency on TopShelf. As such this dependency can add complexity to debugging issues. On example is that there can be issues with exceptions that are passed through TopShelf. 

### Controlling the entry point

When using the NServiceBus Host the host is calling our code. As such the configuration code and behaviours (such as startup and shutdown) need to plug into very specific APIs. For example `IWantCustomLogging`, `IWantCustomInitialization`, `IWantToRunWhenBusStartsAndStops` and `IConfigureLogging`. If you invert the scenario, i.e. the developers code calls NServiceBus configuration, then these APIs are not required. 

# How the code differs

## NServiceBus Host

When using the NServiceBus Host your configuration might look like this 

    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server, IWantCustomInitialization
    {
        public void Init()
        {
            LoggingConfig.ConfigureLogging();

            SerilogConfigurator.Configure();

            Configure.Serialization.Json();

            Configure.With()
                     .DefaultBuilder();
        }
    }

## Self Host

You can achieve the same with a Self Host by using the below.

As you can see it is more code (44 lines versus 15 lines). However the extra code is very simple and easy to manage. 

    class SelfHostService : ServiceBase
    {
        IStartableBus bus;

        static void Main()
        {
            using (var service = new SelfHostService())
            {
                // so we can run interactive from Visual Studio or as a service
                if (Environment.UserInteractive)
                {
                    service.OnStart(null);
                    Console.WriteLine("\r\nPress any key to stop program\r\n");
                    Console.Read();
                    service.OnStop();
                }
                else
                {
                    Run(service);
                }
            }
        }

        protected override void OnStart(string[] args)
        {
            LoggingConfig.ConfigureLogging();

            Configure.Serialization.Json();

            bus = Configure.With()
                           .DefaultBuilder()
                           .UnicastBus()
                           .CreateBus();
            bus.Start(() => Configure.Instance.ForInstallationOn<Windows>().Install());
        }

        protected override void OnStop()
        {
            if (bus != null)
            {
                bus.Shutdown();
            }
        }
    }

# Install / Uninstall

So the NServiceBus Host handles installation and uninstallation. For example:

    cd c:\SalesEndpoint\ 
    NServiceBus.Host.exe /install /serviceName:"SalesEndpoint" 
    NServiceBus.Host.exe /uninstall /serviceName:"SalesEndpoint" 

When using a Self Host there is no such finctioality. However Windows supports these features though the use of the [Service Control tool](http://technet.microsoft.com/en-us/library/cc754599.aspx). So the same commands using `sc.exe` would be: 

    sc create SalesEndpoint binpath= "c:\SalesEndpoint\SalesEndpoint.exe"
    sc delete SalesEndpoint 

Note that they are roughly equivalent in complexity and usage.

## In Detail

The full range of NSerivceBus Host arguments are as following.

    NServiceBus.Host.exe [/install [/serviceName]
                           [/displayName]
                           [/description]
                           [/endpointConfigurationType]
                           [/endpointName]
                           [/installInfrastructure]
                           [/scannedAssemblies]
                           [/dependsOn]
                           [/sideBySide]
                           [/startManually]
                           [/username]
                           [/password]]
                           [/uninstall [/serviceName]
                           [/sidebyside]
                           [/instance:Instance Name ]    


For completeness here are the equivalent approachers to use when Self Hosting:

### serviceName

Service name can be configured when creating the service using the [sc create](http://technet.microsoft.com/en-us/library/cc990289.aspx) command.

    sc create [ServiceName] binpath= [BinaryPathName]
    sc create SalesEndpoint binpath= "c:\SalesEndpoint\SalesEndpoint.exe"

### sideBySide

When configuring a service to run in side-by-side mode NServiceBus will append the endpoint version to the service name. So you can do this yourself at installation time by appending the version to `ServiceName` you pass to `sc create`

### instance:Instance Name 

This setting is used to 

> To install multiple instances of the same service by providing each a different instance name

Since you are explicitly installing services you can do this yourself at installation time by changing the `ServiceName` you pass to `sc create`

### displayName

Display name can be configured when creating the service using the [sc create](http://technet.microsoft.com/en-us/library/cc990289.aspx) command.

    sc create [ServiceName] displayname= [Description] binpath= [BinaryPathName]
    sc create SalesEndpoint displayname= "Sales Endpoint" binpath= "c:\SalesEndpoint\SalesEndpoint.exe"

### description

Description can be changed after the service has been created using the [sc description](http://technet.microsoft.com/en-us/library/cc742069.aspx) command.

    sc description [ServiceName] [Description]
    sc description SalesEndpoint "Service for hosting the Sales Endpoint"

### dependsOn

Service dependencies can be configured after the service has been created using the [sc config](http://technet.microsoft.com/en-us/library/cc990290.aspx) command.

    sc config [ServiceName] depend= <Dependencies(separated by / (forward slash))>
    sc config SalesEndpoint depend= MSMQ/MSDTC/RavenDB

### username and password

Username and password can be configured when creating the service using the [sc create](http://technet.microsoft.com/en-us/library/cc990289.aspx) command.

    sc create [ServiceName] obj= [AccountName] password= [Password] binpath= [BinaryPathName] 
    sc create SalesEndpoint obj= MyDomain\SalesUser password= 9t6X7gkz binpath= "c:\SalesEndpoint\SalesEndpoint.exe"

### startManually

You can configure the service to be a manual start when creating the service using the [sc create](http://technet.microsoft.com/en-us/library/cc990289.aspx) command.

    sc create [ServiceName] start= {auto | demand | disabled} binpath= [BinaryPathName] 
    sc create SalesEndpoint start= demand  binpath= "c:\SalesEndpoint\SalesEndpoint.exe"

### uninstall

A service can be uninstalled using the [sc delete](http://technet.microsoft.com/en-us/library/cc742045.aspx) command.

    sc delete [ServiceName]
    sc delete SalesEndpoint

### endpointConfigurationType

Not required since your code is in full control of configuring NServiceBus. There is no need to tell NServiceBus where to find your configuration type.

### endpointName

Use `Configure.GetEndpointNameAction` before you call `Configure.With()` to change the endpoint name.

    Configure.GetEndpointNameAction = () => "SalesEndpoint";

### scannedAssemblies

By default NServiceBus will scan the current directory (`AppDomain.CurrentDomain.BaseDirectory`). If you want to run with specific assemblies you can use the `With` overload of `Configure.With(IEnumerable<Assembly> assemblies)`.

### installInfrastructure

When self hosting NServiceBus you have to invoke the installers manually using

    Configure.Instance
		.ForInstallationOn<NServiceBus.Installation.Environments.Windows>()
		.Install()


## Other stuff the NServiceBus Host provides

### Profiles and Roles

NServiceBus has the concept of `Roles` and `Profiles`. For a good outline on these see [David Boike's](http://www.make-awesome.com/) post [All About NServiceBus Host Profiles and Roles](http://www.make-awesome.com/2013/02/all-about-nservicebus-host-profiles-and-roles/). I have found that the level of abstraction that comes from `Roles` and `Profiles` is too high. Initially they simplify the solution but when you start to get into more complex cases they are hard to combine, reuse and debug. IMHO it is more better to understand what you want to configure and perform those actions explicitly. For example if you want the behaviour or `AsA_Client` then include this in you configuration code.

    Configure.Transactions.Disable();
    Configure.Features.Disable<Features.SecondLevelRetries>();
    return Configure.Instance
                    .PurgeOnStartup(true)
                    .DisableTimeoutManager()
                    .UnicastBus()
                    .RunHandlersUnderIncomingPrincipal(false);

### Switching configuration by command line 

The NServiceBus host choosing Profiles by passing command line arguments. For example you can use the `Lite Profile` by using  

    NServiceBus.Host.exe NServiceBus.Lite

With the Self Host you can achieve the same by passing in arguments and switching your configuration based on those arguments.

This feature is often used for controlling the configuration for different environments. So the different services have different arguments passed to them.

An alternative to command line switching is to switch via the current environment. 

#### Domain Name

Often the domain name includes the environment type. For example it might be suffixed in "Dev", "Test" or "Production". So you can switch configuration by looking at this value using `Domain.GetCurrentDomain().Name`.
 
#### Machine name

Often the machine name is prefixed or suffixed with the environment type. eg machines in the "Test" environment are prefixed with "TST". So you can switch configuration by looking at this value using `Environment.MachineName`.

#### By environmental variable 

If you want the environment to have more granular control then you can use an evionmental variable. For example `Environment.GetEnvironmentVariable("EnvironmentName")` or `Environment.GetEnvironmentVariable("SalesEndpointConfigName")`.  


   

