using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Portal.ViewModelFactory
{
	[TestFixture]
	public class PortalViewModelFactoryPreferencesTest
	{
		[Test]
		public void ShouldNotHavePreferencesNavigationItemIfNotPermission()
		{
			var permissionProvider = NoPermissionToPreferences();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			result.Controller<PreferenceNavigationItem>().Should().Be.Null();
		}

		[Test]
		public void ShouldHavePreferencesNavigationItemIfOnlyPermissionToExtendedPreferences()
		{
			var permissionProvider = NoPermissionToStandardPreferences();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			result.Controller<PreferenceNavigationItem>().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHavePreferencesNavigationItemIfOnlyPermissionToStandardPreferences()
		{
			var permissionProvider = NoPermissionToExtendedPreferences();
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			result.Controller<PreferenceNavigationItem>().Should().Not.Be.Null();
		}

		private IPermissionProvider NoPermissionToPreferences()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Matches(new PredicateConstraint<string>(s => s != DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb &&
			                                                                                                                         s != DefinedRaptorApplicationFunctionPaths.StandardPreferences)))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb)).Return(false);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences)).Return(false);

			return permissionProvider;
		}

		private IPermissionProvider NoPermissionToStandardPreferences()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.StandardPreferences))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.StandardPreferences)).Return(false);
			return permissionProvider;
		}

		private IPermissionProvider NoPermissionToExtendedPreferences()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(Arg<string>.Is.NotEqual(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb))).Return(true);
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb)).Return(false);
			return permissionProvider;
		}

		[Test]
		public void PreferenceShouldHaveDatePicker()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var datePicker = result.ControllerItems<ToolBarDatePicker, PreferenceNavigationItem>().SingleOrDefault();

			datePicker.Should().Not.Be.Null();
		}

		[Test]
		public void PreferenceShouldNotHaveSplitButtonWhenExtendedPreferencesIsPermitted()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = result.ControllerItems<ToolBarSplitButton, PreferenceNavigationItem>().SingleOrDefault();

			preferenceSplitButton.Should().Be.Null();
		}

		[Test]
		public void PreferenceShouldHaveSplitButton()
		{
			var target = new PortalViewModelFactory(NoPermissionToExtendedPreferences(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = result.ControllerItems<ToolBarSplitButton, PreferenceNavigationItem>().SingleOrDefault();

			preferenceSplitButton.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldRetrieveShiftCategoriesIntoPreferenceSplitButton()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var shiftCategory = new ShiftCategory("Test") {DisplayColor = Color.Pink};
			shiftCategory.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveShiftCategoryOptions()).Return(new[] { shiftCategory });

			var target = new PortalViewModelFactory(NoPermissionToExtendedPreferences(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = result.ControllerItems<ToolBarSplitButton, PreferenceNavigationItem>().SingleOrDefault();

			preferenceSplitButton.Options.Single().Value.Should().Be(shiftCategory.Id.ToString());
			preferenceSplitButton.Options.Single().Text.Should().Be(shiftCategory.Description.Name);
			preferenceSplitButton.Options.Single().Color.Should().Be(shiftCategory.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldRetrieveAbsencesIntoPreferenceSplitButton()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var absence = new Absence {Description = new Description("Test"), DisplayColor = Color.Plum};
			absence.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveAbsenceOptions()).Return(new[] { absence });

			var target = new PortalViewModelFactory(NoPermissionToExtendedPreferences(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = result.ControllerItems<ToolBarSplitButton, PreferenceNavigationItem>().SingleOrDefault();

			preferenceSplitButton.Options.Single().Value.Should().Be(absence.Id.ToString());
			preferenceSplitButton.Options.Single().Text.Should().Be(absence.Description.Name);
			preferenceSplitButton.Options.Single().Color.Should().Be(absence.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldRetrieveDayOffsIntoPreferenceSplitButton()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var dayOff = new DayOffTemplate(new Description("Test")) {DisplayColor = Color.Purple};
			dayOff.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveDayOffOptions()).Return(new[] { dayOff });

			var target = new PortalViewModelFactory(NoPermissionToExtendedPreferences(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = result.ControllerItems<ToolBarSplitButton, PreferenceNavigationItem>().SingleOrDefault();

			preferenceSplitButton.Options.Single().Value.Should().Be(dayOff.Id.ToString());
			preferenceSplitButton.Options.Single().Text.Should().Be(dayOff.Description.Name);
			preferenceSplitButton.Options.Single().Color.Should().Be(dayOff.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldHaveSplitterInPreferenceSplitButton()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			preferenceOptionsProvider.Stub(x => x.RetrieveShiftCategoryOptions()).Return(new[] { new ShiftCategory(" ") });
			preferenceOptionsProvider.Stub(x => x.RetrieveDayOffOptions()).Return(new[] { new DayOffTemplate(new Description()) });

			var target = new PortalViewModelFactory(NoPermissionToExtendedPreferences(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var preferenceSplitButton = result.ControllerItems<ToolBarSplitButton, PreferenceNavigationItem>().SingleOrDefault();

			preferenceSplitButton.Options.ElementAt(1).Should().Be.OfType<PreferenceOptionSplit>();
		}

		[Test]
		public void PreferenceShouldHaveDeleteButton()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var deleteButton = result.ControllerItems<ToolBarButtonItem, PreferenceNavigationItem>().SingleOrDefault(t => t.ButtonType == "delete");

			deleteButton.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldHaveAddExtendedPreferenceButton()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var button = result.ControllerItems<ToolBarButtonItem>("Preference").FirstOrDefault(i => i.ButtonType == "add-extended");

			button.Should().Not.Be.Null();
			button.Title.Should().Be(Resources.Preference);
		}

		[Test]
		public void ShouldNotHaveAddExtendedPreferenceButtonIfNoPermission()
		{
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb)).Return(false);
			var target = new PortalViewModelFactory(permissionProvider, MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var button = result.ControllerItems<ToolBarButtonItem>("Preference").FirstOrDefault(i => i.ButtonType == "add-extended");
			button.Should().Be.Null();
		}

		[Test]
		public void ShouldHaveShiftCategoriesInPreferenceOptions()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var shiftCategory = new ShiftCategory("Test") {DisplayColor = Color.Pink};
			shiftCategory.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveShiftCategoryOptions()).Return(new[] { shiftCategory });
			var target = new PortalViewModelFactory(new FakePermissionProvider(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Value.Should().Be(shiftCategory.Id.ToString());
			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Text.Should().Be(shiftCategory.Description.Name);
			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Color.Should().Be(shiftCategory.DisplayColor.ToHtml());
			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Extended.Should().Be.True();
		}

		[Test]
		public void ShouldHaveAbsenceInPreferenceOptions()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var absence = new Absence { Description = new Description("Test"), DisplayColor = Color.Plum };
			absence.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveAbsenceOptions()).Return(new[] { absence });

			var target = new PortalViewModelFactory(new FakePermissionProvider(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Value.Should().Be(absence.Id.ToString());
			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Text.Should().Be(absence.Description.Name);
			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Color.Should().Be(absence.DisplayColor.ToHtml());
			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Extended.Should().Be.False();
		}

		[Test]
		public void ShouldHaveDayOffInPreferenceOptions()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var dayOff = new DayOffTemplate(new Description("Test")) { DisplayColor = Color.Purple };
			dayOff.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveDayOffOptions()).Return(new[] { dayOff });

			var target = new PortalViewModelFactory(new FakePermissionProvider(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Value.Should().Be(dayOff.Id.ToString());
			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Text.Should().Be(dayOff.Description.Name);
			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Color.Should().Be(dayOff.DisplayColor.ToHtml());
			result.Controller<PreferenceNavigationItem>().PreferenceOptions.Single().Extended.Should().Be.False();
		}

		[Test]
		public void ShouldHaveActivityInActivityOptions()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();
			var activity = new Activity("Test") { DisplayColor = Color.BurlyWood };
			activity.SetId(Guid.NewGuid());
			preferenceOptionsProvider.Stub(x => x.RetrieveActivityOptions()).Return(new[] { activity });

			var target = new PortalViewModelFactory(new FakePermissionProvider(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			result.Controller<PreferenceNavigationItem>().ActivityOptions.Single().Value.Should().Be(activity.Id.ToString());
			result.Controller<PreferenceNavigationItem>().ActivityOptions.Single().Text.Should().Be(activity.Description.Name);
			result.Controller<PreferenceNavigationItem>().ActivityOptions.Single().Color.Should().Be(activity.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldOnlyGetShiftCategoriesOnce()
		{
			var preferenceOptionsProvider = MockRepository.GenerateMock<IPreferenceOptionsProvider>();

			var target = new PortalViewModelFactory(new FakePermissionProvider(), preferenceOptionsProvider, MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateMock<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			target.CreatePortalViewModel();

			preferenceOptionsProvider.AssertWasCalled(x => x.RetrieveShiftCategoryOptions(), o => o.Repeat.Once());

		}

		[Test]
		public void ShouldHaveMustHaveNumberText()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var button = result.ControllerItems<ToolBarTextItem>("Preference").FirstOrDefault(i => i.Id == "must-have-numbers");

			button.Should().Not.Be.Null();
			button.Text.Should().Be("0(0)");
		}

		[Test]
		public void ShouldHaveMustHaveDeleteButton()
		{
			var target = new PortalViewModelFactory(new FakePermissionProvider(), MockRepository.GenerateMock<IPreferenceOptionsProvider>(), MockRepository.GenerateMock<ILicenseActivator>(), MockRepository.GenerateStub<IPushMessageProvider>(), MockRepository.GenerateMock<ILoggedOnUser>());

			var result = target.CreatePortalViewModel();

			var button = result.ControllerItems<ToolBarButtonItem>("Preference").FirstOrDefault(i => i.ButtonType == "must-have-delete");

			button.Should().Not.Be.Null();
			button.Title.Should().Be(Resources.Delete);
			button.Icon.Should().Be("heart-delete");
		}

	}

	public static class Ext
	{
		public static T Controller<T>(this PortalViewModel model) where T : SectionNavigationItem
		{
			return (from ni in model.NavigationItems
				   where ni is T
				   select ni as T).SingleOrDefault();
		}

		public static IEnumerable<T> ControllerItems<T, TController>(this PortalViewModel model)
			where T : ToolBarItemBase
			where TController : SectionNavigationItem 
		{
			return from ni in model.NavigationItems
				   where ni is TController
				   from ti in ni.ToolBarItems
				   where ti is T
				   select ti as T;
		}

		public static IEnumerable<T> ControllerItems<T>(this PortalViewModel model, string controller) where T : ToolBarItemBase
		{
			return from ni in model.NavigationItems
			       from ti in ni.ToolBarItems
			       where ti is T
				   && ni.Controller == controller
			       select ti as T;
		} 
	}

}