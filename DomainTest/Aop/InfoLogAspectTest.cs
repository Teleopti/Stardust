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
	public class InfoLogAspectTest : ISetup
	{
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<Service>();
			var fakeLogger = new LogSpy();
			system.UseTestDouble(fakeLogger).For<ILog>();
			system.UseTestDouble(new FakeLogManagerWrapper(fakeLogger)).For<ILogManagerWrapper>();
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
		public void ShouldNotLogClassName_AlreadyHandledInLog4Net()
		{
			Service.DoesSomething(null);

			Logger.InfoMessage.Should().Not.Contain("Service");
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
			Service.ReturnsInt();

			Logger.InfoMessage.Should().Contain("Result : 1");
		}

		[Test]
		public void ShouldLogReturnValueNull()
		{
			Service.ReturnsNullString();

			Logger.InfoMessage.Should().Contain("Result : null");
		}

		[Test]
		public void ShouldLogEnumerableReturnCount()
		{
			Service.ReturnsArrayAsIEnumerable();

			Logger.InfoMessage.Should().Contain("Result : Count = 1");
		}


		[Test]
		public void ShouldLogSomethingReadableWhenReturningIEnumerable()
		{
			Service.ReturnsIEnumerable();

			Logger.InfoMessage.Should().Contain("Result : Enumerable");
		}
		
		[Test]
		public void ShouldLogEnumerableReturnCountFromList()
		{
			Service.ReturnsListAsIEnumerable();

			Logger.InfoMessage.Should().Contain("Result : Count = 3");
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

		[InfoLog]
		public virtual void DoesSomethingWithParamsArray(params object[] theThing)
		{
			
		}

		[InfoLog]
		public virtual int ReturnsInt()
		{
			return 1;
		}
		
		[InfoLog]
		public virtual IEnumerable<int> ReturnsArrayAsIEnumerable()
		{
			return new []{1};
		}

		[InfoLog]
		public virtual IEnumerable<int> ReturnsIEnumerable()
		{
			return from i in Enumerable.Range(0, 10) select i;
		}

		[InfoLog]
		public virtual IEnumerable<int> ReturnsListAsIEnumerable()
		{
			return new List<int>{1,2,3};
		}

		[InfoLog]
		public virtual string ReturnsNullString()
		{
			return null;
		}
	}
}