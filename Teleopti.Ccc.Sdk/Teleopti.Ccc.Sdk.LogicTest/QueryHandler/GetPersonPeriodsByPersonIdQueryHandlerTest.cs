using System;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPersonPeriodsByPersonIdQueryHandlerTest
	{
		[Test]
		public void ShouldGetPersonPeriodsWithinGivenRangeForSpecifiedPeople()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2001, 1, 1),
				TeamFactory.CreateTeam("Team 1", "Paris")).WithId();
			var personRepository = new FakePersonRepository { person };
			var target = new GetPersonPeriodsByPersonIdQueryHandler(personRepository, new FakeCurrentUnitOfWorkFactory(),
				new PersonPeriodAssembler(new ExternalLogOnAssembler()));
			var query = new GetPersonPeriodsByPersonIdQueryDto
			{
				Range =
					new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 1)},
						EndDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 2)}
					}
			};
			query.PersonIdCollection.Add(person.Id.GetValueOrDefault());
			var result = target.Handle(query);
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotGetPersonPeriodsWithinGivenRangeForSpecifiedPeopleWhenNotPermitted()
		{
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2001, 1, 1),
				TeamFactory.CreateTeam("Team 1", "Paris")).WithId();
			var personRepository = new FakePersonRepository { person };
			var target = new GetPersonPeriodsByPersonIdQueryHandler(personRepository, new FakeCurrentUnitOfWorkFactory(),
				new PersonPeriodAssembler(new ExternalLogOnAssembler()));
			var query = new GetPersonPeriodsByPersonIdQueryDto
			{
				Range =
					new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto { DateTime = new DateTime(2001, 1, 1) },
						EndDate = new DateOnlyDto { DateTime = new DateTime(2001, 1, 2) }
					}
			};
			query.PersonIdCollection.Add(person.Id.GetValueOrDefault());

			using (new CustomAuthorizationContext(new NoPermission()))
			{
				var result = target.Handle(query);
				result.Count.Should().Be.EqualTo(0);
			}
		}

		[Test, ExpectedException(typeof (FaultException))]
		public void ShouldOnlyAllowToGetPersonPeriodsForFiftyPersons()
		{
			var query = new GetPersonPeriodsByPersonIdQueryDto
			{
				Range =
					new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 1)},
						EndDate = new DateOnlyDto {DateTime = new DateTime(2001, 1, 2)}
					}
			};

			Enumerable.Range(0, 51).ForEach(i => query.PersonIdCollection.Add(Guid.NewGuid()));

			var personRepository = new FakePersonRepository();
			var target = new GetPersonPeriodsByPersonIdQueryHandler(personRepository, new FakeCurrentUnitOfWorkFactory(),
				new PersonPeriodAssembler(new ExternalLogOnAssembler()));
			target.Handle(query);
		}
	}
}