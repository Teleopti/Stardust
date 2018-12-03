using System;
using Autofac;
using Autofac.Core.Registration;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[DomainTest]
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
			var date = new DateOnly(dateTime);
			personRequestRepository.Stub(x => x.FindOfferByStatus(person, date, 0)).Return(new[] { shiftExchangeOffer });
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(personId)).Return(person);
			var target = new ShiftExchangeOfferHandlerNew(new ShiftExchangeOfferThingy(personRepository,
				MockRepository.GenerateMock<IPushMessagePersister>(), personRequestRepository));
			target.Handle(new ProjectionChangedEventForShiftExchangeOffer
			{
				PersonId = personId,
				Days = new[]
				{
					new ProjectionChangedEventForShiftExchangeOfferDateAndChecksums{Checksum = checksum},
					new ProjectionChangedEventForShiftExchangeOfferDateAndChecksums{Date = dateTime}
				}
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
			var date = new DateOnly(dateTime);

			personRequestRepository.Stub(x => x.FindOfferByStatus(person, date, 0)).Return(new[] { shiftExchangeOffer });
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(personId)).Return(person);


			var target = new ShiftExchangeOfferHandlerNew(new ShiftExchangeOfferThingy(personRepository,
				MockRepository.GenerateMock<IPushMessagePersister>(), personRequestRepository));
			target.Handle(new ProjectionChangedEventForShiftExchangeOffer
			{
				PersonId = personId,
				Days = new[] 
				{
					new ProjectionChangedEventForShiftExchangeOfferDateAndChecksums{Checksum = checksum},
					new ProjectionChangedEventForShiftExchangeOfferDateAndChecksums()
				}
			});

			shiftExchangeOffer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Invalid);
		}
	}
}