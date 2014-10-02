using System;
using System.IO;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core
{
	[TestFixture]
	public class IpAddressResolverTest
	{
		private IpAddressResolver _target;

		[SetUp]
		public void Setup()
		{
			var httpContext = new FakeHttpContext("");
			var current = new FakeCurrentHttpContext(httpContext);
			_target = new IpAddressResolver(current);
		}

		[Test]
		public void ShouldAskForIp()
		{
			Assert.That(_target.GetIpAddress(),Is.EqualTo("172.168.1.1"));
		}

	}


}