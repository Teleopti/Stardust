using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Controllers
{
	[TestFixture]
	public class SettingsControllerTest
	{
		private SettingsController target;
		private IMappingEngine mappingEngine;
		private ILoggedOnUser loggedOnUser;

		[SetUp]
		public void Setup()
		{
			mappingEngine = MockRepository.GenerateStrictMock<IMappingEngine>();
			loggedOnUser = MockRepository.GenerateStrictMock<ILoggedOnUser>();
			target = new SettingsController(mappingEngine, loggedOnUser);
		}

		[Test]
		public void IndexShouldReturnViewModel()
		{
			var viewModel = new SettingsViewModel();
			var person = new Person();
			loggedOnUser.Expect(obj => obj.CurrentUser()).Return(person);
			mappingEngine.Expect(obj => obj.Map<IPerson, SettingsViewModel>(person)).Return(viewModel);
			var res = target.Index();
			res.Model.Should().Be.SameInstanceAs(viewModel);
		}
	}
}