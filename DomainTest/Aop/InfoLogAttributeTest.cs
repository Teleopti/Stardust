using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.DomainTest.Aop.TestDoubles;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Aop
{
	[IoCTest]
	public class InfoLogAttributeTest : IRegisterInContainer
	{
		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<Service>().AsSelf().SingleInstance().ApplyAspects();
			var fakeLogger = new LogSpy();
			builder.RegisterInstance(fakeLogger).AsSelf();
			builder.RegisterInstance(new FakeLogManagerWrapper(fakeLogger)).As<ILogManagerWrapper>();
		}

		public LogSpy Logger;
		public Service Service;


		[Test]
		public void ShouldLogSomething()
		{
			Service.DoesSomething(null);

			Logger.InfoMessage.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldLogClassName()
		{
			Service.DoesSomething(null);

			Logger.InfoMessage.Should().Contain(Service.GetType().Name);
		}

		[Test]
		public void ShouldLogMethodName()
		{
			var methodName = Service.GetType().GetMethod("DoesSomething").Name;

			Service.DoesSomething(null);

			Logger.InfoMessage.Should().Contain(methodName);
		}

		[Test]
		public void ShouldLogValueOfArgument()
		{
			var guidString = Guid.NewGuid().ToString();

			Service.DoesSomething(guidString);

			Logger.InfoMessage.Should().Contain(guidString);
		}

		[Test]
		public void ShouldLogNameOfArguments()
		{
			const string argumentName = "theString";

			Service.DoesSomething(null);

			Logger.InfoMessage.Should().Contain(argumentName);
		}

		[Test]
		public void ShouldLogValueOfAllarguments()
		{
			Service.DoesSomethingWithMultipleArguments("some cool string", 599);

			Logger.InfoMessage.Should().Contain("some cool string");
			Logger.InfoMessage.Should().Contain("599");
		}

		[Test]
		public void ShouldLogCountOfIEnumerableArgument()
		{
			Service.DoesSomethingWithIEnumerable(new []{new object(), "thing", new object() });

			Logger.InfoMessage.Should().Contain("Count = 3");
		}

		[Test]
		public void ShouldLogCountOfIEnumerableIntArgument()
		{
			Service.DoesSomethingWithIEnumerable(new[] {1,1,1});

			Logger.InfoMessage.Should().Contain("Count = 3");
		}


		[Test, Ignore]
		public void ShouldLogUnproxiedClassName()
		{
			
		}
		[Test, Ignore]
		public void ShouldLogWithParamsArray()
		{
			
		}
		[Test, Ignore]
		public void ShouldLogWithOptionalParameters()
		{
			
		}
	}

	public class Service
	{
		[InfoLog]
		public virtual void DoesSomething(string theString)
		{
			
		}

		[InfoLog]
		public virtual void DoesSomethingWithIEnumerable(IEnumerable<object> enumerable)
		{
			
		}

		[InfoLog]
		public virtual void DoesSomethingWithIEnumerable(IEnumerable<int> enumerable)
		{
			
		}

		[InfoLog]
		public virtual void DoesSomethingWithMultipleArguments(string theString, int index)
		{
			
		}
	}
}