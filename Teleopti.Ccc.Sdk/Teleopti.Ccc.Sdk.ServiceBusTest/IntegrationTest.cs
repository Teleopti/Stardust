using System;
using System.Threading;
using Autofac;
using Autofac.Core;
using NUnit.Framework;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Hosting;
using Rhino.ServiceBus.Autofac;
using Rhino.ServiceBus.Impl;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBusTest;
using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	// commented out so that it doesnt generate a success="false" in the build log. It was put on ignore anyway
	//[TestFixture]
	//public class IntegrationTest
	//{
	//    [Test, Ignore("This test requires real data and is mainly used for troubleshooting")]
	//    public void VerifyCanInitialize()
	//    {
	//        var absenceRequestHandler = new RemoteAppDomainHost(typeof(BusBootStrapper))
	//            .Configuration("Raptor.config");
	//        absenceRequestHandler.Start();

	//        Console.WriteLine("Request bus has started");

	//        var customerHost = new DefaultHost();
	//        var container = new ContainerBuilder().Build();
	//        new RhinoServiceBusConfiguration()
	//            .UseAutofac(container)
	//            .UseStandaloneConfigurationFile("RaptorClient.config")
	//            .Configure();
	//        customerHost.Start<SimpleBootStrapper>();
	//        var bus = container.Resolve<IServiceBus>();

	//        Console.WriteLine("Client started");
	//        bus.Send(      new object[]
	//                           {
	//                               new AcceptShiftTrade
	//                                   {
	//                                       Datasource = "JR V7Config",
	//                                       BusinessUnitId = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
	//                                       Timestamp = DateTime.UtcNow,
	//                                       PersonRequestId = new Guid("AA616F76-3751-42EF-9AF1-9D8B00F43931"),
	//                                       AcceptingPersonId = new Guid("11610FE4-0130-4568-97DE-9B5E015B2564")
	//                                   }
	//                           });
            
	//        customerHost.Dispose();
	//        Console.WriteLine("Transport disposed");

	//        Thread.SpinWait(6000000);
	//        absenceRequestHandler.Close();
	//        Console.WriteLine("Request bus has closed");
	//    }
	//}
}