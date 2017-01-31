using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamSchedulingTest
    {

        private MockRepository _mock;
        private ITeamScheduling _target;
        private IShiftProjectionCache _shiftProjectionCache;
        private IScheduleMatrixPro _matrix;
        private Group _group;
        private IResourceCalculateDelayer _resourceCalculateDelayer;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
		private IEditableShift _mainShift;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private ITeamBlockInfo _teamBlockInfo;
        private ITeamInfo _teaminfo;
        private IBlockInfo _blockInfo;
        private List<IScheduleMatrixPro> _matrixList;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
        private List<IList<IScheduleMatrixPro>> _groupMatrixList;
	    private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
			_shiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
            _matrix = _mock.StrictMock<IScheduleMatrixPro>();

			_person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), DateOnly.MinValue);
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_target = new TeamScheduling(new AssignScheduledLayers());
			_group = new Group(new List<IPerson>{ _person }, "hej");
			_scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_mainShift = new EditableShift(new ShiftCategory("hej"));

			_matrixList = new List<IScheduleMatrixPro> { _matrix };
			_groupMatrixList = new List<IList<IScheduleMatrixPro>>();
			_groupMatrixList.Add(_matrixList);
			_teaminfo = new TeamInfo(_group, _groupMatrixList);
			_blockInfo = new BlockInfo(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
			_teamBlockInfo = new TeamBlockInfo(_teaminfo, _blockInfo);
			_dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(DateOnly.MinValue, _person.PermissionInformation.DefaultTimeZone());
        }
		
        [Test]
        public void ShouldSchedulePerDayAndPerson()
        {
	        var rules = NewBusinessRuleCollection.Minimum();

						using (_mock.Record())
            {
				Expect.Call(_matrix.Person).Return(_person);
	            Expect.Call(_matrix.SchedulePeriod).Return(_person.VirtualSchedulePeriod(DateOnly.MinValue));
	            Expect.Call(_matrix.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
	            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
	            Expect.Call(_matrix.UnlockedDays).Return(new [] {_scheduleDayPro});
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call( () => _shiftProjectionCache.SetDate(_dateOnlyAsDateTimePeriod));
	            Expect.Call(_shiftProjectionCache.TheMainShift).Return(_mainShift);
	            Expect.Call(() => _scheduleDay.AddMainShift(_mainShift));
	            Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDay, rules));
	            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
	            Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(new DateTimePeriod());
	            _resourceCalculateDelayer.CalculateIfNeeded(DateOnly.MinValue, new DateTimePeriod(), false);

	            Expect.Call(_scheduleDay.PersonAssignment()).Return(null);
	            Expect.Call(_scheduleDay.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()));
            }

            using (_mock.Playback())
            {
	            _target.ExecutePerDayPerPerson(_person, DateOnly.MinValue, _teamBlockInfo, _shiftProjectionCache,
		            _schedulePartModifyAndRollbackService,
		            _resourceCalculateDelayer, false, rules, new SchedulingOptions(), null);
            }

        }

        [Test]
        public void ShouldNotSchedulePerDayIfScheduled()
        {

            using (_mock.Record())
            {
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.SchedulePeriod).Return(_person.VirtualSchedulePeriod(DateOnly.MinValue));
				Expect.Call(_matrix.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_matrix.UnlockedDays).Return(new [] { _scheduleDayPro });
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
			}

            using (_mock.Playback())
            {
	            _target.ExecutePerDayPerPerson(_person, DateOnly.MinValue, _teamBlockInfo, _shiftProjectionCache,
		            _schedulePartModifyAndRollbackService,
		            _resourceCalculateDelayer, false, NewBusinessRuleCollection.Minimum(), new SchedulingOptions(), null);
            }

        }

	    [Test]
	    public void ShouldNotScheduleIfMainShiftPeriodNotContainsPersonalShift()
	    {
		    var personAssignment = _mock.StrictMock<IPersonAssignment>();
		    var personalShiftLayer = _mock.StrictMock<IPersonalShiftLayer>();
		    var personalActivities = new List<IPersonalShiftLayer> {personalShiftLayer};

		    using (_mock.Record())
		    {
			    Expect.Call(_matrix.Person).Return(_person);
			    Expect.Call(_matrix.SchedulePeriod).Return(_person.VirtualSchedulePeriod(DateOnly.MinValue));
			    Expect.Call(_matrix.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
			    Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
			    Expect.Call(_matrix.UnlockedDays).Return(new [] {_scheduleDayPro});
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(() => _shiftProjectionCache.SetDate(_dateOnlyAsDateTimePeriod));
			    Expect.Call(_shiftProjectionCache.MainShiftProjection).Return(_mainShift.ProjectionService().CreateProjection());

			    Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
			    Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(new DateTimePeriod());
			    _resourceCalculateDelayer.CalculateIfNeeded(DateOnly.MinValue, new DateTimePeriod(), false);

			    Expect.Call(_scheduleDay.PersonAssignment()).Return(personAssignment).Repeat.AtLeastOnce();
			    Expect.Call(personAssignment.PersonalActivities()).Return(personalActivities).Repeat.Twice();
			    Expect.Call(personalShiftLayer.Period).Return(_dateOnlyAsDateTimePeriod.Period().MovePeriod(TimeSpan.FromHours(1)));
		    }

		    using (_mock.Playback())
		    {
			    _target.ExecutePerDayPerPerson(_person, DateOnly.MinValue, _teamBlockInfo, _shiftProjectionCache, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, false, NewBusinessRuleCollection.Minimum(), new SchedulingOptions(), null);
		    }
	    }

		[Test]
		public void ShouldNotScheduleIfMainShiftPeriodNotContainsPersonalMeeting()
		{
			var personMeeting = _mock.StrictMock<IPersonMeeting>();
			var meetings = new List<IPersonMeeting> {personMeeting};

			using (_mock.Record())
			{
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_matrix.SchedulePeriod).Return(_person.VirtualSchedulePeriod(DateOnly.MinValue));
				Expect.Call(_matrix.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_matrix.UnlockedDays).Return(new [] { _scheduleDayPro });
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.None);
				Expect.Call(() => _shiftProjectionCache.SetDate(_dateOnlyAsDateTimePeriod));
				Expect.Call(_shiftProjectionCache.MainShiftProjection).Return(_mainShift.ProjectionService().CreateProjection());

				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCache.WorkShiftProjectionPeriod).Return(new DateTimePeriod());
				_resourceCalculateDelayer.CalculateIfNeeded(DateOnly.MinValue, new DateTimePeriod(), false);

				Expect.Call(_scheduleDay.PersonAssignment()).Return(null);
				Expect.Call(_scheduleDay.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(meetings));
				Expect.Call(personMeeting.Period).Return(_dateOnlyAsDateTimePeriod.Period().MovePeriod(TimeSpan.FromHours(1)));
			}

			using (_mock.Playback())
			{
				_target.ExecutePerDayPerPerson(_person, DateOnly.MinValue, _teamBlockInfo, _shiftProjectionCache, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, false, NewBusinessRuleCollection.Minimum(), new SchedulingOptions(), null);
			}
		}
    }
}
