using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class NightlyRestRestrictionForTeamBlockTest
	{
		private NightlyRestRestrictionForTeamBlock _target;
		private IAssignmentPeriodRule _nightlyRestRule;
		private MockRepository _mock;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private IPerson _person1;
		private IPerson _person2;
		private ITeamBlockInfo _teamBlockInfo;
		private IPermissionInformation _permissionInformation;
		private ITeamInfo _teamInfo;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_nightlyRestRule = _mock.StrictMock<IAssignmentPeriodRule>();
			_target = new NightlyRestRestrictionForTeamBlock(_nightlyRestRule);
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
			_range1 = _mock.StrictMock<IScheduleRange>();
			_range2 = _mock.StrictMock<IScheduleRange>();
			_person1 = _mock.StrictMock<IPerson>();
			_person2 = _mock.StrictMock<IPerson>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_permissionInformation = _mock.StrictMock<IPermissionInformation>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
		}

		[Test]
		public void SingleAgentSingleDayBlock()
		{
			var date = new DateOnly(2013, 09, 27);
			IBlockInfo blockInfo = new BlockInfo(new DateOnlyPeriod(date, date));
			IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1};
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 09, 27, 11, 0, 0, DateTimeKind.Utc),
			                                        new DateTime(2013, 09, 27, 13, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(date)).Return(matrixList);

				expectCallForOneMatrix(date, dateTimePeriod, _matrix1, _range1, _person1);
			}

			var aggRestriction = _target.AggregatedNightlyRestRestriction(_teamBlockInfo);
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11), null),
			                                              new EndTimeLimitation(null, TimeSpan.FromHours(13)),
			                                              new WorkTimeLimitation(), null, null, null,
			                                              new List<IActivityRestriction>());
			Assert.AreEqual(aggRestriction, expectedResult);
		}

		private void expectCallForOneMatrix(DateOnly date, DateTimePeriod dateTimePeriod, IScheduleMatrixPro matrix,
		                                    IScheduleRange range, IPerson person)
		{
			IScheduleDay scheduleDay = _mock.StrictMock<IScheduleDay>();
			Expect.Call(matrix.ActiveScheduleRange).Return(range);
			Expect.Call(range.ScheduledDay(date)).Return(scheduleDay);
			Expect.Call(scheduleDay.IsScheduled()).Return(false);
			Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(range, date)).Return(dateTimePeriod);
			Expect.Call(matrix.Person).Return(person);
			Expect.Call(person.PermissionInformation).Return(_permissionInformation);
			Expect.Call(_permissionInformation.DefaultTimeZone()).Return(TimeZoneInfo.Utc);
		}

		[Test]
		public void MultipleAgentsSingleDayBlock()
		{
			var date = new DateOnly(2013, 09, 27);
			IBlockInfo blockInfo = new BlockInfo(new DateOnlyPeriod(date, date));
			IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1, _matrix2};
			var dateTimePeriod1 = new DateTimePeriod(new DateTime(2013, 09, 27, 10, 0, 0, DateTimeKind.Utc),
			                                         new DateTime(2013, 09, 27, 14, 0, 0, DateTimeKind.Utc));
			var dateTimePeriod2 = new DateTimePeriod(new DateTime(2013, 09, 27, 11, 0, 0, DateTimeKind.Utc),
			                                         new DateTime(2013, 09, 27, 13, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(date)).Return(matrixList);

				expectCallForOneMatrix(date, dateTimePeriod1, _matrix1, _range1, _person1);
				expectCallForOneMatrix(date, dateTimePeriod2, _matrix2, _range2, _person2);
			}

			var aggRestriction = _target.AggregatedNightlyRestRestriction(_teamBlockInfo);
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11), null),
			                                              new EndTimeLimitation(null, TimeSpan.FromHours(13)),
			                                              new WorkTimeLimitation(), null, null, null,
			                                              new List<IActivityRestriction>());
			Assert.AreEqual(aggRestriction, expectedResult);
		}

		[Test]
		public void MultipleAgentsMultipleDaysBlock()
		{
			var date = new DateOnly(2013, 09, 27);
			IBlockInfo blockInfo = new BlockInfo(new DateOnlyPeriod(date, date.AddDays(1)));
			IEnumerable<IScheduleMatrixPro> matrixList1 = new List<IScheduleMatrixPro> {_matrix1};
			IEnumerable<IScheduleMatrixPro> matrixList2 = new List<IScheduleMatrixPro> {_matrix2};
			var dateTimePeriod1 = new DateTimePeriod(new DateTime(2013, 09, 27, 10, 0, 0, DateTimeKind.Utc),
			                                         new DateTime(2013, 09, 27, 13, 0, 0, DateTimeKind.Utc));
			var dateTimePeriod2 = new DateTimePeriod(new DateTime(2013, 09, 28, 11, 0, 0, DateTimeKind.Utc),
			                                         new DateTime(2013, 09, 28, 14, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(date)).Return(matrixList1);

				expectCallForOneMatrix(date, dateTimePeriod1, _matrix1, _range1, _person1);

				Expect.Call(_teamBlockInfo.BlockInfo).Return(blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(date.AddDays(1))).Return(matrixList2);

				expectCallForOneMatrix(date.AddDays(1), dateTimePeriod2, _matrix2, _range2, _person2);
			}

			var aggRestriction = _target.AggregatedNightlyRestRestriction(_teamBlockInfo);
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11), null),
			                                              new EndTimeLimitation(null, TimeSpan.FromHours(13)),
			                                              new WorkTimeLimitation(), null, null, null,
			                                              new List<IActivityRestriction>());
			Assert.AreEqual(aggRestriction, expectedResult);
		}


		[Test]
		public void SingleAgentSingleDayBlockWithScheduledDay()
		{
			var date = new DateOnly(2013, 09, 27);
			IBlockInfo blockInfo = new BlockInfo(new DateOnlyPeriod(date, date));
			IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1};
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 09, 26, 11, 0, 0, DateTimeKind.Utc),
			                                        new DateTime(2013, 09, 28, 13, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(date)).Return(matrixList);

				IScheduleDay scheduleDay = _mock.StrictMock<IScheduleDay>();
				Expect.Call(_matrix1.ActiveScheduleRange).Return(_range1);
				Expect.Call(_range1.ScheduledDay(date)).Return(scheduleDay);
				Expect.Call(scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_nightlyRestRule.LongestDateTimePeriodForAssignment(_range1, date)).Return(dateTimePeriod);
				Expect.Call(_matrix1.Person).Return(_person1);
				Expect.Call(_person1.PermissionInformation).Return(_permissionInformation);
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(TimeZoneInfo.Utc);
			}

			var aggRestriction = _target.AggregatedNightlyRestRestriction(_teamBlockInfo);
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.MinValue, null),
			                                              new EndTimeLimitation(null, new TimeSpan(1, 23, 59, 59)),
			                                              new WorkTimeLimitation(), null, null, null,
			                                              new List<IActivityRestriction>());
			Assert.AreEqual(aggRestriction, expectedResult);
		}

		[Test]
		public void ShouldHandleDateTimePeriodStartingPriorToToday()
		{
			var date = new DateOnly(2013, 09, 27);
			IBlockInfo blockInfo = new BlockInfo(new DateOnlyPeriod(date, date));
			IEnumerable<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> {_matrix1};
			var dateTimePeriod = new DateTimePeriod(new DateTime(2013, 09, 26, 11, 0, 0, DateTimeKind.Utc),
			                                        new DateTime(2013, 09, 28, 13, 0, 0, DateTimeKind.Utc));

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(date)).Return(matrixList);

				expectCallForOneMatrix(date, dateTimePeriod, _matrix1, _range1, _person1);
			}

			var aggRestriction = _target.AggregatedNightlyRestRestriction(_teamBlockInfo);
			var expectedResult = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(11).Add(TimeSpan.FromDays(-1)), null),
														  new EndTimeLimitation(null, TimeSpan.FromHours(13).Add(TimeSpan.FromDays(1))),
														  new WorkTimeLimitation(), null, null, null,
														  new List<IActivityRestriction>());

			Assert.AreEqual(expectedResult.StartTimeLimitation.StartTime.Value, aggRestriction.StartTimeLimitation.StartTime.Value);
			Assert.IsFalse(aggRestriction.StartTimeLimitation.EndTime.HasValue);
			Assert.AreEqual(expectedResult.EndTimeLimitation.EndTime.Value, aggRestriction.EndTimeLimitation.EndTime.Value);
			Assert.IsFalse(aggRestriction.EndTimeLimitation.StartTime.HasValue);
		}
	}
}
