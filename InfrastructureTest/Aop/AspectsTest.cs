using System;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Aop
{
	[TestFixture]
	[InfrastructureTest]
	public class AspectsTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AspectedService>();
			system.AddService<Aspect1Attribute.Aspect1>();
			system.AddService<Aspect2Attribute.Aspect2>();
			system.AddService<Aspect3Attribute.Aspect3>();
		}

		public AspectedService Target;
		public Aspect1Attribute.Aspect1 Aspect1;
		public Aspect2Attribute.Aspect2 Aspect2;
		public Aspect3Attribute.Aspect3 Aspect3;

		[Test]
		public void ShouldInvokeAspectBeforeMethod()
		{
			Target.AspectedMethod();

			Aspect1.BeforeInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeAspectAfterMethod()
		{
			Target.AspectedMethod();

			Aspect1.AfterInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeOriginalMethod()
		{
			Target.AspectedMethod();

			Target.Invoked.Should().Be.True();
		}

		[Test]
		public void ShouldIgnoreOtherAttributes()
		{
			Target.AttributedMethod();
		}

		[Test]
		public void ShouldResolveAspectFromAttribute()
		{
			Target.AspectedMethod();

			Aspect1.BeforeInvoked.Should().Be.True();
			Aspect1.AfterInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldThrowOnAspectException()
		{
			Aspect1.BeforeFailsWith = new FileNotFoundException();

			Assert.Throws<FileNotFoundException>(Target.AspectedMethod);
		}

		[Test]
		public void ShouldInvokeAspectAfterMethodEvenThoughInvokationThrows()
		{
			Target.FailsWith = new FileNotFoundException();

			Assert.Throws<FileNotFoundException>(Target.AspectedMethod);
			Aspect1.AfterInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeAspectAfterMethodWithExceptionFromInvokation()
		{
			Exception expected = new FileNotFoundException();
			Target.FailsWith  = expected;

			Assert.Throws<FileNotFoundException>(Target.AspectedMethod);

			Aspect1.AfterInvokedWith.Should().Be.SameInstanceAs(expected);
		}

		[Test]
		public void ShouldInvokeAllAfterInvocationMethods()
		{
			Aspect1.AfterFailsWith = new FileNotFoundException();

			Assert.Throws<FileNotFoundException>(Target.AspectedMethod);
			Aspect2.AfterInvoked.Should().Be.True();
			Aspect3.AfterInvoked.Should().Be.True();
		}

		[Test]
		public void ShouldInvokeAfterInvocationForCompletedBeforeInvocationMethods()
		{
			Aspect3.BeforeFailsWith = new FileNotFoundException();

			Assert.Throws<FileNotFoundException>(Target.AspectedMethod);
			Aspect1.AfterInvoked.Should().Be.True();
			Aspect2.AfterInvoked.Should().Be.True();
		}
		
		public class AspectedService
		{
			public bool Invoked;
			public Exception FailsWith;

			[An]
			public virtual void AttributedMethod() { }

			[Aspect1]
			[Aspect2]
			[Aspect3]
			public virtual void AspectedMethod()
			{
				Invoked = true;
				if (FailsWith != null)
					throw FailsWith;
			}

		}

		public class AnAttribute : Attribute
		{
		}

		public class Aspect1Attribute : AspectAttribute
		{
			public Aspect1Attribute() : base(typeof(Aspect1)) { }
			public class Aspect1 : FakeAspect { }
		}

		public class Aspect2Attribute : AspectAttribute
		{
			public Aspect2Attribute() : base(typeof(Aspect2)) { }
			public class Aspect2 : FakeAspect { }
		}


		public class Aspect3Attribute : AspectAttribute
		{
			public Aspect3Attribute() : base(typeof(Aspect3)) { }
			public class Aspect3 : FakeAspect { }
		}

		public class FakeAspect : IAspect
		{
			public bool BeforeInvoked;
			public bool AfterInvoked;
			public Exception BeforeFailsWith;
			public Exception AfterFailsWith;
			public Exception AfterInvokedWith;

			public void OnBeforeInvocation(IInvocationInfo invocation)
			{
				BeforeInvoked = true;
				if (BeforeFailsWith != null)
					throw BeforeFailsWith;
			}

			public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
			{
				AfterInvokedWith = exception;
				AfterInvoked = true;
				if (AfterFailsWith != null)
					throw AfterFailsWith;
			}
		}
		
	}

}
