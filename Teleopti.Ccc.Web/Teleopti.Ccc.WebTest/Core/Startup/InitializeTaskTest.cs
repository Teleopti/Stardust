using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;
using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class InitializeTaskTest
	{
		private InitializeApplicationTask target;
		private MockRepository mocks;
		private IInitializeApplication initializeApplication;
		private IPhysicalApplicationPath physicalApplicationPath;
		private ISettings settings;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			initializeApplication = mocks.DynamicMock<IInitializeApplication>();
			settings = mocks.DynamicMock<ISettings>();
			physicalApplicationPath = mocks.DynamicMock<IPhysicalApplicationPath>();
			target = new InitializeApplicationTask(initializeApplication, settings, physicalApplicationPath);
		}

		[Test]
		public void InitializeApplicationShouldStart()
		{
			using (mocks.Record())
			{
				Expect.Call(settings.nhibConfPath()).Return("path");
				Expect.Call(physicalApplicationPath.Get()).Return("applicationPath");
				initializeApplication.Start(null, null, null, false);
				LastCall.IgnoreArguments().Constraints(
					Is.TypeOf(typeof (WebState)),
					Is.TypeOf(typeof (LoadPasswordPolicyService)),
					Is.TypeOf(typeof(IDictionary<string, string>)),
					Is.Equal(false)
					);
			}
			using (mocks.Playback())
			{
				target.Execute(null);
			}
		}
	}
}