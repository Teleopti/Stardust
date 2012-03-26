using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	//no real test - just check it doesn't crash
	[TestFixture]
	[Category("LongRunning")]
	public class WebReportRepositoryTest : AnalyticsDatabaseTest
	{
		private IWebReportRepository _target;

		protected WebReportInput ReportCommonInput { get; private set; }

		protected override void SetupForAnalyticsDatabaseTest()
		{
			_target = new WebReportRepository(SetupFixtureForAssembly.DataSource);
			ReportCommonInput = new WebReportInput
			                    	{
			                    		ReportId = new Guid("8D8544E4-6B24-4C1C-8083-CBE7522DD0E0"),
			                    		PersonCode = new Guid("EFE16A67-E352-4817-BDA8-9A7200410E30"),
			                    		LanguageId = 1053,
			                    		BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value,
			                    		SkillSet = "-2,-1,1,2,3,4,5",
			                    		TimeZoneId = 1,
			                    		WorkloadSet = "-1,1,2,3,4,5,6,7,8,9",
			                    		DateFrom = new DateTime(2006, 1, 1),
			                    		DateTo = new DateTime(2006, 6, 10),
			                    		IntervalFrom = 0,
			                    		IntervalTo = 95,
			                    		IntervalType = 5,
			                    	};
		}

		protected class WebReportInput
		{
			public int ScenarioId { get; set; }
			public int IntervalType { get; set; }
			public DateTime DateFrom { get; set; }
			public DateTime DateTo { get; set; }
			public int IntervalFrom { get; set; }
			public int IntervalTo { get; set; }
			public Guid PersonCode { get; set; }
			public int LanguageId { get; set; }
			public Guid BusinessUnitCode { get; set; }
			public Guid ReportId { get; set; }
			public string SkillSet { get; set; }
			public string WorkloadSet { get; set; }
			public int TimeZoneId { get; set; }
			public int ServiceLevelCalculationId { get; set; }
		}

		protected class WebReportInit
		{
			public Guid PersonCode { get; set; }
			public int LanguageId { get; set; }
			public Guid BusinessUnitCode { get; set; }
			public string SkillSet { get; set; }
			public string TimeZoneCode { get; set; }
			
		}

		[Test]
		public void VerifyGetMobileReportInit()
		{
			var init = new WebReportInit
			           	{
			           		PersonCode = new Guid("EFE16A67-E352-4817-BDA8-9A7200410E30"),
			           		LanguageId = 1053,
							BusinessUnitCode = BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value,
			           		SkillSet = "-2",
			           		TimeZoneCode = "UTC"
			           	};

			_target.ReportMobileReportInit(init.PersonCode, init.LanguageId, init.BusinessUnitCode, init.SkillSet,
			                              init.TimeZoneCode)
				.Should().Not.Be.Null();
		}

		[Test]
		public void VerifyGetReportDataForecastVersusActualWorkload()
		{
			_target.ReportDataForecastVersusActualWorkload(ReportCommonInput.ScenarioId, ReportCommonInput.SkillSet,
													  ReportCommonInput.WorkloadSet, ReportCommonInput.IntervalType,
													  ReportCommonInput.DateFrom, ReportCommonInput.DateTo,
													  ReportCommonInput.IntervalFrom, ReportCommonInput.IntervalTo,
													  ReportCommonInput.TimeZoneId, ReportCommonInput.PersonCode,
													  ReportCommonInput.ReportId, ReportCommonInput.LanguageId,
													  ReportCommonInput.BusinessUnitCode)
				.Should().Not.Be.Null(); // Unable to figure out params .Empty();
		}


		[Test]
		public void VerifyGetReportDataQueueStatAbandoned()
		{
			_target.ReportDataQueueStatAbandoned(ReportCommonInput.ScenarioId, ReportCommonInput.SkillSet,
													  ReportCommonInput.WorkloadSet, ReportCommonInput.IntervalType,
													  ReportCommonInput.DateFrom, ReportCommonInput.DateTo,
													  ReportCommonInput.IntervalFrom, ReportCommonInput.IntervalTo,
													  ReportCommonInput.TimeZoneId, ReportCommonInput.PersonCode,
													  ReportCommonInput.ReportId, ReportCommonInput.LanguageId,
													  ReportCommonInput.BusinessUnitCode)
				.Should().Not.Be.Null(); // Unable to figure out params .Empty();
		}

		[Test]
		public void VerifyGetReportDataServiceLevelAgentsReady()
		{
			_target.ReportDataServiceLevelAgentsReady(ReportCommonInput.SkillSet,
													  ReportCommonInput.WorkloadSet, ReportCommonInput.IntervalType,
													  ReportCommonInput.DateFrom, ReportCommonInput.DateTo,
													  ReportCommonInput.IntervalFrom, ReportCommonInput.IntervalTo, ReportCommonInput.ServiceLevelCalculationId,
													  ReportCommonInput.TimeZoneId, ReportCommonInput.PersonCode,
													  ReportCommonInput.ReportId, ReportCommonInput.LanguageId,
													  ReportCommonInput.BusinessUnitCode)
				.Should().Not.Be.Null(); // Unable to figure out params .Empty();
		}

		[Test]
		public void VerifyGetSkill()
		{
			var dataFactory = new AnalyticsDataFactory();
			dataFactory.Setup(new UtcAndCetTimeZones());
			dataFactory.Setup(new ExistingDatasources(dataFactory.Data<UtcAndCetTimeZones>()));
			dataFactory.Setup(new BusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, dataFactory.Data<ExistingDatasources>()));
			dataFactory.Setup(new ThreeSkills(dataFactory.Data<UtcAndCetTimeZones>(), dataFactory.Data<BusinessUnit>(), dataFactory.Data<ExistingDatasources>()));
			dataFactory.Persist();
			var skillIds = dataFactory.Data<ThreeSkills>().Rows.Select(r => (int)r["skill_id"]).ToArray();

			var result = _target.ReportControlSkillGet(
				ReportCommonInput.ReportId,
				ReportCommonInput.PersonCode,
				ReportCommonInput.LanguageId,
				ReportCommonInput.BusinessUnitCode)
				;

			result.First().Id
				.Should().Be(-2); //all
			result.Select(e => e.Id).Skip(1)
				.Should().Have.SameValuesAs(skillIds);
		}
	}
}
