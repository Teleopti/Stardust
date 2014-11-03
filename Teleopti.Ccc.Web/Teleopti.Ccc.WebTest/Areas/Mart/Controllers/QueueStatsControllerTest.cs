using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.Mart.Controllers;
using Teleopti.Ccc.Web.Areas.Mart.Core;
using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.WebTest.Areas.Mart.Controllers
{
	[TestFixture]
	class QueueStatsControllerTest
	{

		[Test]
		public void ShouldCallHandler()
		{
			var queueData = new QueueStatsModel();
			var handler = MockRepository.GenerateMock<IQueueStatHandler>();
			var controller = new QueueStatsController(handler);
			controller.Post(queueData);
			handler.AssertWasCalled(x => x.Handle(queueData));
		}
	}
}
