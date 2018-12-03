using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class PublishScheduleDatePresenterTest
	{
		private PublishScheduleDatePresenter _target;
		private MockRepository _mock;
		private IPublishScheduleDateView _view;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IPerson _person1;
		private IPerson _person2;
		private IWorkflowControlSet _workflowControlSet1;
		private IWorkflowControlSet _workflowControlSet2;
		private IList<IScheduleDay> _scheduleDays;
		private IDateOnlyAsDateTimePeriod _dateOnlyPeriodAsDateTimePeriod1;
		private IDateOnlyAsDateTimePeriod _dateOnlyPeriodAsDateTimePeriod2;
		private DateOnly _dateOnly1;
		private DateOnly _dateOnly2;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IPublishScheduleDateView>();
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_person1 = new Person();
			_person2 = new Person();
			_workflowControlSet1 = _mock.StrictMock<IWorkflowControlSet>();
			_workflowControlSet2 = _mock.StrictMock<IWorkflowControlSet>();
			_person1.WorkflowControlSet = _workflowControlSet1;
			_person2.WorkflowControlSet = _workflowControlSet2;
			_scheduleDays = new List<IScheduleDay>{_scheduleDay1, _scheduleDay2};
			_dateOnlyPeriodAsDateTimePeriod1 = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_dateOnlyPeriodAsDateTimePeriod2 = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_dateOnly1 = new DateOnly(2014,1,10);
			_dateOnly2 = new DateOnly(2014, 1, 20);
			_target = new PublishScheduleDatePresenter(_view, _scheduleDays);
		}

		[Test]
		public void ShouldInitialize()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1);
				Expect.Call(_scheduleDay2.Person).Return(_person2);
				Expect.Call(_workflowControlSet1.Name).Return("B").Repeat.AtLeastOnce();
				Expect.Call(_workflowControlSet2.Name).Return("A").Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyPeriodAsDateTimePeriod1);
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyPeriodAsDateTimePeriod2);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod1.DateOnly).Return(_dateOnly1);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod2.DateOnly).Return(_dateOnly2);
				Expect.Call(()=>_view.SetDate(_dateOnly2));
				Expect.Call(()=>_view.SetWorkflowControlSets(new List<IWorkflowControlSet> {_workflowControlSet2, _workflowControlSet1}));
			}

			using (_mock.Playback())
			{
				_target.Initialize();
				Assert.AreEqual(2, _target.WorkflowControlSets.Count);
				Assert.AreEqual("A", _target.WorkflowControlSets[0].Name);
				Assert.AreEqual("B", _target.WorkflowControlSets[1].Name);
				Assert.AreEqual(_dateOnly2, _target.PublishToDate);
			}
		}

		[Test]
		public void ShouldDisabelButtonOkWhenNoSelectedSchedules()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _view.DisableOk());	
			}

			using (_mock.Playback())
			{
				_scheduleDays.Clear();
				_target.Initialize();
			}	
		}

		[Test]
		public void ShouldDisableButtonOkWhenNoSelectedControlSets()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1);
				Expect.Call(_scheduleDay2.Person).Return(_person2);
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyPeriodAsDateTimePeriod1);
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyPeriodAsDateTimePeriod2);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod1.DateOnly).Return(_dateOnly1);
				Expect.Call(_dateOnlyPeriodAsDateTimePeriod2.DateOnly).Return(_dateOnly2);
				Expect.Call(() => _view.DisableOk());	
			}

			using (_mock.Playback())
			{
				_person1.WorkflowControlSet = null;
				_person2.WorkflowControlSet = null;

				_target.Initialize();
				Assert.AreEqual(0, _target.WorkflowControlSets.Count);
			}
		}
	}
}
