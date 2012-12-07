using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Team.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Team.Controllers
{
	[TestFixture]
	public class ApplicationControllerTest
	{
		[SetUp]
		public void Setup()
		{
			
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnDefaultViewInDefaultAction()
		{
			var target = new ApplicationController(null);
			var result = target.Index();
			result.FileName.Should().Be("~/Areas/Team/Content/Templates/index.html");
			result.ContentType.Should().Be("text/html");
		}

	}
}