using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
	public interface IPersonFinderModel
	{
		IPeoplePersonFinderSearchCriteria SearchCriteria { get; set; }
		void Find();
	}

	public class PersonFinderModel : IPersonFinderModel
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;
		public IPeoplePersonFinderSearchCriteria SearchCriteria { get; set; }

		public PersonFinderModel(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository, IPeoplePersonFinderSearchCriteria searchCriteria)
		{
			_personFinderReadOnlyRepository = personFinderReadOnlyRepository;
			SearchCriteria = searchCriteria;
		}

		public void Find()
		{
			_personFinderReadOnlyRepository.FindPeople(SearchCriteria);
			var today = DateOnly.Today;
			var bu = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnitId;
			var auth = Domain.Security.Principal.PrincipalAuthorization.Current();
			foreach (var personFinderDisplayRow in SearchCriteria.DisplayRows)
			{
				if (personFinderDisplayRow.PersonId != Guid.Empty)
				{
					personFinderDisplayRow.Grayed =
						 !auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage, today,
												personFinderDisplayRow);
				}
				if (!personFinderDisplayRow.Grayed && personFinderDisplayRow.BusinessUnitId != Guid.Empty)
				{
					personFinderDisplayRow.Grayed = personFinderDisplayRow.BusinessUnitId != bu;
				}
			}
		}
	}
}
