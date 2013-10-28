using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
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
	    private IScheduleRestrictionExtractor _scheduleRestrictionExtractor;
	    private TeamBlockInfo _teamBlockInfo;
	    private IPerson _person1;
	    private DateOnly _dateOnly;
	    private IScheduleMatrixPro _scheduleMatrixPro;
	    private ISuggestedShiftRestrictionExtractor _suggestedShiftRestrictionExtractor;
	    private TimeZoneInfo _timeZoneInfo;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
            _schedulingOptions = new SchedulingOptions();
		    _schedulingOptions.UseTeamBlockPerOption = true;
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_scheduleRestrictionExtractor = _mocks.StrictMock<IScheduleRestrictionExtractor>();
		    _suggestedShiftRestrictionExtractor = _mocks.StrictMock<ISuggestedShiftRestrictionExtractor>();
			_target = new RestrictionAggregator(_effectiveRestrictionCreator,
                                                _schedulingResultStateHolder,
												_scheduleRestrictionExtractor,
												_suggestedShiftRestrictionExtractor);
		    _person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), _dateOnly);
			_dateOnly = new DateOnly(2012, 12, 7);
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			IGroupPerson groupPerson = new GroupPerson(new List<IPerson> { _person1 }, _dateOnly, "Hej", Guid.NewGuid());
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1))));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
	    public void ShouldAggregateRestrictions()
	    {
		    var dateList = new List<DateOnly> {_dateOnly, _dateOnly.AddDays(1)};
		    
		    var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			IActivity activity = new Activity("bo");
			var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, new ShiftCategory("cat"));
			
		    var firstDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
			                             new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
		    var secondDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(13)),
			                             new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(18)),
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
			    Expect.Call(_scheduleRestrictionExtractor.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo,_teamBlockInfo )).IgnoreArguments()
			          .Return(scheduleRestriction);
		    }

		    using (_mocks.Playback())
		    {
			    var result = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(12)),
			                                          new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(18)),
			                                          new WorkTimeLimitation(), null, null, null,
			                                          new List<IActivityRestriction>()){CommonMainShift = mainShift};

				var restriction = _target.Aggregate(_teamBlockInfo, _schedulingOptions);
				Assert.That(restriction, Is.EqualTo(result));
		    }
	    }

        //should check with morning prefrences

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
	    public void ShouldAggregateRestrictionsPerDayPerPerson()
	    {
		    var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();

		    var matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
		    IActivity activity = new Activity("bo");
		    var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
		                                    new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, new ShiftCategory("cat"));

		    var firstDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
			                             new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

		    var scheduleRestriction =
			    new EffectiveRestriction(new StartTimeLimitation(),
			                             new EndTimeLimitation(),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
				    {
					    CommonMainShift = mainShift
				    };
		    var shiftRestriction =
				new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11.5), null),
			                             new EndTimeLimitation(),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

		    IGroupPerson groupPerson = new GroupPerson(new List<IPerson> {_person1}, _dateOnly, "Hej", Guid.NewGuid());
		    IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> {matrixList};
		    ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
		 	var blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1));
			var blockInfo = new BlockInfo(blockPeriod);
		    var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_schedulingOptions.UseTeamBlockPerOption = true;
		    _schedulingOptions.UseTeamBlockSameStartTime = true;
		    using (_mocks.Record())
		    {
			    Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
			    Expect.Call(
				    _effectiveRestrictionCreator.GetEffectiveRestriction(
					    new ReadOnlyCollection<IPerson>(new List<IPerson> {_person1}), _dateOnly,
					    _schedulingOptions, scheduleDictionary)).IgnoreArguments()
			          .Return(firstDay);
			    Expect.Call(_scheduleRestrictionExtractor.ExtractForOnePersonOneBlock(blockPeriod.DayCollection(), matrixList, _schedulingOptions, _timeZoneInfo)).IgnoreArguments()
			          .Return(scheduleRestriction);
			    Expect.Call(_suggestedShiftRestrictionExtractor.ExtractForOneBlock(shift, _schedulingOptions)).Return(shiftRestriction);
			    Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(blockPeriod);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person1);
		    }

		    using (_mocks.Playback())
		    {
			    var result = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11.5), TimeSpan.FromHours(12)),
			                                          new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
			                                          new WorkTimeLimitation(), null, null, null,
			                                          new List<IActivityRestriction>()) {CommonMainShift = mainShift};

			    var restriction = _target.AggregatePerDayPerPerson(_dateOnly, _person1, new TeamBlockInfo(teamInfo, blockInfo),
			                                                       _schedulingOptions, shift, false);
			    Assert.That(restriction, Is.EqualTo(result));
		    }
	    }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
	    public void ShouldAggregateRestrictionsPerDayPerPersonWithinOneTeam()
	    {
		    var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();

		    var matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
		    IActivity activity = new Activity("bo");
		    var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
		                                    new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, new ShiftCategory("cat"));
			
		    var firstDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
			                             new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

		    var scheduleRestriction =
			    new EffectiveRestriction(new StartTimeLimitation(),
			                             new EndTimeLimitation(),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
				    {
					    CommonMainShift = mainShift
				    };
		    var shiftRestriction =
				new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11.5), null),
			                             new EndTimeLimitation(),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

		    IGroupPerson groupPerson = new GroupPerson(new List<IPerson> {_person1}, _dateOnly, "Hej", Guid.NewGuid());
		    IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> {matrixList};
		    ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
			var blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1));
			var blockInfo = new BlockInfo(blockPeriod);
		    var shift = _mocks.StrictMock<IShiftProjectionCache>();
			var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_schedulingOptions.UseTeamBlockPerOption = true;
		    _schedulingOptions.UseTeamBlockSameStartTime = true;
		    using (_mocks.Record())
		    {
			    Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
			    Expect.Call(
				    _effectiveRestrictionCreator.GetEffectiveRestriction(
					    new ReadOnlyCollection<IPerson>(new List<IPerson> {_person1}), _dateOnly,
					    _schedulingOptions, scheduleDictionary)).IgnoreArguments()
			          .Return(firstDay);
			    Expect.Call(_scheduleRestrictionExtractor.ExtractForOnePersonOneBlock(blockPeriod.DayCollection(), matrixList, _schedulingOptions, _timeZoneInfo)).IgnoreArguments()
			          .Return(scheduleRestriction);
                //Expect.Call(_scheduleRestrictionExtractor.ExtractForOneTeamOneDay(_dateOnly, matrixList, _schedulingOptions, _timeZoneInfo)).IgnoreArguments()
                //      .Return(scheduleRestriction);
			    Expect.Call(_suggestedShiftRestrictionExtractor.ExtractForOneBlock(shift, _schedulingOptions)).Return(shiftRestriction);
			    //Expect.Call(_suggestedShiftRestrictionExtractor.ExtractForOneTeam(shift,_schedulingOptions)).Return(shiftRestriction);
			    Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(blockPeriod);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person1);

		    }

		    using (_mocks.Playback())
		    {
			    var result = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11.5), TimeSpan.FromHours(12)),
			                                          new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
			                                          new WorkTimeLimitation(), null, null, null,
			                                          new List<IActivityRestriction>()) {CommonMainShift = mainShift};

			    var restriction = _target.AggregatePerDayPerPerson(_dateOnly, _person1, new TeamBlockInfo(teamInfo, blockInfo),
			                                                       _schedulingOptions, shift, true);
			    Assert.That(restriction, Is.EqualTo(result));
		    }
	    }

	    [Test]
        public void ShouldReturnNullWhenTeamBlockInfoIsNull()
        {
            Assert.That(_target.Aggregate(null, _schedulingOptions), Is.Null);
        }

		[Test]
		public void ShouldReturnNullIfEffectiveRestrictionIsNullForTeamBlock()
		{
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			IGroupPerson groupPerson = new GroupPerson(new List<IPerson> { _person1 }, _dateOnly, "Hej", Guid.NewGuid());
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1)));
			_schedulingOptions.UseTeamBlockSameStartTime = true;
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(
					_effectiveRestrictionCreator.GetEffectiveRestriction(
						new ReadOnlyCollection<IPerson>(new List<IPerson> { _person1 }), _dateOnly.AddDays(1),
						_schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(null);
			}
			using (_mocks.Playback())
			{
				 var restriction = _target.Aggregate(new TeamBlockInfo(teamInfo, blockInfo), _schedulingOptions);
				Assert.IsNull(restriction);
			}
		}

		[Test]
		public void ShouldReturnNullIfEffectiveRestrictionIsNull()
		{
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			IGroupPerson groupPerson = new GroupPerson(new List<IPerson> { _person1 }, _dateOnly, "Hej", Guid.NewGuid());
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1)));
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
			_schedulingOptions.UseTeamBlockSameStartTime = true;
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(
					_effectiveRestrictionCreator.GetEffectiveRestriction(
						new ReadOnlyCollection<IPerson>(new List<IPerson> { _person1 }), _dateOnly.AddDays(1),
						_schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(null);
			}
			using (_mocks.Playback())
			{
				 var restriction = _target.AggregatePerDayPerPerson(_dateOnly, _person1, new TeamBlockInfo(teamInfo, blockInfo),
			                                                       _schedulingOptions, shift, false);
				Assert.IsNull(restriction);
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnNullIfCombinationOfEffectiveRestrictionsIsNull()
		{
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			IGroupPerson groupPerson = new GroupPerson(new List<IPerson> { _person1 }, _dateOnly, "Hej", Guid.NewGuid());
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1)));
			_schedulingOptions.UseTeamBlockSameStartTime = true;
			var firstDay =
			 new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
									  new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
									  new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(
					_effectiveRestrictionCreator.GetEffectiveRestriction(
						new ReadOnlyCollection<IPerson>(new List<IPerson> { _person1 }), _dateOnly.AddDays(1),
						_schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(firstDay);	
				Expect.Call(
					_effectiveRestrictionCreator.GetEffectiveRestriction(
						new ReadOnlyCollection<IPerson>(new List<IPerson> { _person1 }), _dateOnly.AddDays(1),
						_schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(null);
			}
			using (_mocks.Playback())
			{
				var restriction = _target.Aggregate(new TeamBlockInfo(teamInfo, blockInfo), _schedulingOptions);
				Assert.IsNull(restriction);
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnNullIfScheduleRestrictionIsNull()
		{
			var dateList = new List<DateOnly> { _dateOnly, _dateOnly.AddDays(1) };

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();

			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var firstDay =
				new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
										 new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var secondDay =
				new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(13)),
										 new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(18)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			IGroupPerson groupPerson = new GroupPerson(new List<IPerson> { _person1 }, _dateOnly, "Hej", Guid.NewGuid());
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1)));
			_schedulingOptions.UseTeamBlockSameStartTime = true;
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(
					_effectiveRestrictionCreator.GetEffectiveRestriction(
						new ReadOnlyCollection<IPerson>(new List<IPerson> { _person1 }), _dateOnly,
						_schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(firstDay);
				Expect.Call(
					_effectiveRestrictionCreator.GetEffectiveRestriction(
						new ReadOnlyCollection<IPerson>(new List<IPerson> { _person1 }), _dateOnly.AddDays(1),
						_schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(secondDay);
				Expect.Call(_scheduleRestrictionExtractor.Extract(dateList, matrixList, _schedulingOptions, _timeZoneInfo,_teamBlockInfo )).IgnoreArguments()
					  .Return(null);
			}

			using (_mocks.Playback())
			{
				var restriction = _target.Aggregate(new TeamBlockInfo(teamInfo, blockInfo), _schedulingOptions);
				Assert.IsNull(restriction);
			}
		}
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnNullIfScheduleRestrictionIsNullForTeamBlock()
		{
			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();

			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			var firstDay =
				new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
										 new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

			IGroupPerson groupPerson = new GroupPerson(new List<IPerson> { _person1 }, _dateOnly, "Hej", Guid.NewGuid());
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
	        var blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1));
			var blockInfo = new BlockInfo(blockPeriod);
			var shift = _mocks.StrictMock<IShiftProjectionCache>();
	        var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_schedulingOptions.UseTeamBlockSameStartTime = true;
			using (_mocks.Record())
			{
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
				Expect.Call(
					_effectiveRestrictionCreator.GetEffectiveRestriction(
						new ReadOnlyCollection<IPerson>(new List<IPerson> { _person1 }), _dateOnly,
						_schedulingOptions, scheduleDictionary)).IgnoreArguments()
					  .Return(firstDay);
				Expect.Call(_scheduleRestrictionExtractor.ExtractForOnePersonOneBlock(blockPeriod.DayCollection(), matrixList, _schedulingOptions, _timeZoneInfo)).IgnoreArguments()
					  .Return(null);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(blockPeriod);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person1);
			}

			using (_mocks.Playback())
			{
				var restriction = _target.AggregatePerDayPerPerson(_dateOnly, _person1, new TeamBlockInfo(teamInfo, blockInfo),
				                                                   _schedulingOptions, shift, false);
				Assert.IsNull(restriction);
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
	    public void ShouldReturnNullIfSuggestedShiftRestrictionIsNull()
	    {
 			var dateList = new List<DateOnly> {_dateOnly, _dateOnly.AddDays(1)};

			var scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
		    IActivity activity = new Activity("bo");
		    var period = new DateTimePeriod(new DateTime(2012, 12, 7, 8, 0, 0, DateTimeKind.Utc),
		                                    new DateTime(2012, 12, 7, 8, 30, 0, DateTimeKind.Utc));
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, new ShiftCategory("cat"));

		    var matrixList = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
		    var firstDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(12)),
			                             new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(18)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
		    var secondDay =
			    new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(10), TimeSpan.FromHours(13)),
			                             new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(18)),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());

		    var scheduleRestriction =
			    new EffectiveRestriction(new StartTimeLimitation(),
			                             new EndTimeLimitation(),
			                             new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>())
				    {
					    CommonMainShift = mainShift
				    };
		    IGroupPerson groupPerson = new GroupPerson(new List<IPerson> {_person1}, _dateOnly, "Hej", Guid.NewGuid());
		    IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> {matrixList};
		    ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
		    var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1)));
		    var shift = _mocks.StrictMock<IShiftProjectionCache>();
		    _schedulingOptions.UseTeamBlockSameStartTime = true;
	        var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		    using (_mocks.Record())
		    {
			    Expect.Call(_schedulingResultStateHolder.Schedules).Return(scheduleDictionary);
			    Expect.Call(
				    _effectiveRestrictionCreator.GetEffectiveRestriction(
					    new ReadOnlyCollection<IPerson>(new List<IPerson> {_person1}), _dateOnly.AddDays(1),
					    _schedulingOptions, scheduleDictionary)).IgnoreArguments()
			          .Return(secondDay);

				Expect.Call(_scheduleRestrictionExtractor.ExtractForOnePersonOneBlock(dateList, matrixList, _schedulingOptions, _timeZoneInfo))
			          .IgnoreArguments()
			          .Return(scheduleRestriction);
				Expect.Call(_suggestedShiftRestrictionExtractor.ExtractForOneBlock(shift, _schedulingOptions)).Return(null);
			    Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(schedulePeriod);
			    Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1)));
			    Expect.Call(_scheduleMatrixPro.Person).Return(_person1);
		    }

		    using (_mocks.Playback())
		    {
				var restriction = _target.AggregatePerDayPerPerson(_dateOnly, _person1, new TeamBlockInfo(teamInfo, blockInfo),
																   _schedulingOptions, shift, false);
			    Assert.IsNull(restriction);
		    }
	    }
    }
}
