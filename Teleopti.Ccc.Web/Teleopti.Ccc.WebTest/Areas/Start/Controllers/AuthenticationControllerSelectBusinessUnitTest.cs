using System;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerSelectBusinessUnitTest
	{
		private IWebLogOn logon;
		private AuthenticationController controller;
		private MockRepository mocks;
		private SignInBusinessUnitModel signInBusinessUnitModel;
		private IRedirector _redirector;

		[SetUp]
		public void Setup()
		{
			mocks=new MockRepository();
			logon = mocks.DynamicMock<IWebLogOn>();
			signInBusinessUnitModel = new SignInBusinessUnitModel {BusinessUnitId = Guid.NewGuid()};
			_redirector = MockRepository.GenerateMock<IRedirector>();
			controller = new AuthenticationController(null, null, logon, null, null, _redirector);
		}

		[TearDown]
		public void TearDown()
		{
			controller.Dispose();
		}

		[Test]
		public void ShouldSuccessfullyLogonAsWindowsUser()
		{
			signInBusinessUnitModel.DataSourceName = "arne weise";
			signInBusinessUnitModel.PersonId = Guid.NewGuid();

			using (mocks.Record())
			{
				logon.LogOn(signInBusinessUnitModel.BusinessUnitId, 
								signInBusinessUnitModel.DataSourceName,
				                signInBusinessUnitModel.PersonId,
									 AuthenticationTypeOption.Windows);
			}
			using (mocks.Playback())
			{
				controller.Logon(signInBusinessUnitModel);
			}
		}

		[Test]
		public void ShouldAfterSucessfulLogonRedirectToDefaultView()
		{
			_redirector.Stub(x => x.SignInRedirect()).Return(new RedirectResult("/"));

			var result = controller.Logon(signInBusinessUnitModel) as RedirectResult;

			result.Url.Should().Be.EqualTo("/");
			/*result.RouteValues["controller"].Should().Be.EqualTo(string.Empty);
			result.RouteValues["action"].Should().Be.EqualTo(string.Empty);
			 * */
		}

		[Test]
		public void ShouldViewErrorPartialViewWithFriendlyMessageWhenLogonFailsWithPermissionException()
		{
			signInBusinessUnitModel.DataSourceName = "Datasource";
			signInBusinessUnitModel.PersonId = Guid.NewGuid();

			using (mocks.Record())
			{
				Expect.Call(() => logon.LogOn(signInBusinessUnitModel.BusinessUnitId, signInBusinessUnitModel.DataSourceName, signInBusinessUnitModel.PersonId, AuthenticationTypeOption.Unknown)).Throw(
					new PermissionException("Permission Exception"));
			}
			using (mocks.Playback())
			{
				var result = controller.Logon(signInBusinessUnitModel) as PartialViewResult;

				result.ViewName.Should().Be.EqualTo("ErrorPartial");
				var viewModel = result.Model as ErrorViewModel;
				// Where will this be translated?
				viewModel.Message.Should().Not.Be.Empty();
			}
		}

	}
}