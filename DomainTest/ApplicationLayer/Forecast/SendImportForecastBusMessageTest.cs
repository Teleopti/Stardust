﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	public class SendImportForecastBusMessageTest
	{
		private ISendBusMessage _target;
		private MockRepository _mocks;
		private IForecastsAnalyzeQuery _analyzeQuery;
		private IJobResultFeedback _feedback;
		private IEventPublisher _serviceBus;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_analyzeQuery = _mocks.DynamicMock<IForecastsAnalyzeQuery>();
			_feedback = _mocks.DynamicMock<IJobResultFeedback>();
			_serviceBus = _mocks.StrictMock<IEventPublisher>();
			_target = new SendImportForecastBusMessage(_analyzeQuery, _feedback, _serviceBus);
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
				Expect.Call(() => _serviceBus.Publish()).Constraints(
					 Is.Matching<Object[]>(a => ((OpenAndSplitTargetSkillEvent)a[0]).Date == dateOnly.Date));
			}
			using (_mocks.Playback())
			{
				_target.Process(new[] { row }, targetSkill, new DateOnlyPeriod(dateOnly, dateOnly));
			}
		}
	}
}
