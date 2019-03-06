using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPeopleOptionalValuesByPersonIdQueryHandlerTest
	{
		private IPersonRepository personRepository;
		private IOptionalColumnRepository optionalColumnRepository;
		private IPerson person;
		private IOptionalColumn optionalColumn;

		[SetUp]
		public void Setup()
		{
			person = PersonFactory.CreatePerson().WithId();

			personRepository = new FakePersonRepositoryLegacy { person};
			optionalColumnRepository = MockRepository.GenerateMock<IOptionalColumnRepository>();

			optionalColumn = new OptionalColumn("Shoe size");
		}

		[Test]
		public void ShouldGetOptionalColumnsValuesByPersonId()
		{
			person.SetOptionalColumnValue(new OptionalColumnValue("42"), optionalColumn);

			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			query.PersonIdCollection.Add(person.Id.GetValueOrDefault());

			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>())
				.Return(new List<IOptionalColumn> {optionalColumn});

			var target = new GetPeopleOptionalValuesByPersonIdQueryHandler(optionalColumnRepository, personRepository,
				new FakeCurrentUnitOfWorkFactory(null), new FullPermission());
			var result = target.Handle(query);
			result.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			result.First().OptionalValueCollection.First().Key.Should().Be.EqualTo("Shoe size");
			result.First().OptionalValueCollection.First().Value.Should().Be.EqualTo("42");
		}

		[Test]
		public void ShouldGetOptionalColumnsValuesByPersonIdWhenNoValueIsSet()
		{
			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			query.PersonIdCollection.Add(person.Id.GetValueOrDefault());

			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>())
				.Return(new List<IOptionalColumn> {optionalColumn});

			var target = new GetPeopleOptionalValuesByPersonIdQueryHandler(optionalColumnRepository, personRepository,
				new FakeCurrentUnitOfWorkFactory(null), new FullPermission());
			var result = target.Handle(query);
			result.First().PersonId.Should().Be.EqualTo(person.Id.GetValueOrDefault());
			result.First().OptionalValueCollection.First().Key.Should().Be.EqualTo("Shoe size");
			result.First().OptionalValueCollection.First().Value.Should().Be.EqualTo(string.Empty);
		}

		[Test]
		public void ShouldHandlePersonByIdNotFound()
		{
			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			query.PersonIdCollection.Add(person.Id.GetValueOrDefault());

			personRepository.Remove(person);

			optionalColumnRepository.Stub(x => x.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn> {optionalColumn});

			var target = new GetPeopleOptionalValuesByPersonIdQueryHandler(optionalColumnRepository, personRepository,
				new FakeCurrentUnitOfWorkFactory(null), new FullPermission());
			var result = target.Handle(query);
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotAllowToGetOptionalValuesWithoutPermissions()
		{
			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			query.PersonIdCollection.Add(person.Id.GetValueOrDefault());

			var target = new GetPeopleOptionalValuesByPersonIdQueryHandler(optionalColumnRepository, personRepository,
				new FakeCurrentUnitOfWorkFactory(null), new NoPermission());
			Assert.Throws<FaultException>(() => target.Handle(query));
			optionalColumnRepository.AssertWasNotCalled(x => x.GetOptionalColumns<Person>());
		}

		[Test]
		public void ShouldOnlyAllowToGetOptionalValuesForFiftyPersons()
		{
			var query = new GetPeopleOptionalValuesByPersonIdQueryDto();
			Enumerable.Range(0, 51).ForEach(i => query.PersonIdCollection.Add(Guid.NewGuid()));

			var target = new GetPeopleOptionalValuesByPersonIdQueryHandler(optionalColumnRepository, personRepository,
				new FakeCurrentUnitOfWorkFactory(null), new FullPermission());
			Assert.Throws<FaultException>(() => target.Handle(query));
			optionalColumnRepository.AssertWasNotCalled(x => x.GetOptionalColumns<Person>());
		}
	}
}