using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public class IntegrationIoCTestAttribute : Attribute, ITestAction
	{
		private IoCTestService _service;

		public ActionTargets Targets => ActionTargets.Test;

		public IToggleManager Toggles;

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

			var disabledToggle = _service
				.QueryAllAttributes<OnlyRunIfEnabled>()
				.FirstOrDefault(a => !Toggles.IsEnabled(a.Toggle));
			if (disabledToggle != null)
				Assert.Ignore($"Ignoring test {testDetails.Name} because toggle {disabledToggle.Toggle} is disabled");

			BeforeTest();
		}

		public void AfterTest(ITest testDetails)
		{
			AfterTest();
			_service = null;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class OnlyRunIfEnabled : Attribute
	{
		public Toggles Toggle { get; }

		public OnlyRunIfEnabled(Toggles toggle)
		{
			Toggle = toggle;
		}
	}
}