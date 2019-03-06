using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class SendPushMessageToPersonCommandHandlerTest
    {
        private MockRepository mock;
		private IPushMessagePersister pushMessageRepository;
        private IUnitOfWorkFactory unitOfWorkFactory;
    	private IPersonRepository personRepository;
        private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            mock = new MockRepository();
            pushMessageRepository = mock.StrictMock<IPushMessagePersister>();
            personRepository = mock.StrictMock<IPersonRepository>();
            unitOfWorkFactory = mock.StrictMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mock.DynamicMock<ICurrentUnitOfWorkFactory>();
        }

		[Test]
		public void ShouldSendPushMessageSuccessfully()
		{
			var untiOfWork = mock.StrictMock<IUnitOfWork>();
			var person = PersonFactory.CreatePerson().WithId();
			var messageId = Guid.NewGuid();

			using (mock.Record())
			{
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
				Expect.Call(personRepository.FindPeople(new[] { person.Id.GetValueOrDefault() })).Return(new Collection<IPerson> { person });
				Expect.Call(() => pushMessageRepository.Add(null, null)).Callback<IPushMessage, IEnumerable<IPerson>>(
					(m, s) =>
					{
						m.SetId(messageId);
						return s != null;
					});
				Expect.Call(() => untiOfWork.PersistAll());
				Expect.Call(untiOfWork.Dispose);
			}
			using (mock.Playback())
			{
				var command = new SendPushMessageToPeopleCommandDto();
				command.Recipients.Add(person.Id.GetValueOrDefault());

				var target = new SendPushMessageToPersonCommandHandler(personRepository, pushMessageRepository, currentUnitOfWorkFactory, new FullPermission());
				target.Handle(command);
				
				command.Result.AffectedItems.Should().Be.EqualTo(1);
				command.Result.AffectedId.Should().Be.EqualTo(messageId);
			}
		}

		[Test]
		public void ShouldNotAllowPushMessagesWithMoreThanFiftyReceivers()
		{
			using (mock.Record())
			{
				Expect.Call(personRepository.FindPeople(new Guid[0])).IgnoreArguments().Repeat.Never();
				Expect.Call(() => pushMessageRepository.Add(null, null)).IgnoreArguments().Repeat.Never();
			}
			using (mock.Playback())
			{
				var command = new SendPushMessageToPeopleCommandDto();
				Enumerable.Range(0, 51).ForEach(i => command.Recipients.Add(Guid.NewGuid()));

				var target = new SendPushMessageToPersonCommandHandler(personRepository, pushMessageRepository, currentUnitOfWorkFactory, new FullPermission());
				Assert.Throws<FaultException>(() => target.Handle(command));
			}
		}

		[Test]
		public void ShouldSetDefaultReplyOptionIfNoneGiven()
		{
			var untiOfWork = mock.StrictMock<IUnitOfWork>();
			var person = PersonFactory.CreatePerson().WithId();

			using (mock.Record())
			{
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
				Expect.Call(personRepository.FindPeople(new[] { person.Id.GetValueOrDefault() })).Return(new Collection<IPerson> { person });
				Expect.Call(() => pushMessageRepository.Add(null, null)).Callback<IPushMessage, IEnumerable<IPerson>>(
					(m, s) =>
					{
						m.ReplyOptions.Single().Should().Be.EqualTo("OK");
						return true;
					});
				Expect.Call(() => untiOfWork.PersistAll());
				Expect.Call(untiOfWork.Dispose);
			}
			using (mock.Playback())
			{
				var command = new SendPushMessageToPeopleCommandDto();
				command.Recipients.Add(person.Id.GetValueOrDefault());

				var target = new SendPushMessageToPersonCommandHandler(personRepository, pushMessageRepository, currentUnitOfWorkFactory, new FullPermission());
				target.Handle(command);
			}
		}

		[Test]
		public void ShouldNotSendPushMessageWhenPersonNotFound()
		{
			var untiOfWork = mock.StrictMock<IUnitOfWork>();
			var personId = Guid.NewGuid();

			using (mock.Record())
			{
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
                Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
                Expect.Call(personRepository.FindPeople(new[] { personId })).Return(new Collection<IPerson>());
				Expect.Call(() => pushMessageRepository.Add(null, null)).IgnoreArguments().Repeat.Never();
				Expect.Call(() => untiOfWork.PersistAll()).Repeat.Never();
				Expect.Call(untiOfWork.Dispose);
			}
			using (mock.Playback())
			{
				var command = new SendPushMessageToPeopleCommandDto();
				command.Recipients.Add(personId);

				var target = new SendPushMessageToPersonCommandHandler(personRepository, pushMessageRepository, currentUnitOfWorkFactory, new FullPermission());
				target.Handle(command);
				command.Result.AffectedItems.Should().Be.EqualTo(0);
				command.Result.AffectedId.HasValue.Should().Be.False();
			}
		}

		[Test]
		public void ShouldNotSendPushMessageWhenNotPermittedToPerson()
		{
			var untiOfWork = mock.StrictMock<IUnitOfWork>();
			var person = PersonFactory.CreatePerson().WithId();

			using (mock.Record())
			{
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
                Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
                Expect.Call(personRepository.FindPeople(new[] { person.Id.GetValueOrDefault() })).Return(new Collection<IPerson> { person });
				Expect.Call(() => pushMessageRepository.Add(null, null)).IgnoreArguments().Repeat.Never();
				Expect.Call(() => untiOfWork.PersistAll()).Repeat.Never();
				Expect.Call(untiOfWork.Dispose);
			}

			using (mock.Playback())
			{
				var command = new SendPushMessageToPeopleCommandDto();
				command.Recipients.Add(person.Id.GetValueOrDefault());

				var target = new SendPushMessageToPersonCommandHandler(personRepository, pushMessageRepository, currentUnitOfWorkFactory, new NoPermission());
				Assert.Throws<FaultException>(() => target.Handle(command));
			}
		}
    }
}
