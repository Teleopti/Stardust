using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;



namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class TeamBlockShiftCategoryLimitationValidatorTest
	{
		private MockRepository _mocks;
		private ITeamBlockShiftCategoryLimitationValidator _target;
		private IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;
		private IOptimizationPreferences _optimizationPreferences;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private IBlockInfo _blockInfo1;
		private IBlockInfo _blockInfo2;
		private ITeamInfo _teamInfo1;
		private ITeamInfo _teamInfo2;
		private IPerson _person1;
		private IPerson _person2;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private IShiftCategoryLimitation _weekLimitation;
		private IShiftCategoryLimitation _periodLimitation;
		private IList<DateOnly> _outList;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_shiftCategoryLimitationChecker = _mocks.StrictMock<IShiftCategoryLimitationChecker>();
			_target = new TeamBlockShiftCategoryLimitationValidator(_shiftCategoryLimitationChecker);
			_optimizationPreferences = new OptimizationPreferences();
			_optimizationPreferences.General.UseShiftCategoryLimitations = true;
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mocks.StrictMock<ITeamBlockInfo>();
			_blockInfo1 = _mocks.StrictMock<IBlockInfo>();
			_blockInfo2 = _mocks.StrictMock<IBlockInfo>();
			_teamInfo1 = _mocks.StrictMock<ITeamInfo>();
			_teamInfo2 = _mocks.StrictMock<ITeamInfo>();
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_range1 = _mocks.StrictMock<IScheduleRange>();
			_range2 = _mocks.StrictMock<IScheduleRange>();
			_outList = new List<DateOnly>();

			_weekLimitation = new ShiftCategoryLimitation(new ShiftCategory("late"))
				{
					Weekly = true,
					MaxNumberOf = 1
				};
			
			_periodLimitation = new ShiftCategoryLimitation(new ShiftCategory("early"))
				{
					Weekly = false,
					MaxNumberOf = 1
				};
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), DateOnly.MinValue);
			_person1.SchedulePeriod(DateOnly.MinValue).AddShiftCategoryLimitation(_weekLimitation);
			_person1.SchedulePeriod(DateOnly.MinValue).AddShiftCategoryLimitation(_periodLimitation);

			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), DateOnly.MinValue);
			_person2.SchedulePeriod(DateOnly.MinValue).AddShiftCategoryLimitation(_weekLimitation);
			_person2.SchedulePeriod(DateOnly.MinValue).AddShiftCategoryLimitation(_periodLimitation);
		}

		[Test]
		public void ShouldReturnTrueIfEverythingPasses()
		{
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverWeekLimit(_weekLimitation, _range1,
				                                                                         new DateOnlyPeriod(DateOnly.MinValue,
				                                                                                            DateOnly.MinValue
				                                                                                                    .AddDays(6)),
				                                                                         out _outList)).Return(false);
				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverPeriodLimit(_periodLimitation,
				                                                                           new DateOnlyPeriod(DateOnly.MinValue,
				                                                                                              new DateOnly(1900,5,6)),
				                                                                           _range1, out _outList)).Return(false);

				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverWeekLimit(_weekLimitation, _range2,
																						 new DateOnlyPeriod(DateOnly.MinValue,
																											DateOnly.MinValue
																													.AddDays(6)),
																						 out _outList)).Return(false);
				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverPeriodLimit(_periodLimitation,
																						   new DateOnlyPeriod(DateOnly.MinValue,
																											  new DateOnly(1900, 5, 6)),
																						   _range2, out _outList)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnTrueIfNotUsingLimitations()
		{
			_optimizationPreferences.General.UseShiftCategoryLimitations = false;
			var result = _target.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldReturnTrueIfNoOptimizationPreferences()
		{
			var result = _target.Validate(_teamBlockInfo1, _teamBlockInfo2, null);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldReturnFalseIfTeamBlock1Fails()
		{
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverWeekLimit(_weekLimitation, _range1,
				                                                                         new DateOnlyPeriod(DateOnly.MinValue,
				                                                                                            DateOnly.MinValue
				                                                                                                    .AddDays(6)),
				                                                                         out _outList)).Return(true);

				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverWeekLimit(_weekLimitation, _range2,
																						 new DateOnlyPeriod(DateOnly.MinValue,
																											DateOnly.MinValue
																													.AddDays(6)),
																						 out _outList)).Return(false);
				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverPeriodLimit(_periodLimitation,
																						   new DateOnlyPeriod(DateOnly.MinValue,
																											  new DateOnly(1900, 5, 6)),
																						   _range2, out _outList)).Return(false);
			}
			

			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfTeamBlock2Fails()
		{
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverWeekLimit(_weekLimitation, _range1,
																						 new DateOnlyPeriod(DateOnly.MinValue,
																											DateOnly.MinValue
																													.AddDays(6)),
																						 out _outList)).Return(false);
				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverPeriodLimit(_periodLimitation,
																						   new DateOnlyPeriod(DateOnly.MinValue,
																											  new DateOnly(1900, 5, 6)),
																						   _range1, out _outList)).Return(false);

	
				Expect.Call(_shiftCategoryLimitationChecker.IsShiftCategoryOverWeekLimit(_weekLimitation, _range2,
																						 new DateOnlyPeriod(DateOnly.MinValue,
																											DateOnly.MinValue
																													.AddDays(6)),
																						 out _outList)).Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo1, _teamBlockInfo2, _optimizationPreferences);
				Assert.IsFalse(result);
			}
		}

		private void commonMocks()
		{
			Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
			Expect.Call(_blockInfo1.BlockPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
			Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
			Expect.Call(_teamInfo1.GroupMembers).Return(new[] { _person1 });
			Expect.Call(_teamInfo1.MatrixForMemberAndDate(_person1, DateOnly.MinValue)).Return(_matrix1);
			Expect.Call(_matrix1.ActiveScheduleRange).Return(_range1);

			Expect.Call(_teamBlockInfo2.BlockInfo).Return(_blockInfo2);
			Expect.Call(_blockInfo2.BlockPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
			Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
			Expect.Call(_teamInfo2.GroupMembers).Return(new[] { _person2 });
			Expect.Call(_teamInfo2.MatrixForMemberAndDate(_person2, DateOnly.MinValue)).Return(_matrix2);
			Expect.Call(_matrix2.ActiveScheduleRange).Return(_range2);
		}

	}
}