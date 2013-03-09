using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class RestrictionAggregatorTest
    {
        private MockRepository _mocks;
        private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
        private ISchedulingOptions _schedulingOptions;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IRestrictionAggregator _target;
	    private IOpenHoursToEffectiveRestrictionConverter _openHoursToRestrictionConverter;
	    private IScheduleRestrictionExtractor _scheduleRestrictionExtractor;
	    private TeamBlockInfo _teamBlockInfo;
	    private IPerson _person1;
	    private DateOnly _dateOnly;
	    private IScheduleMatrixPro _scheduleMatrixPro;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
            _schedulingOptions = new SchedulingOptions();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_openHoursToRestrictionConverter = _mocks.StrictMock<IOpenHoursToEffectiveRestrictionConverter>();
			_scheduleRestrictionExtractor = _mocks.StrictMock<IScheduleRestrictionExtractor>();
			_target = new RestrictionAggregator(_effectiveRestrictionCreator,
                                                _schedulingResultStateHolder,
												_openHoursToRestrictionConverter,
												_scheduleRestrictionExtractor);
			_person1 = PersonFactory.CreatePerson("bill");
			_dateOnly = new DateOnly(2012, 12, 7);
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			IGroupPerson groupPerson = new GroupPerson(new List<IPerson> { _person1 }, _dateOnly, "Hej", Guid.NewGuid());
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1))));
        }

	    [Test]
	    public void ShouldAggregateRestrictions()
	    {
		    
		    var dateList = new List<DateOnly> {_dateOnly, _dateOnly.AddDays(1)};
		    
		    var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			IActivity activity = new Activity("bo");
			var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			IMainShift mainShift = MainShiftFactory.CreateMainShift(activity, period, new ShiftCategory("cat"));
			
		    var firstDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
			                             new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
		    var secondDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(13)),
			                             new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(18)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
		    var openHoursRestriction =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11),null),
			                             new EndTimeLimitation(null, TimeSpan.FromHours(17.5)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
		    var scheduleRestriction =
			    new EffectiveRestriction(new StartTimeLimitation(),
			                             new EndTimeLimitation(),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
				    {
					    CommonMainShift = mainShift
				    };
		    using (_mocks.Record())
		    {
			    Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
			    Expect.Call(
				    _effectiveRestrictionCreator.GetEffectiveRestriction(
					    new ReadOnlyCollection<IPerson>(new List<IPerson> {_person1}), _dateOnly,
					    _schedulingOptions, scheduleDictionary)).IgnoreArguments()
			          .Return(firstDay);
			    Expect.Call(
				    _effectiveRestrictionCreator.GetEffectiveRestriction(
					    new ReadOnlyCollection<IPerson>(new List<IPerson> {_person1}), _dateOnly.AddDays(1),
					    _schedulingOptions, scheduleDictionary)).IgnoreArguments()
			          .Return(secondDay);
			    Expect.Call(_openHoursToRestrictionConverter.Convert(_teamBlockInfo.TeamInfo.GroupPerson, _teamBlockInfo.BlockInfo.BlockPeriod.DayCollection()))
			          .Return(openHoursRestriction);
			    Expect.Call(_scheduleRestrictionExtractor.Extract(dateList, matrixList, _schedulingOptions))
			          .Return(scheduleRestriction);
		    }

		    using (_mocks.Playback())
		    {
			    var result = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11), TimeSpan.FromHours(12)),
			                                          new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(17.5)),
			                                          new WorkTimeLimitation(), null, null, null,
			                                          new List<IActivityRestriction>()){CommonMainShift = mainShift};

				var restriction = _target.Aggregate(_teamBlockInfo, _schedulingOptions);
			    Assert.That(restriction, Is.EqualTo(result));
		    }
	    }

	    [Test]
        public void ShouldReturnNullWhenTeamBlockInfoIsNull()
        {
            Assert.That(_target.Aggregate(null, _schedulingOptions), Is.Null);
        }

    }
}
