using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.ConfigToolCode.Gui.StartupConfiguration;
using Teleopti.Analytics.Etl.Interfaces.Common;

namespace Teleopti.Analytics.Etl.ConfigToolCodeTest.Gui.StartupConfiguration
{
	[TestFixture]
	public class StartupConfigurationPresenterTest
	{
		private IStartupConfigurationView _view;
		private IConfigurationHandler _configurationHandler;
		private StartupConfigurationModel _model;
		private StartupConfigurationPresenter _target;

		[SetUp]
		public void Setup()
		{
			_view = MockRepository.GenerateMock<IStartupConfigurationView>();
			_configurationHandler = MockRepository.GenerateMock<IConfigurationHandler>();
			_model = new StartupConfigurationModel(_configurationHandler);
			_target = new StartupConfigurationPresenter(_view, _model);
		}

		[Test]
		public void ShouldFillCultureListOnView()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, null, null, null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.LoadCultureList(_model.CultureList));
		}

		[Test, SetCulture("en-US")]
		public void ShouldSetDefaultCultureFromCurrentCultureIfValueIsMissing()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, 15, "UTC", null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultCulture(_model.GetCultureItem(CultureInfo.CurrentCulture)));
		}

		[Test, SetCulture("en-US")]
		public void ShouldSetDefaultCultureFromCurrentCultureIfValueIsInvalid()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(100976, 15, "UTC", null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultCulture(_model.GetCultureItem(CultureInfo.CurrentCulture)));
		}

		[Test, SetCulture("en-US")]
		public void ShouldSetDefaultCultureFromDatabaseIfValueExist()
		{
			var swedishCulture = CultureInfo.GetCultureInfo("sv-SE");
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(swedishCulture.LCID, 15, "UTC", null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultCulture(_model.GetCultureItem(swedishCulture)));
		}

		[Test]
		public void ShouldFillIntervalLengthList()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, null, null, null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.LoadIntervalLengthList(_model.IntervalLengthList));
		}

		[Test]
		public void ShouldSetDefaultIntervalLengthTo15MinutesIfValueMissing()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, null, null, null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultIntervalLength(15));
		}

		[Test]
		public void ShouldSetDefaultIntervalLengthTo15MinutesIfValueIsInvalid()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, 99, null, null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultIntervalLength(15));
		}

		[Test]
		public void ShouldSetDefaultIntervalLengthFromDatabaseIfValueExist()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, 30, null, null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultIntervalLength(30));
		}

		[Test]
		public void ShouldFillTimeZoneList()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, null, null, null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.LoadTimeZoneList(_model.TimeZoneList));
		}

		[Test]
		public void ShouldSetTimeZoneToUsersIfValueIsMissing()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, null, null, null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultTimeZone(_model.GetTimeZoneItem(TimeZoneInfo.Local)));
		}

		[Test]
		public void ShouldSetTimeZoneToUsersIfValueIsInvalid()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, null, "dummie time zone", null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultTimeZone(_model.GetTimeZoneItem(TimeZoneInfo.Local)));
		}

		[Test]
		public void ShouldSetTimeZoneFromDatabaseIfValueExists()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, null, "UTC", null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultTimeZone(_model.GetTimeZoneItem(TimeZoneInfo.Utc)));
		}

		[Test]
		public void ShouldSaveConfiguration()
		{
			_target.Save(1033, 30, "UTC");

			_target.ConfigurationToSave.CultureId.Should().Be.EqualTo(1033);
			_target.ConfigurationToSave.IntervalLength.Should().Be.EqualTo(30);
			_target.ConfigurationToSave.TimeZoneCode.Should().Be.EqualTo("UTC");
			_configurationHandler.AssertWasCalled(x=>x.SaveBaseConfiguration(_target.ConfigurationToSave));
		}

		[Test]
		public void ShouldSetDefaultIntervalLengthAlreadyInUse()
		{
			_configurationHandler.Stub(x => x.IntervalLengthInUse).Return(30);
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, null, null, null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.SetDefaultIntervalLength(30));
		}

		[Test]
		public void ShouldDisableIntervalLengthControlWhenValueIsAlreadyInUse()
		{
			_configurationHandler.Stub(x => x.IntervalLengthInUse).Return(30);
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, null, null, null, false));
			_target.Initialize();
			_view.AssertWasCalled(x => x.DisableIntervalLength());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Analytics.Etl.ConfigToolCode.StartupConfiguration.IStartupConfigurationView.ShowErrorMessage(System.String)"), Test]
		public void ShouldDisableOkButtonAndShowErrorMessageWhenInvalidIntervalIsInUse()
		{
			_configurationHandler.Stub(x => x.BaseConfiguration).Return(new BaseConfiguration(null, 5, null, null, false));
			_configurationHandler.Stub(x => x.IntervalLengthInUse).Return(5);
			
			_target.Initialize();
			_view.AssertWasCalled(x => x.SelectedIntervalLengthValue);
			_view.AssertWasCalled(x => x.DisableOkButton());
			_view.AssertWasCalled(x => x.ShowErrorMessage("Interval Length already in use is 5 minutes. Supported Interval Length are 10, 15, 30 and 60 minutes."));
		}
	}
}
