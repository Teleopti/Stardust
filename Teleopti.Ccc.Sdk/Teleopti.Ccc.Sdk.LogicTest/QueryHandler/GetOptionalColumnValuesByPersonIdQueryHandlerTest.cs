using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetOptionalColumnValuesByPersonIdQueryHandlerTest
	{
		private MockRepository mocks;
		private GetPersonOptionalValuesByPersonIdQueryHandler target;
		private IPersonRepository personRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IUnitOfWork unitOfWork;
		private IOptionalColumnRepository optionalColumnRepository;
		private Guid personId;
		private IPerson person;
		private IOptionalColumn optionalColumn;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			personRepository = mocks.StrictMock<IPersonRepository>();
			optionalColumnRepository = mocks.StrictMock<IOptionalColumnRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();

			personId = Guid.NewGuid();
			person = PersonFactory.CreatePerson();
			person.SetId(personId);
			optionalColumn = new OptionalColumn("Shoe size");

			target = new GetPersonOptionalValuesByPersonIdQueryHandler(optionalColumnRepository, personRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetOptionalColumnsValuesByPersonId()
		{
			person.AddOptionalColumnValue(new OptionalColumnValue("42"), optionalColumn);

			using (mocks.Record())
			{
				Expect.Call(personRepository.Get(personId)).Return(person);
				Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn>{optionalColumn});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPersonOptionalValuesByPersonIdQueryDto { PersonId = personId });
				result.First().PersonId.Should().Be.EqualTo(personId);
				result.First().OptionalValueCollection.First().Key.Should().Be.EqualTo("Shoe size");
				result.First().OptionalValueCollection.First().Value.Should().Be.EqualTo("42");
			}
		}

		[Test]
		public void ShouldGetOptionalColumnsValuesByPersonIdWhenNoValueIsSet()
		{
			using (mocks.Record())
			{
				Expect.Call(personRepository.Get(personId)).Return(person);
				Expect.Call(optionalColumnRepository.GetOptionalColumns<Person>()).Return(new List<IOptionalColumn> { optionalColumn });
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPersonOptionalValuesByPersonIdQueryDto { PersonId = personId });
				result.First().PersonId.Should().Be.EqualTo(personId);
				result.First().OptionalValueCollection.First().Key.Should().Be.EqualTo("Shoe size");
				result.First().OptionalValueCollection.First().Value.Should().Be.EqualTo(string.Empty);
			}
		}

		[Test]
		public void ShouldHandlePersonByIdNotFound()
		{
			using (mocks.Record())
			{
				Expect.Call(personRepository.Get(personId)).Return(null);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPersonOptionalValuesByPersonIdQueryDto { PersonId = personId });
				result.Count.Should().Be.EqualTo(0);
			}
		}
	}
}