using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class SkillStaffPeriodEvaluatorTest
	{
		private SkillStaffPeriodEvaluator _target;
		private MockRepository _mock;
		private IList<ISkillStaffPeriod> _listBefore;
		private IList<ISkillStaffPeriod> _listAfter;
		private ISkillStaffPeriod _skillStaffPeriod1;
		private ISkillStaffPeriod _skillStaffPeriod2;
		private ISkillStaffPeriod _skillStaffPeriod3;
		private ISkillStaffPeriod _skillStaffPeriod4;
		private DateTimePeriod _period1;
		private DateTimePeriod _period2;
			
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod3 = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriod4 = _mock.StrictMock<ISkillStaffPeriod>();
			_listBefore = new List<ISkillStaffPeriod>{_skillStaffPeriod1, _skillStaffPeriod2};
			_listAfter = new List<ISkillStaffPeriod>{_skillStaffPeriod3, _skillStaffPeriod4};
			_target = new SkillStaffPeriodEvaluator();
			_period1 = new DateTimePeriod(2014, 1, 1, 8, 2014, 1, 1, 10);
			_period2 = new DateTimePeriod(2014, 1, 1, 15, 2014, 1, 1, 16);
		}

		[Test]
		public void ShouldReturnTrueWhenPeriodIsBetter()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriod1.Period).Return(_period1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod2.Period).Return(_period2).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod3.Period).Return(_period1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod4.Period).Return(_period2).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod1.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod2.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod3.IntraIntervalValue).Return(0.9).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod4.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.ResultIsBetter(_listBefore, _listAfter);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenPeriodIsWorse()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriod1.Period).Return(_period1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod2.Period).Return(_period2).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod3.Period).Return(_period1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod4.Period).Return(_period2).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod1.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod2.IntraIntervalValue).Return(0.9).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod3.IntraIntervalValue).Return(0.9).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod4.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.ResultIsBetter(_listBefore, _listAfter);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenPeriodIsEven()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriod1.Period).Return(_period1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod2.Period).Return(_period2).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod3.Period).Return(_period1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod4.Period).Return(_period2).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod1.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod2.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod3.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod4.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.ResultIsBetter(_listBefore, _listAfter);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenMoreNewPeriods()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriod1.Period).Return(_period1).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod3.Period).Return(_period1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod4.Period).Return(_period2).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod1.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod3.IntraIntervalValue).Return(0.9).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_listBefore.RemoveAt(1);
				var result = _target.ResultIsBetter(_listBefore, _listAfter);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnTrueWhenLessBetterPeriods()
		{
			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriod1.Period).Return(_period1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaffPeriod2.Period).Return(_period2).Repeat.AtLeastOnce();

				Expect.Call(_skillStaffPeriod3.Period).Return(_period1).Repeat.AtLeastOnce();
				
				Expect.Call(_skillStaffPeriod1.IntraIntervalValue).Return(0.5).Repeat.AtLeastOnce();
			
				Expect.Call(_skillStaffPeriod3.IntraIntervalValue).Return(0.9).Repeat.AtLeastOnce();

			}

			using (_mock.Playback())
			{
				_listAfter.RemoveAt(1);
				var result = _target.ResultIsBetter(_listBefore, _listAfter);
				Assert.IsTrue(result);
			}
		}
	}
}
