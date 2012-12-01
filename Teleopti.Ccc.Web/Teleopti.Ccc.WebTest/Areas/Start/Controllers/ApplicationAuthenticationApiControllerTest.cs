using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class ApplicationAuthenticationApiControllerTest
	{
		[Test]
		public void ShouldAuthenticateUser()
		{
			var target = new ApplicationAuthenticationApiController();
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult { Successful = true });

			target.CheckPassword(authenticationModel);

			authenticationModel.AssertWasCalled(x => x.AuthenticateUser());
		}

		[Test]
		public void ShouldReturnErrorIfAuthenticationFailed()
		{
			var target = new StubbingControllerBuilder().CreateController<ApplicationAuthenticationApiController>();
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			const string message = "test";
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult { Successful = false, Message = message });

			var result = target.CheckPassword(authenticationModel);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(message);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(message);
		}

		[Test]
		public void ShouldReturnErrorIfPasswordExpired()
		{
			var target = new StubbingControllerBuilder().CreateController<ApplicationAuthenticationApiController>();
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			const string message = "test";
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult { Successful = false, Message = message , PasswordExpired = true});

			var result = target.CheckPassword(authenticationModel);

			target.Response.StatusCode.Should().Be(400);
			target.Response.SubStatusCode.Should().Be(1);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(message);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(message);
		}

		[Test]
		public void ShouldReturnWarningIfPasswordWillExpire()
		{
			var target = new ApplicationAuthenticationApiController();
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			const string message = "test";
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult { Successful = true, HasMessage = true, Message = message });

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.WillExpireSoon.Should().Be.True();
			warning.Warning.Should().Be.EqualTo(message);
		}

		[Test]
		public void ShouldNotReturnWarningIfPasswordWillNotExpire()
		{
			var target = new ApplicationAuthenticationApiController();
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			const string message = "test";
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult { Successful = true, HasMessage = false});

			var result = target.CheckPassword(authenticationModel);

			var warning = result.Data as PasswordWarningViewModel;
			warning.WillExpireSoon.Should().Be.False();
		}
	}
}