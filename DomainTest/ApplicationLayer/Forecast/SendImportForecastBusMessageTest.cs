using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class SendImportForecastBusMessageTest
	{
		private ISplitImportForecastMessage _target;
		private MockRepository _mocks;
		private IForecastsAnalyzeQuery _analyzeQuery;
		private IJobResultFeedback _feedback;
		private IOpenAndSplitTargetSkill _openAndSplitTarget;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_analyzeQuery = _mocks.DynamicMock<IForecastsAnalyzeQuery>();
			_feedback = _mocks.DynamicMock<IJobResultFeedback>();
			_openAndSplitTarget = _mocks.StrictMock<IOpenAndSplitTargetSkill>();
			var person = PersonFactory.CreatePerson(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			_target = new SplitImportForecastMessage(_analyzeQuery, _feedback, _openAndSplitTarget,
				new CurrentIdentity(new FakeCurrentTeleoptiPrincipal(new TeleoptiPrincipalWithUnsafePerson(
					new TeleoptiIdentity(person.Name.ToString(), new DummyDataSource("test"), () => BusinessUnitFactory.BusinessUnitUsedInTest.Id, BusinessUnitFactory.BusinessUnitUsedInTest.Name, null,
						""), person))));
		}

		[Test]
		public void ShouldNotifyBusToOpenAndSplitTargetSkill()
		{
			var dateOnly = new DateOnly(2012, 3, 1);
			var targetSkill = SkillFactory.CreateSkill("Target Skill");
			targetSkill.MidnightBreakOffset = TimeSpan.Zero;
			var queryResult = _mocks.DynamicMock<IForecastsAnalyzeQueryResult>();

			var row = new ForecastsRow
			{
				TaskTime = 179,
				AfterTaskTime = 0,
				Agents = 4.05,
				LocalDateTimeFrom = new DateTime(2012, 3, 1, 12, 45, 0),
				LocalDateTimeTo = new DateTime(2012, 3, 1, 13, 0, 0),
				SkillName = "Insurance",
				Tasks = 17,
				UtcDateTimeFrom = new DateTime(2012, 3, 1, 12, 45, 0, DateTimeKind.Utc),
				UtcDateTimeTo = new DateTime(2012, 3, 1, 13, 0, 0, DateTimeKind.Utc)
			};

			var openHours = new WorkloadDayOpenHoursContainer();
			openHours.AddOpenHour(dateOnly, new TimePeriod(12, 45, 13, 00));
			var forecasts = new ForecastFileContainer();
			forecasts.AddForecastsRow(dateOnly, row);
			using (_mocks.Record())
			{
				Expect.Call(_analyzeQuery.Run(new[] { row }, targetSkill)).Return(queryResult);
				Expect.Call(queryResult.WorkloadDayOpenHours).Return(openHours);
				Expect.Call(queryResult.ForecastFileContainer).Return(forecasts);
				Expect.Call(()=>_openAndSplitTarget.Process(new OpenAndSplitTargetSkillMessage())).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				_target.Process(new[] { row }, targetSkill, new DateOnlyPeriod(dateOnly, dateOnly));
			}
		}
	}
}
