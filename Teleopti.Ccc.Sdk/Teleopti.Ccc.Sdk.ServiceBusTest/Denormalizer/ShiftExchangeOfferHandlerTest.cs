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
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var shiftExchangeOffer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			shiftExchangeOffer.Stub(x => x.Status).PropertyBehavior();
			shiftExchangeOffer.Stub(x => x.Checksum).Return(checksum);
			var personId = Guid.NewGuid();
			var person = PersonFactory.CreatePerson();
			person.SetId(personId);
			var dateTime = new DateTime();
			var date = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
			personRequestRepository.Stub(x => x.FindOfferByStatus(person, date, 0)).Return(new[] { shiftExchangeOffer });
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

		[Test]
		public void ShouldSetOfferInvalidWhenScheduleIsEmpty()
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
			var date = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);

			personRequestRepository.Stub(x => x.FindOfferByStatus(person, date, 0)).Return(new[] { shiftExchangeOffer });
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(personId)).Return(person);

			
			var target = new ShiftExchangeOfferHandler(personRepository,
								 MockRepository.GenerateMock<IPushMessagePersister>(), personRequestRepository);
			target.Handle(new ProjectionChangedEvent
			{
				PersonId = personId,
				ScheduleDays = new[] {
					new ProjectionChangedEventScheduleDay{CheckSum = checksum}, 
					new ProjectionChangedEventScheduleDay{NotScheduled = true} 
				}
			});

			shiftExchangeOffer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Invalid);
		}
	}
}