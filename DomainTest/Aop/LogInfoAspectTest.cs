using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Aop
{
	[IoCTest]
	public class LogInfoAspectTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<Service>();
			var log = new LogSpy();
			system.UseTestDouble(log).For<ILog>();
			system.UseTestDouble(new FakeLogManager(log)).For<ILogManager>();
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
		public void ShouldLogNullWhenArgumentIsNull()
		{
			Service.DoesSomething(null);

			Logger.InfoMessage.Should().Contain("null");
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

		[Test]
		public void ShouldLogWithParamsArray()
		{
			Service.DoesSomethingWithParamsArray();

			Logger.InfoMessage.Should().Contain("Count = 0");
		}





		[Test]
		public void ShouldLogReturnValue()
		{
			Service.Returns(() => 1);

			Logger.InfoMessage.Should().Contain("1");
		}

		[Test]
		public void ShouldLogReturnValueNull()
		{
			Service.Returns<string>(() => null);

			Logger.InfoMessage.Should().Contain("null");
		}

		[Test]
		public void ShouldLogEnumerableReturnCountFromArray()
		{
			Service.Returns<IEnumerable<int>>(() => new[] {1});

			Logger.InfoMessage.Should().Contain("Count = 1");
		}

		[Test]
		public void ShouldLogEnumerableReturnCountFromList()
		{
			Service.Returns<IEnumerable<int>>(() => new List<int> { 1, 2, 3 });

			Logger.InfoMessage.Should().Contain("Count = 3");
		}

		[Test]
		public void ShouldLogSomethingReadableWhenReturningIEnumerable()
		{
			Service.Returns(() => from i in Enumerable.Range(0, 10) select i);

			Logger.InfoMessage.Should().Contain("Enumerable");
		}

		[Test]
		public void ShouldLogSomethingReadableWhenReturningGeneric()
		{
			Service.Returns(() => new Lazy<int>(() => 1));

			Logger.InfoMessage.Should().Contain("Lazy");
		}
		
	}

	public class Service
	{
		[LogInfo]
		public virtual void DoesSomething(string theString)
		{
			
		}

		[LogInfo]
		public virtual void DoesSomethingWithIEnumerable(IEnumerable<object> enumerable)
		{
			
		}

		[LogInfo]
		public virtual void DoesSomethingWithIEnumerable(IEnumerable<int> enumerable)
		{
			
		}

		[LogInfo]
		public virtual void DoesSomethingWithMultipleArguments(string theString, int index)
		{
			
		}

		[LogInfo]
		public virtual void DoesSomethingWithParamsArray(params object[] theThing)
		{
			
		}

		[LogInfo]
		public virtual T Returns<T>(Func<T> func )
		{
			return func.Invoke();
		}
	}
}