using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation.IntraIntervalAnalyze
{
	[TestFixture]
	public class FullIntervalFinderTest
	{
		private FullIntervalFinder _target;
		private DateTimePeriod _interval;
		private IResourceCalculationDataContainer _resourceCalculationDataContainer;
		private ISkill _skill;
		private IList<DateTimePeriod> _intraIntervalPeriods;
		
		[SetUp]
		public void SetUp()
		{
			_target = new FullIntervalFinder();
			var start = new DateTime(2014, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2014, 1, 1, 10, 30, 0, DateTimeKind.Utc);
			_interval = new DateTimePeriod(start,end);
			_resourceCalculationDataContainer = MockRepository.GenerateMock<IResourceCalculationDataContainer>();
			_skill = SkillFactory.CreateSkill("skill");
			var intraIntervalStart1 = new DateTime(2014, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var intraIntervalEnd1 = new DateTime(2014, 1, 1, 10, 15, 0, DateTimeKind.Utc);
			var intraIntervalStart2 = new DateTime(2014, 1, 1, 10, 15, 0, DateTimeKind.Utc);
			var intraIntervalEnd2 = new DateTime(2014, 1, 1, 10, 30, 0, DateTimeKind.Utc);
			var intraIntervalPeriod1 = new DateTimePeriod(intraIntervalStart1, intraIntervalEnd1);
			var intraIntervalPeriod2 = new DateTimePeriod(intraIntervalStart2, intraIntervalEnd2);
			_intraIntervalPeriods = new List<DateTimePeriod>{intraIntervalPeriod1, intraIntervalPeriod2};
		}

		[Test]
		public void ShouldFind()
		{
			_resourceCalculationDataContainer.Stub(x => x.SkillResources(_skill, _interval))
				.Return(new Tuple<double, double>(0d, 10d));

			var result = _target.FindForInterval(_interval, _resourceCalculationDataContainer, _skill, _intraIntervalPeriods);
			Assert.AreEqual(9d, result);
		}

		[Test]
		public void ShouldHandleHourLongIntervals()
		{
			_interval = new DateTimePeriod(_interval.StartDateTime,_interval.StartDateTime.AddHours(1));
			_resourceCalculationDataContainer.Stub(x => x.SkillResources(_skill, _interval))
				.Return(new Tuple<double, double>(0d, 10d));

			var result = _target.FindForInterval(_interval, _resourceCalculationDataContainer, _skill, _intraIntervalPeriods);
			Assert.AreEqual(9.5d, result);
		}

		[Test]
		public void ShouldHandleHourLongFractions()
		{
			_interval = new DateTimePeriod(_interval.StartDateTime, _interval.StartDateTime.AddHours(2));
			_intraIntervalPeriods.Add(new DateTimePeriod(_interval.StartDateTime, _interval.StartDateTime.AddHours(1)));

			_resourceCalculationDataContainer.Stub(x => x.SkillResources(_skill, _interval))
				.Return(new Tuple<double, double>(0d, 10d));

			var result = _target.FindForInterval(_interval, _resourceCalculationDataContainer, _skill, _intraIntervalPeriods);
			Assert.AreEqual(9.25d, result);
		}
	}
}
