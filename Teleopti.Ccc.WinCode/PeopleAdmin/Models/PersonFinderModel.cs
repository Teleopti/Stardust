using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    public interface IPersonFinderModel
    {
        IPersonFinderSearchCriteria SearchCriteria { get; set; }
        void Find();
    }

    public class PersonFinderModel : IPersonFinderModel
    {
        private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;
        public IPersonFinderSearchCriteria SearchCriteria { get; set; }

        public PersonFinderModel(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository, IPersonFinderSearchCriteria searchCriteria)
        {
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
            SearchCriteria = searchCriteria;
        }

        public void Find()
        {
            _personFinderReadOnlyRepository.Find(SearchCriteria);
            var today = new DateOnly(DateTime.Today);
        	var bu = ((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).BusinessUnit.Id;
            var auth = PrincipalAuthorization.Instance();
            foreach (var personFinderDisplayRow in SearchCriteria.DisplayRows)
            {
                if(personFinderDisplayRow.PersonId != new Guid() )
                {
                    personFinderDisplayRow.Grayed =
                        !auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, today,
                                         personFinderDisplayRow);
                }
				if(personFinderDisplayRow.BusinessUnitId != new Guid() )
				{
					personFinderDisplayRow.Grayed = personFinderDisplayRow.BusinessUnitId != bu;
				}
            }
        }
    }
}
