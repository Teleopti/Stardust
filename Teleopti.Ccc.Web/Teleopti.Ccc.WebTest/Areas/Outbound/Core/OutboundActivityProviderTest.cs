using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class OutboundActivityProviderTest
	{
		private FakeActivityRepository _activityRepository;
		private ActivityProvider _target;

		[SetUp]
		public void Init()
		{
			_activityRepository = new FakeActivityRepository();
		}

		[Test]
		public void ShouldLoadAllActivitiesThatRequiresSkill()
		{
			var activity1 = ActivityFactory.CreateActivity("a1");
			activity1.RequiresSkill = true;
			_activityRepository.Add(activity1);
			var activity2 = ActivityFactory.CreateActivity("a2");
			activity2.RequiresSkill = false;
			_activityRepository.Add(activity2);

			_target = new ActivityProvider(_activityRepository);

			var result = _target.GetAll();

			result.Count().Should().Be.EqualTo(1);
			result.First().Name.Should().Be.EqualTo("a1");
		}		
		
		[Test]
		public void ShouldLoadNothingWhenActivityNotRequiresSkill()
		{
			var activity1 = ActivityFactory.CreateActivity("a1");
			activity1.RequiresSkill = false;
			_activityRepository.Add(activity1);
			var activity2 = ActivityFactory.CreateActivity("a2");
			activity2.RequiresSkill = false;
			_activityRepository.Add(activity2);

			_target = new ActivityProvider(_activityRepository);

			var result = _target.GetAll();

			result.Count().Should().Be.EqualTo(0);
		}
	}
}
