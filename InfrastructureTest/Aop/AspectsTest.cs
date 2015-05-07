using System;
using System.IO;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.Aop;

namespace Teleopti.Ccc.InfrastructureTest.Aop
{
	[TestFixture]
	public class AspectsTest
	{
		private static IContainer setupContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<AspectInterceptor>();
			builder.RegisterType<AspectedClass>().ApplyAspects();
			builder.RegisterType<AResolvedAspect.TheResolvedAspect>();
			return builder.Build();
		}

		[SetUp]
		public void Setup()
		{
			AResolvedAspect.BeforeCallback = null;
			AResolvedAspect.AfterCallback = null;
		}

		[Test]
		public void ShouldInvokeAspectBeforeMethod()
		{
			var beforeInvoked = false;
			var container = setupContainer();
			AResolvedAspect.BeforeCallback = () => beforeInvoked = true;

			container.Resolve<AspectedClass>().ResolvedAspectMethod();

			beforeInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeAspectAfterMethod()
		{
			var afterInvokted = false;
			var container = setupContainer();
			AResolvedAspect.AfterCallback = () => afterInvokted = true;

			container.Resolve<AspectedClass>().ResolvedAspectMethod();

			afterInvokted.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeOriginalMethod()
		{
			var methodInvoked = false;
			var container = setupContainer();

			var target = container.Resolve<AspectedClass>();
			target.AspectedMethodCallback = () => methodInvoked = true;
			target.ResolvedAspectMethod();
			
			methodInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldIgnoreOtherAttributes()
		{
			var container = setupContainer();
			container.Resolve<AspectedClass>().AttributedMethod();
		}

		[Test]
		public void ShouldResolveAspectFromAttribute()
		{
			var beforeInvoked = false;
			var afterInvoked = false;
			var container = setupContainer();
			AResolvedAspect.BeforeCallback = () => beforeInvoked = true;
			AResolvedAspect.AfterCallback = () => afterInvoked = true;

			container.Resolve<AspectedClass>().ResolvedAspectMethod();

			beforeInvoked.Should().Be.True();
			afterInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldThrowOnAspectException()
		{
			var container = setupContainer();
			AResolvedAspect.BeforeCallback = () => { throw new FileNotFoundException(); };

			var target = container.Resolve<AspectedClass>();

			Assert.Throws<FileNotFoundException>(target.ResolvedAspectMethod);
		}

		[Test]
		public void ShouldInvokeAspectAfterMethodEvenThoughInvokationThrows()
		{
			var afterInvoked = false;
			var container = setupContainer();
			AResolvedAspect.AfterCallback = () => afterInvoked = true;

			var target = container.Resolve<AspectedClass>();
			target.AspectedMethodCallback = () => { throw new FileNotFoundException(); };

			Assert.Throws<FileNotFoundException>(target.ResolvedAspectMethod);
			afterInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeAspectAfterMethodWithExceptionFromInvokation()
		{
			Exception expected = new FileNotFoundException();
			Exception actual = null;
			var container = setupContainer();
			AResolvedAspect.AfterCallbackWithException = e => actual = e;

			var target = container.Resolve<AspectedClass>();
			target.AspectedMethodCallback = () => { throw expected; };

			Assert.Throws<FileNotFoundException>(target.ResolvedAspectMethod);
			actual.Should().Be.SameInstanceAs(expected);
		}

		public class AspectedClass
		{
			public Action AspectedMethodCallback;

			[An]
			public virtual void AttributedMethod() { }

			[AResolvedAspect]
			public virtual void ResolvedAspectMethod() { if (AspectedMethodCallback != null) AspectedMethodCallback(); }

		}

		private class AnAttribute : Attribute
		{
		}

		private class AResolvedAspect : AspectAttribute
		{
			public static Action BeforeCallback;
			public static Action AfterCallback;
			public static Action<Exception> AfterCallbackWithException;

			public AResolvedAspect() : base(typeof(TheResolvedAspect)) { }

			public class TheResolvedAspect : IAspect
			{
				public void OnBeforeInvocation(IInvocationInfo invocation)
				{
					if (BeforeCallback != null) BeforeCallback();
				}

				public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
				{
					if (AfterCallback != null) AfterCallback();
					if (AfterCallbackWithException != null) AfterCallbackWithException(exception);
				}
			}
		}
	}
}
