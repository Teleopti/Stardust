using System;
using System.Collections.Generic;
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
			ASimpleAspect.BeforeCallback = null;
			ASimpleAspect.AfterCallback = null;
			ASimpleAspect.AfterCallbackWithException = null;
			AnotherAspect.BeforeCallback = null;
			AnotherAspect.AfterCallback = null;
			AResolvedAspect.BeforeCallback = null;
			AResolvedAspect.AfterCallback = null;
		}

		[Test]
		public void ShouldInvokeAspectBeforeMethod()
		{
			var beforeInvoked = false;
			var container = setupContainer();
			ASimpleAspect.BeforeCallback = () => beforeInvoked = true;

			container.Resolve<AspectedClass>().AspectedMethod();

			beforeInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeAspectAfterMethod()
		{
			var afterInvokted = false;
			var container = setupContainer();
			ASimpleAspect.AfterCallback = () => afterInvokted = true;

			container.Resolve<AspectedClass>().AspectedMethod();

			afterInvokted.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeOriginalMethod()
		{
			var methodInvoked = false;
			var container = setupContainer();

			var target = container.Resolve<AspectedClass>();
			target.AspectedMethodCallback = () => methodInvoked = true;
			target.AspectedMethod();
			
			methodInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeAspectBeforeMethodsInOrder()
		{
			var callbacks = new List<int>();
			var container = setupContainer();
			ASimpleAspect.BeforeCallback = () => callbacks.Add(1);
			AnotherAspect.BeforeCallback = () => callbacks.Add(2);

			container.Resolve<AspectedClass>().OrderedAspectedMethod();

			callbacks.Should().Have.SameSequenceAs(new[] {1, 2});
		}

		[Test]
		public void ShouldInvokeAspectAfterMethodsInReverseOrder()
		{
			var callbacks = new List<int>();
			var container = setupContainer();
			ASimpleAspect.AfterCallback = () => callbacks.Add(1);
			AnotherAspect.AfterCallback = () => callbacks.Add(2);

			container.Resolve<AspectedClass>().OrderedAspectedMethod();

			callbacks.Should().Have.SameSequenceAs(new[] {2, 1});
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
			ASimpleAspect.BeforeCallback = () => { throw new FileNotFoundException(); };

			var target = container.Resolve<AspectedClass>();

			Assert.Throws<FileNotFoundException>(target.AspectedMethod);
		}

		[Test]
		public void ShouldInvokeAspectAfterMethodEvenThoughInvokationThrows()
		{
			var afterInvoked = false;
			var container = setupContainer();
			ASimpleAspect.AfterCallback = () => afterInvoked = true;

			var target = container.Resolve<AspectedClass>();
			target.AspectedMethodCallback = () => { throw new FileNotFoundException(); };

			Assert.Throws<FileNotFoundException>(target.AspectedMethod);
			afterInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeAspectAfterMethodWithExceptionFromInvokation()
		{
			Exception expected = new FileNotFoundException();
			Exception actual = null;
			var container = setupContainer();
			ASimpleAspect.AfterCallbackWithException = e => actual = e;

			var target = container.Resolve<AspectedClass>();
			target.AspectedMethodCallback = () => { throw expected; };

			Assert.Throws<FileNotFoundException>(target.AspectedMethod);
			actual.Should().Be.SameInstanceAs(expected);
		}

		public class AspectedClass
		{
			public Action AspectedMethodCallback;

			[ASimpleAspect]
			public virtual void AspectedMethod() { if (AspectedMethodCallback != null) AspectedMethodCallback(); }

			[AnotherAspect(Order = 2)]
			[ASimpleAspect(Order = 1)]
			public virtual void OrderedAspectedMethod() { }

			[An]
			public virtual void AttributedMethod() { }

			[AResolvedAspect]
			public virtual void ResolvedAspectMethod() { }

		}

		public class ASimpleAspect : AspectAttribute
		{
			public static Action BeforeCallback;
			public static Action AfterCallback;
			public static Action<Exception> AfterCallbackWithException;

			public override void OnBeforeInvocation(IInvocationInfo invocation)
			{
				base.OnBeforeInvocation(invocation);
				if (BeforeCallback != null) BeforeCallback();
			}
			public override void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
			{
				base.OnAfterInvocation(exception, invocation);
				if (AfterCallback != null) AfterCallback();
				if (AfterCallbackWithException != null) AfterCallbackWithException(exception);
			}
		}

		public class AnotherAspect : AspectAttribute
		{
			public static Action BeforeCallback;
			public static Action AfterCallback;

			public override void OnBeforeInvocation(IInvocationInfo invocation)
			{
				base.OnBeforeInvocation(invocation);
				if (BeforeCallback != null) BeforeCallback();
			}
			public override void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
			{
				base.OnAfterInvocation(exception, invocation);
				if (AfterCallback != null) AfterCallback();
			}
		}

		public class AnAttribute : Attribute
		{
			
		}

		public class AResolvedAspect : ResolvedAspectAttribute
		{
			public static Action BeforeCallback;
			public static Action AfterCallback;

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
				}
			}
		}
	}
}
