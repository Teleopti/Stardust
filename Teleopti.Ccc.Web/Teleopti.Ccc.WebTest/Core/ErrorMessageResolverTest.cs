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
		private const string controllerName = "Persmission";
		private const string actionName = "Error";
		private ErrorMessageProvider target;

		[SetUp]
		public void Setup()
		{
			target = new ErrorMessageProvider();
		}

		[Test]
		public void ShouldDefaultReturnGenericMessage()
		{
			var handleErrorInfo = creatHandleErrorInfo(new Exception("bla bla"));

			target.ResolveMessage(handleErrorInfo)
				.Should().Be.EqualTo(ErrorMessageProvider.GenericMessage);
		}

		[Test]
		public void ShouldDefaultReturnGenericShortMessage()
		{
			var handleErrorInfo = creatHandleErrorInfo(new Exception("bla bla"));

			target.ResolveShortMessage(handleErrorInfo)
				.Should().Be.EqualTo(ErrorMessageProvider.GenericShortMessage);
		}


		[Test]
		public void PermissionErrorShouldReturnPermissionMessage()
		{
			const string errorMessage = "You dont have permission";
			var handleErrorInfo = creatHandleErrorInfo(new PermissionException(errorMessage));

			var result = target.ResolveMessage(handleErrorInfo);

			result.Should().Be.EqualTo(errorMessage);
		}

		private HandleErrorInfo creatHandleErrorInfo(Exception ex)
		{
			// ReSharper disable All 
			return new HandleErrorInfo(ex, controllerName, actionName);
			// ReSharper restore All 
		}
	}
}