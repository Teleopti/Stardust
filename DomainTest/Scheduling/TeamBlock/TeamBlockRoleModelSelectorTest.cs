﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockRoleModelSelectorTest
	{
		private ITeamBlockRoleModelSelector _target;
		private MockRepository _mocks;
		private IRestrictionAggregator _restrictionAggregator;
		private ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
		private IWorkShiftFilterService _workShiftFilterService;
		private DateOnly _dateOnly;
		private IGroupPerson _groupPerson;
		private DateOnlyPeriod _selectedPeriod;
		private TeamBlockInfo _teamBlockInfo;
		private IScheduleMatrixPro _matrix1;
		private SchedulingOptions _schedulingOptions;
		private IWorkShiftSelector _workShiftSelector;
		private ISameOpenHoursInTeamBlockSpecification _sameOpenHoursInTeamBlockSpecification;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_restrictionAggregator = _mocks.StrictMock<IRestrictionAggregator>();
			_skillDayPeriodIntervalDataGenerator = _mocks.StrictMock<ISkillDayPeriodIntervalDataGenerator>();
			_workShiftFilterService = _mocks.StrictMock<IWorkShiftFilterService>();
			_sameOpenHoursInTeamBlockSpecification = _mocks.StrictMock<ISameOpenHoursInTeamBlockSpecification>();
			_workShiftSelector = _mocks.StrictMock<IWorkShiftSelector>();

			_dateOnly = new DateOnly(2013, 4, 8);
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { _matrix1 };
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			_groupPerson = _mocks.StrictMock<IGroupPerson>();
			var teaminfo = new TeamInfo(_groupPerson, groupMatrixList);
			_selectedPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			var blockInfo = new BlockInfo(_selectedPeriod);
			_teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			_schedulingOptions = new SchedulingOptions();

			_target = new TeamBlockRoleModelSelector(_restrictionAggregator, _skillDayPeriodIntervalDataGenerator,
													 _workShiftFilterService, _sameOpenHoursInTeamBlockSpecification,
													 _workShiftSelector);
		}

		[Test]
		public void ShouldVerifyParameters()
		{
			var result = _target.Select(null, _dateOnly, new SchedulingOptions());
			Assert.That(result, Is.Null);

			result = _target.Select(_teamBlockInfo, new DateOnly(), null);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void ShouldAggregateRestrictions()
		{
			using (_mocks.Record())
			{
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_teamBlockInfo, new DateOnly(), _schedulingOptions);

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
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
				Expect.Call(_workShiftFilterService.FilterForRoleModel(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
																	   new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(new List<IShiftProjectionCache>());
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_teamBlockInfo, _dateOnly, _schedulingOptions);

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
			var activityData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();

			using (_mocks.Record())
			{
				Expect.Call(_restrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions)).Return(restriction);
				Expect.Call(_sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(_teamBlockInfo)).Return(true);
				Expect.Call(_workShiftFilterService.FilterForRoleModel(_dateOnly, _teamBlockInfo, restriction, _schedulingOptions,
																	   new WorkShiftFinderResult(_groupPerson, _dateOnly), true))
					  .Return(shifts);
				Expect.Call(_groupPerson.Id).Return(Guid.Empty).Repeat.AtLeastOnce();
				Expect.Call(_skillDayPeriodIntervalDataGenerator.GeneratePerDay(_teamBlockInfo)).Return(activityData);
				Expect.Call(_workShiftSelector.SelectShiftProjectionCache(shifts, activityData,
																		  _schedulingOptions.WorkShiftLengthHintOption,
																		  _schedulingOptions.UseMinimumPersons,
																		  _schedulingOptions.UseMaximumPersons))
					  .Return(shiftProjectionCache);
			}
			using (_mocks.Playback())
			{
				var result = _target.Select(_teamBlockInfo, _dateOnly, _schedulingOptions);

				Assert.That(result, Is.EqualTo(shiftProjectionCache));
			}
		}
	}
}
