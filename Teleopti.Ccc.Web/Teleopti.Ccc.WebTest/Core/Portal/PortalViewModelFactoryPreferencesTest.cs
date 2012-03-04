using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal
{
	[TestFixture]
	public class PortalViewModelFactoryPreferencesTest
	{
		[Test]
		public void ShouldNotHavePreferencesNavigationItemIfNotPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.StandardPreferences))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>());

			var result = target.CreatePortalViewModel();

			var preferences = (from i in result.NavigationItems where i.Controller == "Preference" select i).SingleOrDefault();
			preferences.Should().Be.Null();
		}

		[Test]
		public void PreferenceShouldHaveDatePicker()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>());

			var result = target.CreatePortalViewModel();

			var preference = (from i in result.NavigationItems where i.Controller == "Preference" select i).Single();
			var datePicker = (from i in preference.ToolBarItems where i is ToolBarDatePicker select i).SingleOrDefault();
			datePicker.Should().Not.Be.Null();
		}

		[Test]
		public void PreferenceShouldHaveSplitButton()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>());

			var result = target.CreatePortalViewModel();

			var preference = (from i in result.NavigationItems where i.Controller == "Preference" select i).Single();
			var datePicker = (from i in preference.ToolBarItems where i is ToolBarSplitButton select i).SingleOrDefault();
			datePicker.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRetrieveShiftCategoriesIntoPreferenceSplitButton()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var shiftCategory = new ShiftCategory("Test") {DisplayColor = Color.Pink};
			shiftCategory.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveShiftCategoryOptions()).Return(new[] { shiftCategory });

			var target = new PortalViewModelFactory(new FakePermissionProvider(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = (from n in result.NavigationItems
			                             from t in n.ToolBarItems
			                             where n.Controller == "Preference"
			                                   && t.GetType() == typeof(ToolBarSplitButton)
			                             select t as ToolBarSplitButton)
				.Single();

			preferenceSplitButton.Options.Single().Value.Should().Be(shiftCategory.Id.ToString());
			preferenceSplitButton.Options.Single().Text.Should().Be(shiftCategory.Description.Name);
			preferenceSplitButton.Options.Single().Style.Name.Should().Be(shiftCategory.DisplayColor.ToStyleClass());
			preferenceSplitButton.Options.Single().Style.ColorHex.Should().Be(shiftCategory.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldRetrieveAbsencesIntoPreferenceSplitButton()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var absence = new Absence {Description = new Description("Test"), DisplayColor = Color.Plum};
			absence.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveAbsenceOptions()).Return(new[] { absence });

			var target = new PortalViewModelFactory(new FakePermissionProvider(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = (from n in result.NavigationItems
			                             from t in n.ToolBarItems
			                             where n.Controller == "Preference"
			                                   && t.GetType() == typeof(ToolBarSplitButton)
			                             select t as ToolBarSplitButton)
				.Single();

			preferenceSplitButton.Options.Single().Value.Should().Be(absence.Id.ToString());
			preferenceSplitButton.Options.Single().Text.Should().Be(absence.Description.Name);
			preferenceSplitButton.Options.Single().Style.Name.Should().Be(absence.DisplayColor.ToStyleClass());
			preferenceSplitButton.Options.Single().Style.ColorHex.Should().Be(absence.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldRetrieveDayOffsIntoPreferenceSplitButton()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var dayOff = new DayOffTemplate(new Description("Test")) {DisplayColor = Color.Purple};
			dayOff.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveDayOffOptions()).Return(new[] { dayOff });

			var target = new PortalViewModelFactory(new FakePermissionProvider(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = (from n in result.NavigationItems
			                             from t in n.ToolBarItems
			                             where n.Controller == "Preference"
			                                   && t.GetType() == typeof(ToolBarSplitButton)
			                             select t as ToolBarSplitButton)
				.Single();

			preferenceSplitButton.Options.Single().Value.Should().Be(dayOff.Id.ToString());
			preferenceSplitButton.Options.Single().Text.Should().Be(dayOff.Description.Name);
			preferenceSplitButton.Options.Single().Style.Name.Should().Be(dayOff.DisplayColor.ToStyleClass());
			preferenceSplitButton.Options.Single().Style.ColorHex.Should().Be(dayOff.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldHaveSplitterInPreferenceSplitButton()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			preferenceOptionsProvider.Stub(x => x.RetrieveShiftCategoryOptions()).Return(new[] { new ShiftCategory(" ") });
			preferenceOptionsProvider.Stub(x => x.RetrieveDayOffOptions()).Return(new[] { new DayOffTemplate(new Description()) });

			var target = new PortalViewModelFactory(permissionProvider, preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = (from n in result.NavigationItems
			                             from t in n.ToolBarItems
			                             where n.Controller == "Preference"
			                                   && t.GetType() == typeof(ToolBarSplitButton)
			                             select t as ToolBarSplitButton)
				.Single();

			preferenceSplitButton.Options.ElementAt(1).Should().Be.OfType<SplitButtonSplitter>();
		}

		[Test]
		public void PreferenceShouldHaveDeleteButton()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.Anything)).Return(true);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>());

			var result = target.CreatePortalViewModel();

			var deleteButton = (from n in result.NavigationItems
			                    from t in n.ToolBarItems
			                    where n.Controller == "Preference"
			                          && t is ToolBarButtonItem
			                          && ((ToolBarButtonItem)t).ButtonType == "delete"
			                    select t).SingleOrDefault();

			deleteButton.Should().Not.Be.Null();
		}

	}
}