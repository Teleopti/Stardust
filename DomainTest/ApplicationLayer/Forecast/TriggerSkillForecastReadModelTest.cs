using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	[DomainTest]
	[AllTogglesOn]
	[Ignore("WIP")]
	public class TriggerSkillForecastReadModelTest : IIsolateSystem
	{
		public FakeReadModelStartTimeRepository ReadModelStartTimeRepository;
		public MutableNow Now;
		public FakeEventPublisher Publisher;
		public TriggerSkillForecastReadModel Target;
		public SkillForecastReadModelPeriodBuilder SkillForecastReadModelPeriodBuilder;
		public StaffingSettingsReader49Days StaffingSettingsReader;
		public SkillForecastSettingsReader SkillForecastSettingsReader;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<SkillForecastIntervalCalculator>().For<SkillForecastIntervalCalculator>();
			isolate.UseTestDouble<FakeReadModelStartTimeRepository>().For<IReadModelStartTimeRepository>();
			isolate.UseTestDouble<StaffingSettingsReader49Days>().For<IStaffingSettingsReader>();
		}

		[Test]
		public void ShouldPublishEvent()
		{
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			Now.Is(new DateTime(2019,1,30,10,0,0,DateTimeKind.Utc));

			Target.Handle(new TenantDayTickEvent());

			Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(1);
			var @event = Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().FirstOrDefault();
			var period = SkillForecastReadModelPeriodBuilder.BuildFullPeriod();
			@event.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			@event.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			@event.LogOnBusinessUnitId.Should().Be.EqualTo(bu.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldPublishEventForNext8Days()
		{
			Now.Is(new DateTime(2019, 1, 30, 10, 0, 0, DateTimeKind.Utc));
			//ReadModelStartTimeRepository.FakeTime = new DateTime(2019, 1, 23, 10, 0, 0, DateTimeKind.Utc);
			Target.Handle(new TenantDayTickEvent());

			Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(1);
			var @event = Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().FirstOrDefault();
			var staffingDaysNum = StaffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 49);
			var extraDaysForForecast = SkillForecastSettingsReader.NumberOfExtraDaysInFuture;
			//@event.StartDateTime.Should().Be.EqualTo(ReadModelStartTimeRepository.FakeTime.Value.AddDays(staffingDaysNum+extraDaysForForecast).Date);
			@event.EndDateTime.Should().Be.EqualTo(@event.StartDateTime.AddDays(extraDaysForForecast).Date);
		}

		[Test]
		public void ShouldNotPublishEventForNext8DaysIfRecentlyUpdated()
		{
			Now.Is(new DateTime(2019, 1, 30, 10, 0, 0, DateTimeKind.Utc));
			//ReadModelStartTimeRepository.FakeTime = new DateTime(2019, 1, 24, 10, 0, 0, DateTimeKind.Utc);
			Target.Handle(new TenantDayTickEvent());

			Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldNotPublishEventForNext8DaysIfRecentlyUpdated2()
		{
		}

		private void createFakeStartTimeModel(DateTime lastScheduledTime)
		{
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			var fakeStartTimeModel = new FakeStartTimeModel()
			{
				LastScheduledTime = lastScheduledTime,
				BusinessUnit = bu.Id.GetValueOrDefault(),
				JobName = "TriggerSkillForecastReadModel"
			};
			ReadModelStartTimeRepository.List.Add(fakeStartTimeModel);
			
			
		}
	}

	
	public class FakeReadModelStartTimeRepository : IReadModelStartTimeRepository
	{
		public List<FakeStartTimeModel> List = new List<FakeStartTimeModel>();

		public DateTime? GetLastCalculatedTime(Guid bu, string jobName)
		{
			return List.FirstOrDefault(x => x.BusinessUnit == bu && x.JobName.Equals(jobName)).LastScheduledTime;
		}
	}

	public class FakeStartTimeModel
	{
		public DateTime LastScheduledTime { get; set; }
		public Guid BusinessUnit { get; set; }
		public string JobName { get; set; }
	}
}