using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupMoveTimeOptimizationResourceHelperTest
	{
		private GroupMoveTimeOptimizationResourceHelper _target;
		private IResourceOptimizationHelper _resourceOptimizationHelper;
		private MockRepository _mocks;
		private IList<IScheduleDay> _daysToDelete;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod1;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod2;
		private DateOnly _date1;
		private DateOnly _date2;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private IScheduleDictionary _scheduleDictionary;
		private IEnumerable<IScheduleDay> _modifiedSchedules;
		private IScheduleRange _scheduleRange;
		private IPerson _person;
		private DateTimePeriod _dateTimePeriod;

			
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
			_target = new GroupMoveTimeOptimizationResourceHelper(_resourceOptimizationHelper);
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_daysToDelete = new List<IScheduleDay>{_scheduleDay1, _scheduleDay2};
			_date1 = new DateOnly(2012,1,1);
			_date2 = new DateOnly(2012,1,2);
			_dateOnlyAsDateTimePeriod1 = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			_dateOnlyAsDateTimePeriod2 = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_modifiedSchedules = new List<IScheduleDay>{_scheduleDay1};
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
			_person = PersonFactory.CreatePerson("person");
			_dateTimePeriod = new DateTimePeriod(2012, 1, 1, 2012, 1, 2);
		}

		[Test]
		public void ShouldResourceCalculateDeletedDays()
		{
			var toRemove1 = new List<IScheduleDay> {_scheduleDay1};
			var toRemove2 = new List<IScheduleDay> {_scheduleDay2};
			using(_mocks.Record())
			{
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod2).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod1.DateOnly).Return(_date1).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod2.DateOnly).Return(_date2).Repeat.AtLeastOnce();
				Expect.Call(() =>_resourceOptimizationHelper.ResourceCalculateDate(_date1, true, true, toRemove1, new List<IScheduleDay>()));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_date2, true, true, toRemove2, new List<IScheduleDay>()));
			}

			using(_mocks.Playback())
			{
				_target.CalculateDeletedDays(_daysToDelete);
			}
		}

		[Test]
		public void ShouldRollback()
		{
			var toRemove = new List<IScheduleDay> {_scheduleDay2};
			var toRemoveNextDay = new List<IScheduleDay> {_scheduleDay2};

			var toAdd = new List<IScheduleDay> {_scheduleDay1};
			var toAddNextDay = new List<IScheduleDay> {_scheduleDay1};

			using(_mocks.Record())
			{
				Expect.Call(_schedulePartModifyAndRollbackService.ModificationCollection).Return(_modifiedSchedules);
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1).Repeat.AtLeastOnce();
				Expect.Call(_dateOnlyAsDateTimePeriod1.DateOnly).Return(_date1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleDay1.Person).Return(_person);
				Expect.Call(_scheduleRange.ScheduledDay(_date1)).Return(_scheduleDay2);
				Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
				Expect.Call(_scheduleDay1.HasProjection).Return(true);
				Expect.Call(_scheduleDay1.Period).Return(_dateTimePeriod);

				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod1);
				Expect.Call(_scheduleDay2.HasProjection).Return(true);
				Expect.Call(_scheduleDay2.Period).Return(_dateTimePeriod);

				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_date1, true, true, toRemove, toAdd));
				Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(_date1.AddDays(1), true, true, toRemoveNextDay, toAddNextDay));
			}

			using(_mocks.Playback())
			{
				_target.Rollback(_schedulePartModifyAndRollbackService, _scheduleDictionary);
			}
		}
	}
}
