using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Analytics.Etl.CommonTest
{

	[TestFixture]
	public class EtlAppModuleTest
	{
		private FakeConfigReader _configReader;		

		[SetUp]
		public void SetUp()
		{
			_configReader = new FakeConfigReader();			
		}

		[Test]
		public void ResolvedUrlShouldNotBeNull()
		{			
			_configReader.FakeSetting("MessageBroker", "a url");
			using(var container = BuildContainer())
			{
				var url = container.Resolve<IMessageBrokerUrl>();
				url.Should().Not.Be.Null();
				url.Url.Should().Be.EqualTo("a url");
			}
		}


		private IContainer BuildContainer()
		{
			var builder = new ContainerBuilder();
			var module = new EtlAppModule();
			module.SetConfigReader(_configReader);		
			builder.RegisterModule(module);
			return builder.Build();
		}


	}
}
