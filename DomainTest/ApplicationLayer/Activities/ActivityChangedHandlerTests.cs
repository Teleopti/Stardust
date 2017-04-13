using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Activity;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Activities
{
	[DomainTest]
	[TestFixture]
	public class ActivityChangedHandlerTests : ISetup
	{
		public AnalyticsActivityUpdater Target;
		public FakeAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeAnalyticsActivityRepository AnalyticsActivityRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsActivityUpdater>();
		}

		[Test]
		public void ShouldAddActivity()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));

			var activity = ActivityFactory.CreateActivity("Test activity").WithId();
			ActivityRepository.Add(activity);
			AnalyticsActivityRepository.Activities().Should().Be.Empty();

			Target.Handle(new ActivityChangedEvent
			{
				ActivityId = activity.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsActivityRepository.Activities().Count.Should().Be.EqualTo(1);
			var analyticsActivity = AnalyticsActivityRepository.Activities().First();
			assertMapping(activity, analyticsActivity);
		}

		[Test]
		public void ShouldUpdateActivity()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));

			var activity = ActivityFactory.CreateActivity("Updated name activity").WithId();
			ActivityRepository.Add(activity);

			var analyticsActivity = new AnalyticsActivity
			{
				ActivityCode = activity.Id.GetValueOrDefault(),
				ActivityName = "Old name"
			};
			AnalyticsActivityRepository.AddActivity(analyticsActivity);
			AnalyticsActivityRepository.Activities().Should().Not.Be.Empty();

			Target.Handle(new ActivityChangedEvent
			{
				ActivityId = activity.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsActivityRepository.Activities().Count.Should().Be.EqualTo(1);
			var updatedAnalyticsActivity = AnalyticsActivityRepository.Activities().First();
			assertMapping(activity, updatedAnalyticsActivity);
		}

		[Test]
		public void ShouldUpdateDeletedActivity()
		{
			var businessUnitId = Guid.NewGuid();
			BusinessUnitRepository.Has(BusinessUnitFactory.CreateSimpleBusinessUnit().WithId(businessUnitId));

			var activity = ActivityFactory.CreateActivity("Updated name activity").WithId();
			activity.SetDeleted();
			ActivityRepository.Add(activity);

			var analyticsActivity = new AnalyticsActivity
			{
				ActivityCode = activity.Id.GetValueOrDefault(),
				ActivityName = "Old name"
			};
			AnalyticsActivityRepository.AddActivity(analyticsActivity);
			AnalyticsActivityRepository.Activities().Should().Not.Be.Empty();

			Target.Handle(new ActivityChangedEvent
			{
				ActivityId = activity.Id.GetValueOrDefault(),
				LogOnBusinessUnitId = businessUnitId
			});

			AnalyticsActivityRepository.Activities().Count.Should().Be.EqualTo(1);
			var updatedAnalyticsActivity = AnalyticsActivityRepository.Activities().First();
			assertMapping(activity, updatedAnalyticsActivity);
		}

		private static void assertMapping(IActivity activity, AnalyticsActivity analyticsActivity)
		{
			analyticsActivity.ActivityCode.Should().Be.EqualTo(activity.Id);
			analyticsActivity.ActivityName.Should().Be.EqualTo(activity.Name);
			analyticsActivity.InContractTime.Should().Be.EqualTo(activity.InContractTime);
			analyticsActivity.InPaidTime.Should().Be.EqualTo(activity.InPaidTime);
			analyticsActivity.InReadyTime.Should().Be.EqualTo(activity.InReadyTime);
			analyticsActivity.InWorkTime.Should().Be.EqualTo(activity.InWorkTime);
			analyticsActivity.DisplayColor.Should().Be.EqualTo(activity.DisplayColor.ToArgb());
			analyticsActivity.DisplayColorHtml.Should().Be.EqualTo(ColorTranslator.ToHtml(activity.DisplayColor));
			analyticsActivity.IsDeleted.Should().Be.EqualTo(activity.IsDeleted);
		}
	}
}