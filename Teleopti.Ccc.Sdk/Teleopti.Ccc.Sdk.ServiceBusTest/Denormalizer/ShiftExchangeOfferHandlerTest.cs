using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class ShiftExchangeOfferHandlerTest
	{
		//RobTodo: need check the type cast from request to offer
		[Test, Ignore]
		public void ShouldSetOfferInvalid()
		{
			const int checksum = 12345678;
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var shiftExchangeOffer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			shiftExchangeOffer.Stub(x => x.Status).PropertyBehavior();
			shiftExchangeOffer.Stub(x => x.Checksum).Return(checksum);
			var personId = Guid.NewGuid();
			var person = PersonFactory.CreatePerson();
			person.SetId(personId);
			var dateTime = new DateTime();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			personRequest.Stub(x => x.Request).Return(shiftExchangeOffer);
			personRequestRepository.Stub(x => x.FindByStatus<ShiftExchangeOffer>(person, dateTime, 0)).Return(new[] { personRequest });
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(personId)).Return(person);
			var target = new ShiftExchangeOfferHandler(personRepository,
								 MockRepository.GenerateMock<IPushMessagePersister>(), personRequestRepository);
			target.Handle(new ProjectionChangedEvent
			{
				PersonId = personId,
				ScheduleDays = new[] {
					new ProjectionChangedEventScheduleDay{CheckSum = checksum}, 
					new ProjectionChangedEventScheduleDay
				{
					Date = dateTime
				} }
			});

			shiftExchangeOffer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Invalid);
		}
	}
}