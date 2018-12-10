using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;


namespace Teleopti.Ccc.WinCodeTest.Presentation
{
	[TestFixture]
	public class ReportSettingsScheduleTimeVersusTargetTimePresenterTest
	{
		private MockRepository _mocks;
		private ReportSettingsScheduleTimeVersusTargetTimePresenter _target;
		private IReportSettingsScheduleTimeVersusTargetTimeView _view;
		private readonly IScenario _scenario = new Scenario("scenario");
        private readonly DateOnlyPeriod _period = new DateOnlyPeriod();
        private readonly IList<IPerson> _persons = new List<IPerson>();

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_view = _mocks.StrictMock<IReportSettingsScheduleTimeVersusTargetTimeView>();
			_target = new ReportSettingsScheduleTimeVersusTargetTimePresenter(_view);
		}

		[Test]
		public void ShouldInitialize()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _view.InitAgentSelector());
			}

			using (_mocks.Playback())
			{
				_target.InitializeSettings();
			}
		}


		[Test]
		public void ShouldGetSettingsModel()
		{
			using (_mocks.Record())
			{
				Expect.Call(_view.Period).Return(_period).Repeat.Once();
				Expect.Call(_view.Persons).Return(_persons).Repeat.Once();
				Expect.Call(_view.Scenario).Return(_scenario).Repeat.Once();
			}

			using (_mocks.Playback())
			{
				var model = _target.GetSettingsModel;
				Assert.IsNotNull(model);
			}
		}
	}
}
