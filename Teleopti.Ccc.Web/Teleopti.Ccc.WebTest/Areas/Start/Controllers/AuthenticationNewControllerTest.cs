using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.LayoutBase;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationNewControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnSignInView()
		{
			var layoutBaseViewModelFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var target = new AuthenticationNewController(layoutBaseViewModelFactory);
			var layoutBaseViewModel = new LayoutBaseViewModel();

			layoutBaseViewModelFactory.Stub(x => x.CreateLayoutBaseViewModel()).Return(layoutBaseViewModel);

			var result = target.SignIn();
			result.ViewName.Should().Be.EqualTo(string.Empty);
			Assert.That(result.ViewBag.LayoutBase, Is.SameAs(layoutBaseViewModel));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void DefaultActionShouldRenderDefaultView()
		{
			var target = new AuthenticationNewController(null);
			var result = target.Index();
			result.ViewName.Should().Be.EqualTo(string.Empty);
		}

	}


}
