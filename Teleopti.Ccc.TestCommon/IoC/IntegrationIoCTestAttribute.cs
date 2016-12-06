using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public class IntegrationIoCTestAttribute : Attribute, ITestAction
	{
		private IoCTestService _service;

		public ActionTargets Targets => ActionTargets.Test;

		protected virtual void BeforeTest()
		{
		}

		protected virtual void AfterTest()
		{
		}

		public void BeforeTest(ITest testDetails)
		{
			_service = new IoCTestService(testDetails, this);
			_service.InjectFrom(IntegrationIoCTest.Container);
			BeforeTest();
		}

		public void AfterTest(ITest testDetails)
		{
			AfterTest();
			_service = null;
		}

	}
}