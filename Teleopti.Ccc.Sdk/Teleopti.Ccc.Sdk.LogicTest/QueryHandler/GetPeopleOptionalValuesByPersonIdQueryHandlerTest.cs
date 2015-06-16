using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPeopleOptionalValuesByPersonIdQueryHandlerTest
	{
		private MockRepository mocks;
		private GetPeopleOptionalValuesByPersonIdQueryHandler target;
		private IPersonRepository personRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IUnitOfWork unitOfWork;
		private IOptionalColumnRepository optionalColumnRepository;
		private Guid personId;
		private IPerson person;
		private IOptionalColumn optionalColumn;
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			personRepository = mocks.StrictMock<IPersonRepository>();
			optionalColumnRepository = mocks.StrictMock<IOptionalColumnRepository>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();

			personId = Guid.NewGuid();
			person = PersonFactory.CreatePerson();
			person.SetId(personId);
			optionalColumn = new OptionalColumn("Shoe size");

			target = new GetPeopleOptionalValuesByPersonIdQueryHandler(optionalColumnRepository, personRepository, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetOptionalColumnsValuesByPersonId()
		{
			person.AddOptionalColumnValue(new OptionalColumnValue("42"), optionalColumn);

			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			query.PersonIdCollection.Add(personId);

			using (mocks.Record())
			{
				Expect.Call(personRepository.FindPeople(query.PersonIdCollection)).Return(new Collection<IPerson>{person});
				Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>{optionalColumn});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(query);
				result.First().PersonId.Should().Be.EqualTo(personId);
				result.First().OptionalValueCollection.First().Key.Should().Be.EqualTo("Shoe size");
				result.First().OptionalValueCollection.First().Value.Should().Be.EqualTo("42");
			}
		}

		[Test]
		public void ShouldGetOptionalColumnsValuesByPersonIdWhenNoValueIsSet()
		{
			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			query.PersonIdCollection.Add(personId);
			
			using (mocks.Record())
			{
				Expect.Call(personRepository.FindPeople(query.PersonIdCollection)).Return(new Collection<IPerson> { person });
				Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn> { optionalColumn });
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(query);
				result.First().PersonId.Should().Be.EqualTo(personId);
				result.First().OptionalValueCollection.First().Key.Should().Be.EqualTo("Shoe size");
				result.First().OptionalValueCollection.First().Value.Should().Be.EqualTo(string.Empty);
			}
		}

		[Test]
		public void ShouldHandlePersonByIdNotFound()
		{
			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			query.PersonIdCollection.Add(personId);

			using (mocks.Record())
			{
				Expect.Call(personRepository.FindPeople(query.PersonIdCollection)).Return(new Collection<IPerson>());
				Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn> { optionalColumn });
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(query);
				result.Count.Should().Be.EqualTo(0);
			}
		}

		[Test, ExpectedException(typeof(FaultException))]
		public void ShouldNotAllowToGetOptionalValuesWithoutPermissions()
		{
			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			query.PersonIdCollection.Add(personId);

			using (mocks.Record())
			{
				Expect.Call(personRepository.FindPeople(query.PersonIdCollection)).Return(new Collection<IPerson> { person });
				Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn> { optionalColumn }).Repeat.Never();
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
				{
					target.Handle(query);
				}
			}
		}

		[Test, ExpectedException(typeof(FaultException))]
		public void ShouldOnlyAllowToGetOptionalValuesForFiftyPersons()
		{
			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			Enumerable.Range(0,51).ForEach(i=>query.PersonIdCollection.Add(Guid.NewGuid()));

			using (mocks.Record())
			{
				Expect.Call(personRepository.FindPeople(query.PersonIdCollection)).Return(new Collection<IPerson> { person }).Repeat.Never();
				Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn> { optionalColumn }).Repeat.Never();
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.Never();
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory).Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Handle(query);
			}
		}
	}
}