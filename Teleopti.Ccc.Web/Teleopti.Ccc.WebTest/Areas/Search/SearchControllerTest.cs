using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Search
{
	public class SearchControllerTest
	{
		[Test]
		public void ShouldSearchForPeople()
		{
			var personId = Guid.NewGuid();
			var field = PersonFinderField.Skill;
			string keyword = null;
			var searchRepository = MockRepository.GenerateMock<IPersonFinderReadOnlyRepository>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var personFinderDisplayRow = new PersonFinderDisplayRow{FirstName = "Ashley",LastName = "Andeen",EmploymentNumber = "1011",PersonId = personId,RowNumber = 1};
				
			searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
			{
				field = c.Field;
				keyword = c.SearchValue;
				c.SetRow(1,personFinderDisplayRow);
				return true;
			}));
			permissionProvider.Stub(
				x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, DateOnly.Today, personFinderDisplayRow))
				.Return(true);

			var target = new SearchController(searchRepository,permissionProvider);
			var result = (OkNegotiatedContentResult<IEnumerable<IPersonFinderDisplayRow>>)target.GetResult("ashley");
			var first = (dynamic)result.Content.First();
			first.FirstName.Equals("Ashley");
			first.LastName.Equals("Andeen");
			first.EmploymentNumber.Equals("1011");
			first.PersonId.Equals(personId);
			field.Should().Be.EqualTo(PersonFinderField.All);
			keyword.Should().Be.EqualTo("ashley");
		}

		[Test]
		public void ShouldSearchForPeopleWithNoPermission()
		{
			var personId = Guid.NewGuid();
			var searchRepository = MockRepository.GenerateMock<IPersonFinderReadOnlyRepository>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			var personFinderDisplayRow = new PersonFinderDisplayRow { FirstName = "Ashley", LastName = "Andeen", EmploymentNumber = "1011", PersonId = personId,RowNumber = 1};

			searchRepository.Stub(x => x.Find(null)).Callback(new Func<IPersonFinderSearchCriteria, bool>(c =>
			{
				c.SetRow(1, personFinderDisplayRow);
				return true;
			}));
			permissionProvider.Stub(
				x => x.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, DateOnly.Today, personFinderDisplayRow))
				.Return(false);

			var target = new SearchController(searchRepository, permissionProvider);
			var result = (OkNegotiatedContentResult<IEnumerable<IPersonFinderDisplayRow>>)target.GetResult("ashley");
			result.Content.Should().Be.Empty();
		}
	}
}
