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
		public void ShouldGetOutboundActivity()
		{
			_activityRepository.SetOutboundActivity(true);
			_target = new OutboundActivityProvider(_activityRepository);

			var result = _target.GetAll();

			result.First().Id.Should().Be.EqualTo(_activityRepository.LoadAll().First().Id);
		}

		[Test]
		public void ShouldNotGetNotOutboundActivity()
		{
			_activityRepository.SetOutboundActivity(false);
			_target = new OutboundActivityProvider(_activityRepository);

			var result = _target.GetAll();

			result.Count().Should().Be.EqualTo(0);
		}
	}
}
