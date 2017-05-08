using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class ApproveBatchRequestsCommandHandlerTest
	{
		[SetUp]
		public void SetUp()
		{

		}

		[Test]
		public void ShouldPublishApproveRequestsWithValidatorsEvent()
		{
			var personRequestIdList = new Guid[] { new Guid() };
			var command = new ApproveBatchRequestsCommand()
			{
				PersonRequestIdList = personRequestIdList
			};
			var publisher = new LegacyFakeEventPublisher();
			var target = new ApproveBatchRequestsCommandHandler(publisher);

			target.Handle(command);
			
			var published = publisher.PublishedEvents.OfType<ApproveRequestsWithValidatorsEvent>().Single();
			published.PersonRequestIdList.Should().Contain(personRequestIdList[0]);
		}

	}
}