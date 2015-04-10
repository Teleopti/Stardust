﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Search.Controllers
{
    public class PeopleSearchController : ApiController
    {
        private readonly IPersonFinderReadOnlyRepository _searchRepository;
        private readonly IPermissionProvider _permissionProvider;
        private readonly IPersonRepository _personRepository;
        private readonly IOptionalColumnRepository _optionalColumnRepository;

        public PeopleSearchController(IPersonFinderReadOnlyRepository searchRepository, IPersonRepository personRepository, IPermissionProvider permissionProvider, IOptionalColumnRepository optionalColumnRepository)
        {
            _searchRepository = searchRepository;
            _personRepository = personRepository;
            _permissionProvider = permissionProvider;
            _optionalColumnRepository = optionalColumnRepository;
        }

        [UnitOfWork]
        [HttpGet, Route("api/Search/People")]
        public virtual IHttpActionResult GetResult(string keyword, int pageSize, int currentPageIndex)
        {
            var optionalColumnCollection = _optionalColumnRepository.GetOptionalColumns<Person>();
			var search = new PersonFinderSearchCriteria(PersonFinderField.All, keyword, pageSize, DateOnly.Today, 1, 1);
            search.CurrentPage = currentPageIndex;
            _searchRepository.Find(search);
            var permittedPersonList =
                search.DisplayRows.Where(
                    r =>
                        r.RowNumber > 0 &&
                        _permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage,
                            DateOnly.Today, r));
            var personIdList = permittedPersonList.Select(x => x.PersonId);
            var peopleList = _personRepository.FindPeople(personIdList);
            var resultPeople = peopleList.Select(x => new
            {
                FirstName = x.Name.FirstName,
                LastName = x.Name.LastName,
                EmploymentNumber = x.EmploymentNumber,
                PersonId = x.Id.Value,
                Email = x.Email,
                LeavingDate = x.TerminalDate == null ? "" : x.TerminalDate.Value.ToShortDateString(),
                OptionalColumnValues = optionalColumnCollection.Select(c =>
                {
                    var columnValue = x.GetColumnValue(c);
                    var value = columnValue == null ? "" : columnValue.Description;
                    return new KeyValuePair<string, string>(c.Name, value);
                })
			}).OrderBy(p=>p.LastName);
            var result = new
            {
                People = resultPeople,
                CurrentPage = currentPageIndex,
                TotalPages = search.TotalPages,
                OptionalColumns = optionalColumnCollection.Select(x => x.Name)
            };
            return Ok(result);
        }
    }
}