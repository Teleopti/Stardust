using System.Threading;
using AutoMapper;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class SettingsControllerTest
	{
		private IMappingEngine mappingEngine;
		private ILoggedOnUser loggedOnUser;
		private IModifyPassword modifyPassword;

		[SetUp]
		public void Setup()
		{
			mappingEngine = MockRepository.GenerateStrictMock<IMappingEngine>();
			loggedOnUser = MockRepository.GenerateStrictMock<ILoggedOnUser>();
			modifyPassword = MockRepository.GenerateStrictMock<IModifyPassword>();
		}

		[Test]
		public void IndexShouldReturnViewModel()
		{
			using (var target = new SettingsController(mappingEngine, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null)))
			{
				var viewModel = new SettingsViewModel();
				var person = new Person();
				loggedOnUser.Expect(obj => obj.CurrentUser()).Return(person);
				mappingEngine.Expect(obj => obj.Map<IPerson, SettingsViewModel>(person)).Return(viewModel);
				var res = target.Index();
				res.Model.Should().Be.SameInstanceAs(viewModel);
			}
		}

		[Test]
		public void PassWordShouldReturnCorrectView()
		{
			using (var target = new SettingsController(mappingEngine, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null)))
			{
				var res = target.Password();
				res.ViewName.Should().Be.EqualTo("PasswordPartial");
			}
		}

		[Test]
		public void ShouldUpdateCulture()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (var target = new SettingsController(null, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null)))
			{
				target.UpdateCulture(1034);
			}
			person.PermissionInformation.Culture().LCID.Should().Be.EqualTo(1034);
		}

		[Test]
		public void ShouldUpdateCultureRepresentingNull()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (var target = new SettingsController(null, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null)))
			{
				target.UpdateCulture(-1);
			}
			person.PermissionInformation.Culture().Should().Be.EqualTo(Thread.CurrentThread.CurrentCulture);
		}

		[Test]
		public void ShouldUpdateUiCulture()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (var target = new SettingsController(null, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null)))
			{
				target.UpdateUiCulture(1034);
			}
			person.PermissionInformation.UICulture().LCID.Should().Be.EqualTo(1034);
		}

		[Test]
		public void ShouldUpdateUiCultureRepresentingNull()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			using (var target = new SettingsController(null, loggedOnUser, null, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null)))
			{
				target.UpdateUiCulture(-1);
			}
			person.PermissionInformation.Culture().Should().Be.EqualTo(Thread.CurrentThread.CurrentCulture);
		}


		[Test]
		public void ShouldChangePassword()
		{
			var person = new Person();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(person);
			modifyPassword.Expect(x => x.Change(person, "old", "new")).Return(true);
			using (var target = new SettingsController(null, loggedOnUser, modifyPassword, new PersonPersister(MockRepository.GenerateMock<IMbCacheFactory>(), null)))
			{
				target.ChangePassword(new ChangePasswordViewModel {NewPassword = "new", OldPassword = "old"});
			}
		}
	}
}