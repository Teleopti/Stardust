using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
		[Test]
		public void ShouldSetOfferInvalid()
		{
			const int checksum = 12345678;
			var offerRepository = MockRepository.GenerateMock<IShiftExchangeOfferRepository>();
			var shiftExchangeOffer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			shiftExchangeOffer.Stub(x => x.Status).PropertyBehavior();
			shiftExchangeOffer.Stub(x => x.Checksum).Return(checksum);
			var personId = Guid.NewGuid();
			var person = PersonFactory.CreatePerson();
			person.SetId(personId);
			var dateTime = new DateTime();
			offerRepository.Stub(x => x.FindPendingOffer(person, new DateOnly(dateTime))).Return(new[] { shiftExchangeOffer });
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(personId)).Return(person);
			var target = new ShiftExchangeOfferHandler(personRepository,
								offerRepository, MockRepository.GenerateMock<IPushMessagePersister>());
			target.Handle(new ProjectionChangedEvent
			{
				PersonId = personId,
				ScheduleDays = new[] {
					new ProjectionChangedEventScheduleDay(), 
					new ProjectionChangedEventScheduleDay
				{
					Date = dateTime,
					CheckSum = checksum
				} }
			});

			shiftExchangeOffer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Invalid);
		}
	}
}