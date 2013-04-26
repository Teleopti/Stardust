using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Requests
{
	[TestFixture]
	public class MyTimeWebPersonRequestCheckAuthorizationTest
	{
		[Test]
		public void ShouldHaveCoverage()
		{
			var target = new MyTimeWebPersonRequestCheckAuthorization();
			target.HasEditRequestPermission(null);
			target.HasViewRequestPermission(null);
			target.VerifyEditRequestPermission(null);
		}
	}
}
