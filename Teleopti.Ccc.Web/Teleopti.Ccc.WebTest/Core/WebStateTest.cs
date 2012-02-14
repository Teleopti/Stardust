using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;

namespace Teleopti.Ccc.WebTest.Core
{
	//remove this one later....
	//need to understand Robins refactoring first
	[TestFixture]
	public class WebStateTest
	{
		private WebState target;

		[SetUp]
		public void Setup()
		{
			target = new WebState();
		}

		[Test]
		public void DummyTests()
		{
			//will be removed
			target.ClearSession();
			target.SetSessionData(null);
			target.SessionScopeData.Should().Be.Null();
		}
	}
}