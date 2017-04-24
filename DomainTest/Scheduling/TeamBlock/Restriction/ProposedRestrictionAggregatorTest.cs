using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		private ShiftProjectionCache _shift;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IScheduleDictionary _scheduleDictionary;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamRestrictionAggregator = _mocks.StrictMock<ITeamRestrictionAggregator>();
			_blockRestrictionAggregator = _mocks.StrictMock<IBlockRestrictionAggregator>();
			_teamBlockRestrictionAggregator = _mocks.StrictMock<ITeamBlockRestrictionAggregator>();
			_teamBlockSchedulingOptions = _mocks.StrictMock<ITeamBlockSchedulingOptions>();
			_person = PersonFactory.CreatePerson("Bill");
			_shift = _mocks.StrictMock<ShiftProjectionCache>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_target = new ProposedRestrictionAggregator(_teamRestrictionAggregator, _blockRestrictionAggregator,
														_teamBlockRestrictionAggregator, _teamBlockSchedulingOptions,
														_effectiveRestrictionCreator);

			_schedulingOptions = new SchedulingOptions();
			_dateOnly = new DateOnly(2013, 11, 14);
			var person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			var person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("ball"), _dateOnly);
			var group = new Group(new List<IPerson> { person1, person2 }, "Hej");
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro>();
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(group, groupMatrixes);
			var blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly.AddDays(1));
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(blockPeriod));
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
		}

		[Test]
		public void ShouldReturnRestrictionForSinglePersonIfNotTeamOrBlockScheduling()
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
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestrictionForSinglePerson(_person, _dateOnly,
					_schedulingOptions, _scheduleDictionary)).Return(exprectedResult);
			}
			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_scheduleDictionary, _schedulingOptions, _teamBlockInfo, _dateOnly, _person, _shift);
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
				Expect.Call(_teamRestrictionAggregator.Aggregate(_scheduleDictionary, _dateOnly, _teamBlockInfo, _schedulingOptions, _shift))
				      .Return(exprectedResult);
			}
			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_scheduleDictionary, _schedulingOptions, _teamBlockInfo, _dateOnly, _person, _shift);
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
				Expect.Call(_blockRestrictionAggregator.Aggregate(_scheduleDictionary, _person, _teamBlockInfo, _schedulingOptions, _shift, _dateOnly))
				      .Return(exprectedResult);
			}
			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_scheduleDictionary, _schedulingOptions, _teamBlockInfo, _dateOnly, _person, _shift);
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
				Expect.Call(_teamBlockRestrictionAggregator.Aggregate(_scheduleDictionary, _dateOnly, _person, _teamBlockInfo, _schedulingOptions, _shift))
				      .Return(exprectedResult);
			}
			using (_mocks.Playback())
			{
				var result = _target.Aggregate(_scheduleDictionary, _schedulingOptions, _teamBlockInfo, _dateOnly, _person, _shift);
				Assert.That(result, Is.EqualTo(exprectedResult));
			}
		}

	}
}
