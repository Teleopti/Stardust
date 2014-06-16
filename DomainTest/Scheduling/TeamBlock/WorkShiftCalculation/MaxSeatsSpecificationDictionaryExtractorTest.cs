using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftCalculation
{
	[TestFixture]
	public class MaxSeatsSpecificationDictionaryExtractorTest
	{
		private MaxSeatsSpecificationDictionaryExtractor _target;
		private List<ISkillStaffPeriod> _skillStaffPeriodList;
		private MockRepository _mocks;
		private ISkillStaffPeriod _skillStaffPeriod1;
		private ISkillStaffPeriod _skillStaffPeriod2;
		private ISkillStaff _skillStaff1;
		private IIsMaxSeatsReachedOnSkillStaffPeriodSpecification _isMaxSeatsReachedOnSkillStaffPeriodSpecification;
		private ISkillStaff _skillStaff2;
		private MaxSeatBoostingFactorCalculator _maxSeatBoostingFactorCalculator;

		[SetUp]
		public void SetUp()
		{
			_mocks = new MockRepository();
			_isMaxSeatsReachedOnSkillStaffPeriodSpecification = new IsMaxSeatsReachedOnSkillStaffPeriodSpecification();
			_maxSeatBoostingFactorCalculator = new MaxSeatBoostingFactorCalculator();
			_target = new MaxSeatsSpecificationDictionaryExtractor(_isMaxSeatsReachedOnSkillStaffPeriodSpecification,_maxSeatBoostingFactorCalculator );
			_skillStaffPeriod1 = _mocks.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod2 = _mocks.StrictMock<ISkillStaffPeriod>();
			_skillStaff1 = _mocks.StrictMock<ISkillStaff>();
			_skillStaff2 = _mocks.StrictMock<ISkillStaff>();
			_skillStaffPeriodList = new List<ISkillStaffPeriod>() { _skillStaffPeriod1, _skillStaffPeriod2 };

		}

		[Test]
		public void ShouldReturnNullIfGivenStaffPeripdIsNull()
		{
			Assert.IsNull(_target.ExtractMaxSeatsFlag(null, TimeZoneInfo.Utc,false));
		}

		[Test]
		public void ShouldReturnSameNumberOfMaxSeatsFlagsAsGivenInterval()
		{
			var startDateTime = new DateTime(2014, 05, 26, 16, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2014, 05, 26, 16, 15, 0, DateTimeKind.Utc);
			DateTimePeriod datetime = new DateTimePeriod(startDateTime, endDateTime);

			DateTimePeriod datetime2 = new DateTimePeriod(startDateTime.AddMinutes(30), endDateTime.AddMinutes(30));
			using (_mocks.Record())
			{

				Expect.Call(_skillStaffPeriod1.Period).Return(datetime);

				Expect.Call(_skillStaffPeriod1.Payload).Return(_skillStaff1).Repeat.Times(2);
				Expect.Call(_skillStaff1.CalculatedUsedSeats).Return(25);
				Expect.Call(_skillStaff1.MaxSeats).Return(25);

				Expect.Call(_skillStaffPeriod2.Period).Return(datetime2);

				Expect.Call(_skillStaffPeriod2.Payload).Return(_skillStaff2).Repeat.Times(2);
				Expect.Call(_skillStaff2.CalculatedUsedSeats).Return(16);
				Expect.Call(_skillStaff2.MaxSeats).Return(25);

			}
			using (_mocks.Playback())
			{
				Assert.AreEqual(2, _target.ExtractMaxSeatsFlag(_skillStaffPeriodList, TimeZoneInfo.Utc,false).Count());
			}
		}

		[Test] 
		public void ShouldReturnTrueFlagOnThoseEqualMaxSeatsInterval()
		{
			var startDateTime = new DateTime(2014, 05, 26, 16, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2014, 05, 26, 16, 15, 0, DateTimeKind.Utc);
			DateTimePeriod datetime = new DateTimePeriod(startDateTime, endDateTime);

			DateTimePeriod datetime2 = new DateTimePeriod(startDateTime.AddMinutes(30), endDateTime.AddMinutes(30));
			using (_mocks.Record())
			{

				Expect.Call(_skillStaffPeriod1.Period).Return(datetime);

				Expect.Call(_skillStaffPeriod1.Payload).Return(_skillStaff1).Repeat.Times(2);
				Expect.Call(_skillStaff1.CalculatedUsedSeats).Return(25);
				Expect.Call(_skillStaff1.MaxSeats).Return(25);

				Expect.Call(_skillStaffPeriod2.Period).Return(datetime2);

				Expect.Call(_skillStaffPeriod2.Payload).Return(_skillStaff2).Repeat.Times(2);
				Expect.Call(_skillStaff2.CalculatedUsedSeats).Return(16);
				Expect.Call(_skillStaff2.MaxSeats).Return(25);

			}
			using (_mocks.Playback())
			{
				var result = _target.ExtractMaxSeatsFlag(_skillStaffPeriodList, TimeZoneInfo.Utc,true);
				Assert.IsTrue(result[startDateTime].IsMaxSeatReached );
				Assert.IsFalse(result[startDateTime.AddMinutes(30)].IsMaxSeatReached );
			}
		}

		[Test] 
		public void ShouldReturnFalseFlagOnThoseNotReachMaxSeatsInterval()
		{
			var startDateTime = new DateTime(2014, 05, 26, 16, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2014, 05, 26, 16, 15, 0, DateTimeKind.Utc);
			DateTimePeriod datetime = new DateTimePeriod(startDateTime, endDateTime);

			DateTimePeriod datetime2 = new DateTimePeriod(startDateTime.AddMinutes(30), endDateTime.AddMinutes(30));
			using (_mocks.Record())
			{

				Expect.Call(_skillStaffPeriod1.Period).Return(datetime);

				Expect.Call(_skillStaffPeriod1.Payload).Return(_skillStaff1).Repeat.Times(2);
				Expect.Call(_skillStaff1.CalculatedUsedSeats).Return(10);
				Expect.Call(_skillStaff1.MaxSeats).Return(25);

				Expect.Call(_skillStaffPeriod2.Period).Return(datetime2);

				Expect.Call(_skillStaffPeriod2.Payload).Return(_skillStaff2).Repeat.Times(2);
				Expect.Call(_skillStaff2.CalculatedUsedSeats).Return(15);
				Expect.Call(_skillStaff2.MaxSeats).Return(25);

			}
			using (_mocks.Playback())
			{
				var result = _target.ExtractMaxSeatsFlag(_skillStaffPeriodList, TimeZoneInfo.Utc,false);
				Assert.IsFalse(result[startDateTime].IsMaxSeatReached );
				Assert.IsFalse(result[startDateTime.AddMinutes(30)].IsMaxSeatReached );
			}
		}

		[Test] 
		public void ShouldConvertSkillStaffIntervalToLocalTime()
		{
			var startDateTime = new DateTime(2014, 02, 26, 16, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2014, 02, 26, 16, 15, 0, DateTimeKind.Utc);
			DateTimePeriod datetime = new DateTimePeriod(startDateTime, endDateTime);

			DateTimePeriod datetime2 = new DateTimePeriod(startDateTime.AddMinutes(30), endDateTime.AddMinutes(30));
			using (_mocks.Record())
			{

				Expect.Call(_skillStaffPeriod1.Period).Return(datetime);

				Expect.Call(_skillStaffPeriod1.Payload).Return(_skillStaff1).Repeat.Times(2);
				Expect.Call(_skillStaff1.CalculatedUsedSeats).Return(10);
				Expect.Call(_skillStaff1.MaxSeats).Return(25);

				Expect.Call(_skillStaffPeriod2.Period).Return(datetime2);

				Expect.Call(_skillStaffPeriod2.Payload).Return(_skillStaff2).Repeat.Times(2);
				Expect.Call(_skillStaff2.CalculatedUsedSeats).Return(15);
				Expect.Call(_skillStaff2.MaxSeats).Return(25);

			}
			using (_mocks.Playback())
			{
				TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
				var result = _target.ExtractMaxSeatsFlag(_skillStaffPeriodList, tzi,false);
				Assert.AreEqual(startDateTime.AddHours(1), result.Keys.First());

			}
		}

		[Test]
		public void ShouldReturnCorrectBoostingFactor()
		{
			var startDateTime = new DateTime(2014, 05, 26, 16, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2014, 05, 26, 16, 15, 0, DateTimeKind.Utc);
			DateTimePeriod datetime = new DateTimePeriod(startDateTime, endDateTime);

			DateTimePeriod datetime2 = new DateTimePeriod(startDateTime.AddMinutes(30), endDateTime.AddMinutes(30));
			using (_mocks.Record())
			{

				Expect.Call(_skillStaffPeriod1.Period).Return(datetime);

				Expect.Call(_skillStaffPeriod1.Payload).Return(_skillStaff1).Repeat.Times(2);
				Expect.Call(_skillStaff1.CalculatedUsedSeats).Return(10);
				Expect.Call(_skillStaff1.MaxSeats).Return(25);

				Expect.Call(_skillStaffPeriod2.Period).Return(datetime2);

				Expect.Call(_skillStaffPeriod2.Payload).Return(_skillStaff2).Repeat.Times(2);
				Expect.Call(_skillStaff2.CalculatedUsedSeats).Return(26);
				Expect.Call(_skillStaff2.MaxSeats).Return(25);

			}
			using (_mocks.Playback())
			{
				var result = _target.ExtractMaxSeatsFlag(_skillStaffPeriodList, TimeZoneInfo.Utc,false);
				Assert.IsFalse(result[startDateTime].IsMaxSeatReached);
				Assert.AreEqual(1,result[startDateTime].MaxSeatBoostingFactor );
				Assert.IsTrue( result[startDateTime.AddMinutes(30)].IsMaxSeatReached);
				Assert.AreEqual(1, result[startDateTime.AddMinutes(30)].MaxSeatBoostingFactor);
			}
		}

	}
}
