
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
	[TestFixture]
	public class ScheduleReportDialogGraphicalPresenterTest
	{
		private ScheduleReportDialogGraphicalPresenter _presenter;
		private ScheduleReportDialogGraphicalModel _model;
		private IScheduleReportDialogGraphicalView _view;
		private MockRepository _mockRepository;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_view = _mockRepository.StrictMock<IScheduleReportDialogGraphicalView>();
			_model = new ScheduleReportDialogGraphicalModel();
			_presenter = new ScheduleReportDialogGraphicalPresenter(_view, _model);
			
		}

		[Test]
		public void ShouldSetTeamOnModelTrue()
		{
			_presenter.OnRadioButtonTeamCheckedChanged(true);
			Assert.IsTrue(_model.Team);
		}

		[Test]
		public void ShouldSetIndividualOnModelTrue()
		{
			_presenter.OnRadioButtonIndividualCheckedChanged(true);
			Assert.IsTrue(_model.Individual);
		}

		[Test]
		public void ShouldSetSortOnAgentNameOnModelTrue()
		{
			_presenter.OnRadioButtonSortOnAgentNameCheckedChanged(true);
			Assert.IsTrue(_model.SortOnAgentName);
		}

		[Test]
		public void ShouldSetSortOnStartTimeOnModelTrue()
		{
			_presenter.OnRadioButtonSortOnStartTimeCheckedChanged(true);
			Assert.IsTrue(_model.SortOnStartTime);
		}

		[Test]
		public void ShouldSetSortOnEndTimeOnModelTrue()
		{
			_presenter.OnRadioButtonSortOnEndTimeCheckedChanged(true);
			Assert.IsTrue(_model.SortOnEndTime);
		}

		[Test]
		public void ShouldSetOneFileForSelectedOnModelTrue()
		{
			_presenter.OnCheckBoxSingleFileCheckedChanged(true);
			Assert.IsTrue(_model.OneFileForSelected);
		}

        [Test]
        public void ShouldSetShowPublicNoteOnModelTrue()
        {
            _presenter.OnCheckBoxShowPublicNoteCheckedChanged(true);
            Assert.IsTrue(_model.ShowPublicNote);
        }

		[Test]
		public void ShouldCancel()
		{
			using(_mockRepository.Record())
			{
				Expect.Call(()=>_view.UserCancel());
			}

			using(_mockRepository.Playback())
			{
				_presenter.OnButtonCancelClick();
			}
		}

		[Test]
		public void ShouldOk()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(()=>_view.UserOk());
			}

			using (_mockRepository.Playback())
			{
				_presenter.OnButtonOkClick();
			}
		}

		[Test]
		public void ShouldSetDialogBackColorAndUpdateFromModelOnLoad()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_view.BackgroundColor = ColorHelper.DialogBackColor());
				Expect.Call(()=>_view.UpdateFromModel(_model));
			}

			using (_mockRepository.Playback())
			{
				_presenter.OnLoad();
			}	
		}
	}
}
