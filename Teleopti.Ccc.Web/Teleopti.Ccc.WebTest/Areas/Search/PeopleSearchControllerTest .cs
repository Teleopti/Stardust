using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Ccc.Web.Areas.Search.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
    public class PeopleSearchControllerTest
    {
        [Test]
        public void ShouldSearchForPeople()
        {
            var field = PersonFinderField.Skill;
            string keyword = null;
            var searchRepository = MockRepository.GenerateMock<IPersonFinderReadOnlyRepository>();
            var personRepository = MockRepository.GenerateMock<IPersonRepository>();


            var person = PersonFactory.CreatePersonWithGuid("Ashley", "Andeen");
            person.Email = "ashley.andeen@abc.com";

            var personId = person.Id.Value;
            var personFinderDisplayRow = new PersonFinderDisplayRow
            {
                FirstName = "Ashley",
                LastName = "Andeen",
                EmploymentNumber = "1011",
                PersonId = personId,
                RowNumber = 1
            };

            searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
            {
                field = c.Field;
                keyword = c.SearchValue;
                c.SetRow(1, personFinderDisplayRow);
                return true;
            }));

            personRepository.Stub(x => x.FindPeople(new List<Guid> { personId })).IgnoreArguments().Return(new List<IPerson> { person });

            var target = new PeopleSearchController(searchRepository, personRepository, new FakePermissionProvider());
            var result = target.GetResult("Ashley", 1, 1);
            var peopleList = result as OkNegotiatedContentResult<IEnumerable<PeopleSummary>>;
            Assert.NotNull(peopleList);
            var first = peopleList.Content.First();
            first.FirstName.Equals("Ashley");
            first.LastName.Equals("Andeen");
            first.EmploymentNumber.Equals("1011");
            first.PersonId.Equals(personId);
            first.Email.Equals("ashley.andeen@abc.com");
        }

        [Test]
        public void ShouldSearchForPeopleWithNoPermission()
        {
            var personId = Guid.NewGuid();
            var searchRepository = MockRepository.GenerateMock<IPersonFinderReadOnlyRepository>();
            var personRepository = MockRepository.GenerateMock<IPersonRepository>();
            var personFinderDisplayRow = new PersonFinderDisplayRow { FirstName = "Ashley", LastName = "Andeen", EmploymentNumber = "1011", PersonId = personId, RowNumber = 1 };

            searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
            {
                c.SetRow(1, personFinderDisplayRow);
                return true;
            }));
            personRepository.Stub(x => x.FindPeople(new List<Guid>())).IgnoreArguments().Return(new List<IPerson>());

            var target = new PeopleSearchController(searchRepository, personRepository, new FakeNoPermissionProvider());
            var result = (OkNegotiatedContentResult<IEnumerable<PeopleSummary>>)target.GetResult("ashley", 1, 1);
            result.Content.Should().Be.Empty();
        }
    }
}
