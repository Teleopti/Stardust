using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class SetPersonOptionalValuesForPersonCommandHandlerTest
    {
        private MockRepository mock;
        private IOptionalColumnRepository optionalColumnRepository;
        private IUnitOfWorkFactory unitOfWorkFactory;
    	private IPersonRepository personRepository;
        private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            mock = new MockRepository();
            optionalColumnRepository = mock.StrictMock<IOptionalColumnRepository>();
            personRepository = mock.StrictMock<IPersonRepository>();
            unitOfWorkFactory = mock.StrictMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mock.DynamicMock<ICurrentUnitOfWorkFactory>();
        }

		[Test]
		public void ShouldSetOptionalColumnValueSuccessfully()
		{
			var untiOfWork = mock.StrictMock<IUnitOfWork>();
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());
            var optionalColumn = new OptionalColumn("Shoe size");
			    
			using (mock.Record())
			{
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
				Expect.Call(personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
			    Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>{optionalColumn});
				Expect.Call(() => untiOfWork.PersistAll());
				Expect.Call(untiOfWork.Dispose);
			}
			using (mock.Playback())
			{
                var command = new SetPersonOptionalValuesForPersonCommandDto { PersonId = person.Id.GetValueOrDefault() };
                command.OptionalValueCollection.Add(new OptionalValueDto { Key = "Shoe size", Value = "42" });

				var target = new SetPersonOptionalValuesForPersonCommandHandler(optionalColumnRepository, personRepository, currentUnitOfWorkFactory, new FullPermission());
				target.Handle(command);

				var result = command.Result;
				result.AffectedItems.Should().Be.EqualTo(1);
				result.AffectedId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			    
                person.GetColumnValue(optionalColumn).Description.Should().Be.EqualTo("42");
			}
		}

        [Test]
        public void ShouldRemoveOptionalColumnValueSuccessfully()
        {
            var untiOfWork = mock.StrictMock<IUnitOfWork>();
            var person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());
           
            var optionalColumn = new OptionalColumn("Shoe size");
            person.SetOptionalColumnValue(new OptionalColumnValue("42"), optionalColumn);

            using (mock.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
                Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
                Expect.Call(personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
                Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn> { optionalColumn });
                Expect.Call(() => untiOfWork.PersistAll());
                Expect.Call(untiOfWork.Dispose);
            }
            using (mock.Playback())
            {
                var command = new SetPersonOptionalValuesForPersonCommandDto { PersonId = person.Id.GetValueOrDefault() };
                command.OptionalValueCollection.Add(new OptionalValueDto { Key = "Shoe size", Value = "" });

				var target = new SetPersonOptionalValuesForPersonCommandHandler(optionalColumnRepository, personRepository, currentUnitOfWorkFactory, new FullPermission());
				target.Handle(command);
				
				person.GetColumnValue(optionalColumn).Should().Be.Null();
            }
        }

		[Test]
		public void ShouldThrowAnExceptionGivenThePersonIdNotExisting()
        {
            var untiOfWork = mock.StrictMock<IUnitOfWork>();
		    var personId = Guid.NewGuid();
			using (mock.Record())
			{
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
                Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
                Expect.Call(() => untiOfWork.PersistAll());
                Expect.Call(untiOfWork.Dispose);

				Expect.Call(personRepository.Get(personId)).Return(null);
				Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Repeat.Never();
			}
			Assert.Throws<FaultException>(() =>
			{
				using (mock.Playback())
				{
					var command = new SetPersonOptionalValuesForPersonCommandDto {PersonId = personId};
					command.OptionalValueCollection.Add(new OptionalValueDto {Key = "Shoe size", Value = "42"});

					var target = new SetPersonOptionalValuesForPersonCommandHandler(optionalColumnRepository, personRepository, currentUnitOfWorkFactory, new FullPermission());
					target.Handle(command);
				}
			});
        }

		[Test]
		public void ShouldNotSetOptionalColumnValuesWhenNotPermittedToPerson()
		{
			var untiOfWork = mock.StrictMock<IUnitOfWork>();
			var person = PersonFactory.CreatePerson();
			person.SetId(Guid.NewGuid());

			using (mock.Record())
			{
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
				Expect.Call(personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
				Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Repeat.Never();
				Expect.Call(() => untiOfWork.PersistAll()).Repeat.Never();
				Expect.Call(untiOfWork.Dispose);
			}

			using (mock.Playback())
			{
				var command = new SetPersonOptionalValuesForPersonCommandDto {PersonId = person.Id.GetValueOrDefault()};
				command.OptionalValueCollection.Add(new OptionalValueDto {Key = "Shoe size", Value = "42"});

				var target = new SetPersonOptionalValuesForPersonCommandHandler(optionalColumnRepository, personRepository, currentUnitOfWorkFactory, new NoPermission());
				Assert.Throws<FaultException>(() => target.Handle(command));
			}
		}
    }
}
