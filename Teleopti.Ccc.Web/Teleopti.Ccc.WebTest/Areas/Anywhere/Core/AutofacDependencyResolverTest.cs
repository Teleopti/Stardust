using Autofac;
using Autofac.Core;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Core
{
	[TestFixture]
	public class AutofacDependencyResolverTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldHaveCoverage()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<RegisteredService>();
			builder.RegisterType<MultipleRegisteredServiceOne>().As<IMultipleRegisteredService>();
			builder.RegisterType<MultipleRegisteredServiceTwo>().As<IMultipleRegisteredService>();
			builder.RegisterType<RegisteredService>();
			var container = builder.Build();

			var target = new AutofacDependencyResolver(container);

			target.GetService(typeof(UnregisteredService)).Should().Be.Null();
			target.GetService(typeof(RegisteredService)).Should().Be.OfType<RegisteredService>();

			target.GetServices(typeof(IMultipleRegisteredService)).Should().Have.Count.EqualTo(2);
			target.GetServices(typeof(UnregisteredService)).Should().Have.Count.EqualTo(0);

			// returns 0, a bug?
			target.RegistrationsFor(new KeyedService("", typeof (IMultipleRegisteredService)), null);
		}

		private class RegisteredService
		{
			
		}

		private class MultipleRegisteredServiceOne : IMultipleRegisteredService
		{

		}

		private class MultipleRegisteredServiceTwo : IMultipleRegisteredService
		{

		}

		internal interface IMultipleRegisteredService
		{
		}

		private class UnregisteredService
		{

		}
	}

}
