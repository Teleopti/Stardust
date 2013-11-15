using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.Restriction
{
	[TestFixture]
	public class ProposedRestrictionAggregatorTest
	{
		private MockRepository _mocks;
		private ITeamRestrictionAggregator _teamRestrictionAggregator;
		private IBlockRestrictionAggregator _blockRestrictionAggregator;
		private ProposedRestrictionAggregator _target;
		private ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private ISchedulingOptions _schedulingOptions;
		private ITeamBlockInfo _teamBlockInfo;
		private DateOnly _dateOnly;
		private IPerson _person;
		private IShiftProjectionCache _shift;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamRestrictionAggregator = _mocks.StrictMock<ITeamRestrictionAggregator>();
			_blockRestrictionAggregator = _mocks.StrictMock<IBlockRestrictionAggregator>();
			_teamBlockRestrictionAggregator = _mocks.StrictMock<ITeamBlockRestrictionAggregator>();
			_teamBlockSchedulingOptions = _mocks.StrictMock<ITeamBlockSchedulingOptions>();
			_target = new ProposedRestrictionAggregator(_teamRestrictionAggregator, _blockRestrictionAggregator,
														_teamBlockRestrictionAggregator, _teamBlockSchedulingOptions);

			_schedulingOptions = new SchedulingOptions();
			_dateOnly = new DateOnly(2013, 11, 14);
			var person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			var person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("ball"), _dateOnly);
			var groupPerson = new GroupPerson(new List<IPerson> { person1, person2 }, _dateOnly, "Hej", Guid.Empty);
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro>();
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(groupPerson, groupMatrixes);
			var blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1));
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(blockPeriod));
		}

		[Test]
		public void ShouldReturnEmptyIfNotTeamOrBlockScheduling()
		{
			var exprectedResult = new EffectiveRestriction(new StartTimeLimitation(),
														   new EndTimeLimitation(),
														   new WorkTimeLimitation(), null, null, null,
														   new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsTeamScheduling(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockScheduling(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamBlockScheduling(_schedulingOptions)).Return(false);
			}
			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person, _shift);
				Assert.That(result, Is.EqualTo(exprectedResult));
			}
		}

		[Test]
		public void ShouldAggregateForTeam()
		{
			var exprectedResult = new EffectiveRestriction(new StartTimeLimitation(),
														   new EndTimeLimitation(),
														   new WorkTimeLimitation(), null, null, null,
														   new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsTeamScheduling(_schedulingOptions)).Return(true);
				Expect.Call(_teamRestrictionAggregator.Aggregate(_dateOnly, _teamBlockInfo, _schedulingOptions, _shift))
				      .Return(exprectedResult);
			}
			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person, _shift);
				Assert.That(result, Is.EqualTo(exprectedResult));
			}
		}

		[Test]
		public void ShouldAggregateForBlock()
		{
			var exprectedResult = new EffectiveRestriction(new StartTimeLimitation(),
														   new EndTimeLimitation(),
														   new WorkTimeLimitation(), null, null, null,
														   new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsTeamScheduling(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockScheduling(_schedulingOptions)).Return(true);
				Expect.Call(_blockRestrictionAggregator.Aggregate(_person, _teamBlockInfo, _schedulingOptions, _shift))
				      .Return(exprectedResult);
			}
			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person, _shift);
				Assert.That(result, Is.EqualTo(exprectedResult));
			}
		}

		[Test]
		public void ShouldAggregateForTeamBlock()
		{
			var exprectedResult = new EffectiveRestriction(new StartTimeLimitation(),
														   new EndTimeLimitation(),
														   new WorkTimeLimitation(), null, null, null,
														   new List<IActivityRestriction>());
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsTeamScheduling(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockScheduling(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsTeamBlockScheduling(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockRestrictionAggregator.Aggregate(_teamBlockInfo, _schedulingOptions, _shift))
				      .Return(exprectedResult);
			}
			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_schedulingOptions, _teamBlockInfo, _dateOnly, _person, _shift);
				Assert.That(result, Is.EqualTo(exprectedResult));
			}
		}

	}
}
