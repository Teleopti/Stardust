//using System.Collections.Generic;
//using NUnit.Framework;
//using Rhino.Mocks;
//using SharpTestsEx;
//using Teleopti.Ccc.Domain.Forecasting;
//using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
//using Teleopti.Ccc.TestCommon.FakeData;
//

//namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.LegacyWrappers
//{
//	public class LoadStatisticsTest
//	{
//		[Test]
//		public void ShouldWrapLoadWorkloadDayCall()
//		{
//			var expected = new List<IWorkloadDayBase>();
//			var wl = new Workload(SkillFactory.CreateSkill("dsf"));
//			var period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
//			var statFactory = new StatisticHelperFactoryForTest(statHelper => statHelper.Stub(x => x.LoadStatisticData(period, wl)).Return(expected));

//			ILoadStatistics target = new LoadStatistics(null);

//			target.LoadWorkloadDay(wl, period)
//				.Should().Be.SameInstanceAs(expected);
//		}

//		[Test]
//		public void MakeSureNotSameUnitOfWorkIsReused()
//		{
//			var statFactory = new StatisticHelperFactoryForTest();
//			ILoadStatistics target = new LoadStatistics(null);

//			target.LoadWorkloadDay(null, new DateOnlyPeriod());
//			var orgHelper = statFactory.StatisticHelper;
//			target.LoadWorkloadDay(null, new DateOnlyPeriod());

//			orgHelper.Should().Not.Be.SameInstanceAs(statFactory.StatisticHelper);
//		}
//	}
//}