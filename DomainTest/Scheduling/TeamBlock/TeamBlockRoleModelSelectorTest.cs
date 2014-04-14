using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockRoleModelSelectorTest
	{
		private ITeamBlockRoleModelSelector _target;
		private MockRepository _mocks;
		private ITeamBlockRestrictionAggregator _restrictionAggregator;
		private IWorkShiftFilterService _workShiftFilterService;
		private DateOnly _dateOnly;
		private ITeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private IWorkShiftSelector _workShiftSelector;
		private ISameOpenHoursInTeamBlockSpecification _sameOpenHoursInTeamBlockSpecification;
		private IPerson _person;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private List<IPerson> _groupMembers;
		private ITeamInfo _teaminfo;
		private IActivityIntervalDataCreator _activityIntervalDataCreator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_restrictionAggregator = _mocks.StrictMock<ITeamBlockRestrictionAggregator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_sameOpenHoursInTeamBlockSpecification = _mocks.StrictMock<ISameOpenHoursInTeamBlockSpecification>();
			_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();
			_dateOnly = new DateOnly(2013, 4, 8);
			_person = PersonFactory.CreatePerson("bill");
			_groupMembers = new List<IPerson> {_person};
			_teaminfo = _mocks.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_schedulingOptions = new SchedulingOptions();
			_activityIntervalDataCreator = _mocks.StrictMock<IActivityIntervalDataCreator>();

			_target = new TeamBlockRoleModelSelector(_restrictionAggregator, _workShiftFilterService, _sameOpenHoursInTeamBlockSpecification,
													 _workShiftSelector,
													 _schedulingResultStateHolder,
													 _activityIntervalDataCreator);
		}

		[Test]
		public void ShouldVerifyParameters()
		{
			var result = _target.Select(null, _dateOnly,_person,  new SchedulingOptions());
			Assert.That(result, Is.Null);

			result = _target.Select(_teamBlockInfo, new DateOnly(), _person, null);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void ShouldAggregateRestrictions()
		{
			using (_mocks.Record())
			{
				Expect.Call(_restrictionAggregator.Aggregate(_dateOnly, _person,_teamBlockInfo, _schedulingOptions)).Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_teamBlockInfo, _dateOnly, _person, _schedulingOptions);

				Assert.That(result, Is.Null);
			}
		}

		[Test]
		public void ShouldFilterCandicateShiftsForRoleModel()
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
										 new EndTimeLimitation(),
										 new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teaminfo);
				Expect.Call(_teaminfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_restrictionAggregator.Aggregate(_dateOnly, _person, _teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
				Expect.Call(_workShiftFilterService.FilterForRoleModel(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
																	   new WorkShiftFinderResult(_person, _dateOnly), true))
					  .Return(new List<IShiftProjectionCache>());
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_teamBlockInfo, _dateOnly, _person, _schedulingOptions);

				Assert.That(result, Is.Null);
			}
		}

		[Test]
		public void ShouldSelectBestShiftAsRoleModel()
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
			var shiftProjectionCache = _mocks.StrictMock<IShiftProjectionCache>();
			var shifts = new List<IShiftProjectionCache> { shiftProjectionCache };
			var activityData = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teaminfo);
				Expect.Call(_teaminfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_restrictionAggregator.Aggregate(_dateOnly, _person, _teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
				Expect.Call(_workShiftFilterService.FilterForRoleModel(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
																	   new WorkShiftFinderResult(_person, _dateOnly), true))
					  .Return(shifts);
				Expect.Call(_activityIntervalDataCreator.CreateFor(_teamBlockInfo,_dateOnly, _schedulingResultStateHolder, true))
					  .Return(activityData);
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons, TimeZoneGuard.Instance.TimeZone))
					  .Return(shiftProjectionCache);
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_teamBlockInfo, _dateOnly, _person, _schedulingOptions);

				Assert.That(result, Is.EqualTo(shiftProjectionCache));
			}
		}
	}
}
