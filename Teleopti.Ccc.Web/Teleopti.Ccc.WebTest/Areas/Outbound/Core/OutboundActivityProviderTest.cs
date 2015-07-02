using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core
{
	[TestFixture]
	class OutboundActivityProviderTest
	{
		private FakeActivityRepository _activityRepository;
		private OutboundActivityProvider _target;

		[SetUp]
		public void Init()
		{
			_activityRepository = new FakeActivityRepository();
		}

		[Test]
		public void ShouldLoadAllActivitiesThatRequiresSkill()
		{
			_target = new OutboundActivityProvider(_activityRepository);
			//add one activity that req skill and are outbound (A1)
			//add one activity that req skill and are not outbound (A2)
			//add one activity that not req skill and are outbound (A3)
			//add one activity that not req skill and are not outbound (A4)

			var result = _target.GetAll();
			//result should be A1 and A2
			result.Count().Should().Be.EqualTo(1);
		}
	}
}
