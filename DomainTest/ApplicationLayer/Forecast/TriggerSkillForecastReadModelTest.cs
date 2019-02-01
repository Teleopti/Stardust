using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
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
	public class TriggerSkillForecastReadModelTest : IIsolateSystem
	{
		public FakeSystemJobStartTimeRepository SystemJobStartTimeRepository;
		public MutableNow Now;
		public FakeEventPublisher Publisher;
		public TriggerSkillForecastReadModel Target;
		public SkillForecastReadModelPeriodBuilder SkillForecastReadModelPeriodBuilder;
		public StaffingSettingsReader49Days StaffingSettingsReader;
		public SkillForecastSettingsReader SkillForecastSettingsReader;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public void Isolate(IIsolate isolate)
		{
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
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			Now.Is(new DateTime(2019, 1, 30, 10, 0, 0, DateTimeKind.Utc));
			var lastExecuted = new DateTime(2019, 1, 23, 10, 0, 0, DateTimeKind.Utc);
			createFakeRecord(bu.Id.GetValueOrDefault(),lastExecuted );

			Target.Handle(new TenantDayTickEvent());

			Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(1);
			var @event = Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().FirstOrDefault();
			var staffingDaysNum = StaffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 49);
			var extraDaysForForecast = SkillForecastSettingsReader.NumberOfExtraDaysInFuture;
			@event.StartDateTime.Should().Be.EqualTo(lastExecuted.AddDays(staffingDaysNum+extraDaysForForecast).Date);
			@event.EndDateTime.Should().Be.EqualTo(@event.StartDateTime.AddDays(extraDaysForForecast).Date);
		}

		[Test]
		public void ShouldNotPublishEventForNext8DaysIfRecentlyUpdated()
		{
			var bu = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu);
			Now.Is(new DateTime(2019, 1, 30, 10, 0, 0, DateTimeKind.Utc));
			createFakeRecord(bu.Id.GetValueOrDefault(), new DateTime(2019, 1, 24, 10, 0, 0, DateTimeKind.Utc));
			Target.Handle(new TenantDayTickEvent());

			Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldPublishEventForAllBus()
		{
			var bu1 = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu1);
			var bu2 = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.Add(bu2);
			Now.Is(new DateTime(2019, 1, 30, 10, 0, 0, DateTimeKind.Utc));

			Target.Handle(new TenantDayTickEvent());

			Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(2);
			var allEvents = Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Select(x=>x);
			allEvents.Any(x=>x.LogOnBusinessUnitId==bu1.Id.GetValueOrDefault()).Should().Be.True();
			allEvents.Any(x=>x.LogOnBusinessUnitId==bu2.Id.GetValueOrDefault()).Should().Be.True();
		}

		[Test]
		public void ShouldPublishJobForTheBuThatHasOldReadModel()
		{
			var bu1 = BusinessUnitFactory.CreateWithId("bu");
			var bu2 = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.AddRange(new []{bu1,bu2});
			Now.Is(new DateTime(2019, 1, 30, 10, 0, 0, DateTimeKind.Utc));
			var lastExecuted = new DateTime(2019, 1, 23, 10, 0, 0, DateTimeKind.Utc);
			createFakeRecord(bu1.Id.GetValueOrDefault(), lastExecuted);
			createFakeRecord(bu2.Id.GetValueOrDefault(), lastExecuted.AddDays(3));

			Target.Handle(new TenantDayTickEvent());

			Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(1);
			var @event = Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().FirstOrDefault();
			@event.LogOnBusinessUnitId.Should().Be.EqualTo(bu1.Id.GetValueOrDefault());
		}

		[Test]
		public void ShouldPublishJobForAllOldBUs()
		{
			var bu1 = BusinessUnitFactory.CreateWithId("bu");
			var bu2 = BusinessUnitFactory.CreateWithId("bu");
			BusinessUnitRepository.AddRange(new[] { bu1, bu2 });
			Now.Is(new DateTime(2019, 1, 30, 10, 0, 0, DateTimeKind.Utc));
			var lastExecuted = new DateTime(2019, 1, 23, 10, 0, 0, DateTimeKind.Utc);
			createFakeRecord(bu1.Id.GetValueOrDefault(), lastExecuted);
			createFakeRecord(bu2.Id.GetValueOrDefault(), lastExecuted);

			Target.Handle(new TenantDayTickEvent());

			Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(2);
			var allEvents = Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Select(x=>x);
			allEvents.Any(x=>x.LogOnBusinessUnitId==bu1.Id.GetValueOrDefault()).Should().Be.True();
			allEvents.Any(x=>x.LogOnBusinessUnitId==bu2.Id.GetValueOrDefault()).Should().Be.True();
		}

		//[Test]
		//public void ShouldNotPublishEventIfAlreadyPublished()
		//{
		//	var bu = BusinessUnitFactory.CreateWithId("bu");
		//	BusinessUnitRepository.Add(bu);

		//	Now.Is(new DateTime(2019, 1, 30, 10, 0, 0, DateTimeKind.Utc));
		//	Target.Handle(new TenantDayTickEvent());
		//	Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(1);

		//	Now.Is(new DateTime(2019, 1, 31, 10, 0, 0, DateTimeKind.Utc));
		//	Target.Handle(new TenantDayTickEvent());
		//	Publisher.PublishedEvents.OfType<UpdateSkillForecastReadModelEvent>().Count().Should().Be.EqualTo(0);
		//}

		private void createFakeRecord(Guid bu,DateTime lastScheduledTime)
		{
			
			var fakeStartTimeModel = new FakeStartTimeModel()
			{
				StartedAt = lastScheduledTime,
				BusinessUnit = bu,
				JobName = JobNamesForJoStartTime.TriggerSkillForecastReadModel
			};
			SystemJobStartTimeRepository.EntryList.Add(fakeStartTimeModel);
			
		}
	}
}