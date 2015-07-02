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
		public void ShouldAllActivities()
		{
			_target = new OutboundActivityProvider(_activityRepository);

			var result = _target.GetAll();

			result.Count().Should().Be.EqualTo(1);
		}
	}
}
