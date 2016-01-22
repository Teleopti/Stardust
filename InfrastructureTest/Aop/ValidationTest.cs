using System;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.InfrastructureTest.Aop
{
	[TestFixture]
	public class ValidationTest
	{
		[Test]
		public void ShouldThrowWhenApplyingAspectsToPrivateMethod()
		{
			Assert.Throws<AspectApplicationException>(() =>
				new ContainerBuilder().RegisterType<PrivateMethodAspectClass>().ApplyAspects()
				);
		}

		[Test]
		public void ShouldThrowWhenApplyingAspectsToNonVirtualPublicMethod()
		{
			Assert.Throws<AspectApplicationException>(() =>
				new ContainerBuilder().RegisterType<NonVirtualPublicMethodAspectClass>().ApplyAspects()
				);
		}

		[Test]
		public void ShouldThrowWhenResolvingRegistrationByReflection()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<AspectInterceptor>();
			builder.RegisterAssemblyTypes(typeof (PrivateMethodAspectClass).Assembly)
				.Where(t => t == typeof (PrivateMethodAspectClass))
				.As<PrivateMethodAspectClass>()
				.ApplyAspects();
			var container = builder.Build();
			
			var exception = Assert.Catch(() => container.Resolve<PrivateMethodAspectClass>());

			exception.InnerException.Should().Be.OfType<AspectApplicationException>();
		}

		public class PrivateMethodAspectClass
		{
			[AnAspect]
			private void method()
			{
			}
		}

		public class NonVirtualPublicMethodAspectClass
		{
			[AnAspect]
			public void Method()
			{
			}
		}

		public class AnAspectAttribute : AspectAttribute
		{
			public AnAspectAttribute():base(typeof(AnAspect))
			{
			}
		}

		public class AnAspect : IAspect
		{
			public void OnBeforeInvocation(IInvocationInfo invocation)
			{
			}

			public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
			{
			}
		}
	}
}