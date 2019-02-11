using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.WinCodeTest
{
	[TestFixture]
	public class AddOvertimeCommandTest
	{
		private AddOvertimeCommand _addOvertimeCommand;
		private MockRepository _mock;
		private ISchedulerStateHolder _schedulerStateHolder;
		private IScheduleViewBase _scheduleViewBase;
		private ISchedulePresenterBase _schedulePresenterBase;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;
		private IList<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSets;
		private IScheduleDay _scheduleDay;
		private IList<IScheduleDay> _scheduleDays;
		private IEditableShiftMapper _editableShiftMapper;
		private IPersonAssignment _personAssignment;
		private IPerson _person;
		private IPersonPeriod _personPeriod;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private DateOnly _dateOnly;
		private IPersonContract _personContract;
		private IContract _contract;
		private IAddOvertimeViewModel _addOvertimeViewModel;
		private IActivity _activity;
			
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulerStateHolder = _mock.StrictMock<ISchedulerStateHolder>();
			_scheduleViewBase = _mock.StrictMock<IScheduleViewBase>();
			_schedulePresenterBase = _mock.StrictMock<ISchedulePresenterBase>();
			_multiplicatorDefinitionSet = _mock.StrictMock<IMultiplicatorDefinitionSet>();
			_multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>{_multiplicatorDefinitionSet};
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_scheduleDays = new List<IScheduleDay>{_scheduleDay};
			_editableShiftMapper = _mock.StrictMock<IEditableShiftMapper>();
			_personAssignment = _mock.StrictMock<IPersonAssignment>();
			_person = _mock.StrictMock<IPerson>();
			_personPeriod = _mock.StrictMock<IPersonPeriod>();
			_dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_dateOnly = new DateOnly(2013, 1, 1);
			_personContract = _mock.StrictMock<IPersonContract>();
			_contract = _mock.StrictMock<IContract>();
			_addOvertimeViewModel = _mock.StrictMock<IAddOvertimeViewModel>();
			_activity = _mock.StrictMock<IActivity>();

			
		}

		[Test]
		public void ShouldNotUsePeriodFromPersonAssignmentWhenNoProjection()
		{
			var startDateTime = new DateTime(2013, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2013, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);	

			using (_mock.Record())
			{
				Expect.Call(_scheduleDay.Period).Return(new DateTimePeriod(2013, 1 ,1, 2013, 1, 2)).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainActivities()).Return(new List<MainShiftLayer>());
				Expect.Call(_personAssignment.OvertimeActivities()).Return(new List<OvertimeShiftLayer>());
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly).Repeat.AtLeastOnce();
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
				Expect.Call(_personPeriod.PersonContract).Return(_personContract);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(_contract.MultiplicatorDefinitionSetCollection).Return(new ReadOnlyCollection<IMultiplicatorDefinitionSet>(_multiplicatorDefinitionSets));
				Expect.Call(_schedulerStateHolder.CommonStateHolder).Return(new CommonStateHolder(_mock.DynamicMock<IDisableDeletedFilter>()));
				Expect.Call(_scheduleViewBase.CreateAddOvertimeViewModel( new List<IActivity>(),
				                                                         _multiplicatorDefinitionSets, null, dateTimePeriod,
				                                                         null))
																		 .Return(_addOvertimeViewModel)
																		 .IgnoreArguments();

				Expect.Call(_addOvertimeViewModel.Result).Return(true);
				Expect.Call(_addOvertimeViewModel.SelectedItem).Return(_activity);
				Expect.Call(_addOvertimeViewModel.SelectedMultiplicatorDefinitionSet).Return(_multiplicatorDefinitionSet);
				Expect.Call(_addOvertimeViewModel.SelectedPeriod).Return(dateTimePeriod);
				Expect.Call(() => _scheduleDay.CreateAndAddOvertime(_activity, dateTimePeriod, _multiplicatorDefinitionSet));
				Expect.Call(_schedulePresenterBase.ModifySchedulePart(_scheduleDays)).Return(true);
				Expect.Call(()=>_scheduleViewBase.RefreshRangeForAgentPeriod(_person, dateTimePeriod));
				Expect.Call(_person.IsAgent(_dateOnly)).Return(true);
			}

			using (_mock.Playback())
			{
				_addOvertimeCommand = new AddOvertimeCommand(_schedulerStateHolder, _scheduleViewBase, _schedulePresenterBase, _multiplicatorDefinitionSets, _scheduleDays, _editableShiftMapper);
				Assert.AreEqual(_addOvertimeCommand.DefaultPeriod, dateTimePeriod);
				_addOvertimeCommand.Execute();	
			}
		}
	}
}
