using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Reporting
{
	[TestFixture]
	public class ScheduleReportDialogGraphicalModelTest
	{
		private ScheduleReportDialogGraphicalModel _model;

		[SetUp]
		public void Setup()
		{
			_model = new ScheduleReportDialogGraphicalModel();	
		}

		[Test]
		public void ShouldSetDefaultProperties()
		{
			Assert.IsTrue(_model.Individual);
			Assert.IsTrue(_model.OneFileForSelected);
			Assert.IsTrue(_model.SortOnStartTime);
            Assert.IsFalse(_model.ShowPublicNote);
		}

		[Test]
		public void ShouldSetTeamProperty()
		{
			_model.Team = true;
			Assert.IsTrue(_model.Team);

			_model.Team = false;
			Assert.IsFalse(_model.Team);
		}

		[Test]
		public void ShouldSetIndividualProperty()
		{
			_model.Individual = true;
			Assert.IsTrue(_model.Individual);

			_model.Individual = false;
			Assert.IsFalse(_model.Individual);
		}

		[Test]
		public void ShouldSetOneFileForSelectedProperty()
		{
			_model.OneFileForSelected = true;
			Assert.IsTrue(_model.OneFileForSelected);

			_model.OneFileForSelected = false;
			Assert.IsFalse(_model.OneFileForSelected);
		}

        [Test]
        public void ShouldSetShowPublicNoteProperty()
        {
            _model.ShowPublicNote = true;
            Assert.IsTrue(_model.ShowPublicNote);

            _model.ShowPublicNote = false;
            Assert.IsFalse(_model.ShowPublicNote);
        }

		[Test]
		public void ShouldSetSortOnAgentNameProperty()
		{
			_model.SortOnAgentName = true;
			Assert.IsTrue(_model.SortOnAgentName);

			_model.SortOnAgentName = false;
			Assert.IsFalse(_model.SortOnAgentName);
		}

		[Test]
		public void ShouldSetSortOnStartTimeProperty()
		{
			_model.SortOnStartTime = true;
			Assert.IsTrue(_model.SortOnStartTime);

			_model.SortOnStartTime = false;
			Assert.IsFalse(_model.SortOnStartTime);
		}

		[Test]
		public void ShouldSetSortOnEndTimeProperty()
		{
			_model.SortOnEndTime = true;
			Assert.IsTrue(_model.SortOnEndTime);

			_model.SortOnEndTime = false;
			Assert.IsFalse(_model.SortOnEndTime);
		}

	}
}
