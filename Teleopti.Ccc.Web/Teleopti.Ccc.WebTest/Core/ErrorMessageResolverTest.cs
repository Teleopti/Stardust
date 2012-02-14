using System;
using System.Web.Mvc;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class ErrorMessageResolverTest
	{
		private ErrorMessageProvider target;

		[SetUp]
		public void Setup()
		{
			target = new ErrorMessageProvider();
		}

		[Test]
		public void ShouldDefaultReturnGenericMessage()
		{
			var ex = new Exception("bla bla");
			var handleErrorInfo = new HandleErrorInfo(ex, "Persmission", "Error");

			target.ResolveMessage(handleErrorInfo)
				.Should().Be.EqualTo(ErrorMessageProvider.GenericMessage);
		}

		[Test]
		public void ShouldDefaultReturnGenericShortMessage()
		{
			var ex = new Exception("bla bla");
			var handleErrorInfo = new HandleErrorInfo(ex, "Persmission", "Error");

			target.ResolveShortMessage(handleErrorInfo)
				.Should().Be.EqualTo(ErrorMessageProvider.GenericShortMessage);
		}


		[Test]
		public void PermissionErrorShouldReturnPermissionMessage()
		{
			const string errorMessage = "You dont have permission";
			var handleErrorInfo = new HandleErrorInfo(new PermissionException(errorMessage), "Persmission", "Error");

			var result = target.ResolveMessage(handleErrorInfo);

			result.Should().Be.EqualTo(errorMessage);
		}
	}
}