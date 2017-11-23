using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.InfrastructureTest.MessageBroker;

namespace Teleopti.Ccc.InfrastructureTest.ScheduleChangeMessage
{
	[TestFixture, MessageBrokerUnitOfWorkTest]
	public class PersonScheduleChangeMessageRepositoryTest
	{
		public IPersonScheduleChangeMessageRepository Target;
		public INow Now;

		[Test]
		public void ShouldAddMessage()
		{
			var personId = Guid.NewGuid();
			var timeStamp = Now.UtcDateTime();
			var date = new DateTime(2017, 11, 23);
			Target.Add(new PersonScheduleChangeMessage
			{
				StartDate = date,
				EndDate = date,
				PersonId = personId,
				TimeStamp = timeStamp
			});
			var messages = Target.PopMessages(personId);
			messages.Single().StartDate.Should().Be(date);
			messages.Single().EndDate.Should().Be(date);
			messages.Single().TimeStamp.Should().Be(timeStamp);
			
		}

		[Test]
		public void ShouldRemoveAllMessagesForPersonAfterPopMessages()
		{
			var personId = Guid.NewGuid();
			Target.Add(new PersonScheduleChangeMessage
			{
				StartDate = new DateTime(2017, 11, 23),
				EndDate = new DateTime(2017, 11, 23),
				PersonId = personId,
				TimeStamp = Now.UtcDateTime()
			});
			var messages = Target.PopMessages(personId);
			messages.Count.Should().Be(1);
			messages = Target.PopMessages(personId);
			messages.Count.Should().Be(0);
		}

		[Test]
		public void ShouldNotGetMessagesForWrongPerson()
		{
			var personId = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Target.Add(new PersonScheduleChangeMessage
			{
				StartDate = new DateTime(2017, 11, 23),
				EndDate = new DateTime(2017, 11, 23),
				PersonId = personId,
				TimeStamp = Now.UtcDateTime()
			});

			Target.PopMessages(personId).Count.Should().Be(1);
			Target.PopMessages(personId2).Count.Should().Be(0);
		}
	}
}
