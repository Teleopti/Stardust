using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Activity;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ActivityTests
{
	[TestFixture]
	public class ActivityChangedHandlerTests
	{
		private AnalyticsActivityUpdater _target;
		private FakeAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private FakeActivityRepository _activityRepository;
		private FakeAnalyticsActivityRepository _analyticsActivityRepository;

		[SetUp]
		public void Setup()
		{
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			_activityRepository = new FakeActivityRepository();
			_analyticsActivityRepository = new FakeAnalyticsActivityRepository();

			_target = new AnalyticsActivityUpdater(_analyticsBusinessUnitRepository, _activityRepository, _analyticsActivityRepository);
		}

		[Test]
		public void ShouldAddActivity()
		{
			var activity = ActivityFactory.CreateActivity("Test activity").WithId();
			_activityRepository.Add(activity);
			_analyticsActivityRepository.Activities().Should().Be.Empty();

			_target.Handle(new ActivityChangedEvent
			{
				ActivityId = activity.Id.GetValueOrDefault()
			});

			_analyticsActivityRepository.Activities().Count.Should().Be.EqualTo(1);
			var analyticsActivity = _analyticsActivityRepository.Activities().First();
			assertMapping(activity, analyticsActivity);
		}

		[Test]
		public void ShouldUpdateActivity()
		{
			var activity = ActivityFactory.CreateActivity("Updated name activity").WithId();
			_activityRepository.Add(activity);

			var analyticsActivity = new AnalyticsActivity
			{
				ActivityCode = activity.Id.GetValueOrDefault(),
				ActivityName = "Old name"
			};
			_analyticsActivityRepository.AddActivity(analyticsActivity);
			_analyticsActivityRepository.Activities().Should().Not.Be.Empty();

			_target.Handle(new ActivityChangedEvent
			{
				ActivityId = activity.Id.GetValueOrDefault()
			});

			_analyticsActivityRepository.Activities().Count.Should().Be.EqualTo(1);
			var updatedAnalyticsActivity = _analyticsActivityRepository.Activities().First();
			assertMapping(activity, updatedAnalyticsActivity);
		}

		[Test]
		public void ShouldUpdateDeletedActivity()
		{
			var activity = ActivityFactory.CreateActivity("Updated name activity").WithId();
			activity.SetDeleted();
			_activityRepository.Add(activity);

			var analyticsActivity = new AnalyticsActivity
			{
				ActivityCode = activity.Id.GetValueOrDefault(),
				ActivityName = "Old name"
			};
			_analyticsActivityRepository.AddActivity(analyticsActivity);
			_analyticsActivityRepository.Activities().Should().Not.Be.Empty();

			_target.Handle(new ActivityChangedEvent
			{
				ActivityId = activity.Id.GetValueOrDefault()
			});

			_analyticsActivityRepository.Activities().Count.Should().Be.EqualTo(1);
			var updatedAnalyticsActivity = _analyticsActivityRepository.Activities().First();
			assertMapping(activity, updatedAnalyticsActivity);
		}

		private static void assertMapping(Interfaces.Domain.IActivity activity, AnalyticsActivity analyticsActivity)
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