using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonSelectorReadOnlyRepository : IPersonSelectorReadOnlyRepository
	{
		private IList<IPersonSelectorOrganization> _organizations = new List<IPersonSelectorOrganization>();
		private ICurrentBusinessUnit _currentBusiness;

		public FakePersonSelectorReadOnlyRepository(ICurrentBusinessUnit currentBusiness)
		{
			_currentBusiness = currentBusiness;
		}

		public void Has(ITeam team)
		{
			if (team.Site.BusinessUnit != null && team.Site.BusinessUnit.Id == _currentBusiness.Current().Id)
				_organizations.Add(new PersonSelectorOrganization
				{
					BusinessUnitId = _currentBusiness.Current().Id.Value,
					TeamId = team.Id,
					SiteId = team.Site.Id,
					Team = team.Description.Name,
					Site = team.Site.Description.Name
				});
		}
		public IList<IPersonSelectorOrganization> GetOrganization(DateOnlyPeriod dateOnlyPeriod, bool loadUsers)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonSelectorOrganization> GetOrganizationForWeb(DateOnlyPeriod dateOnlyPeriod)
		{
			return _organizations;
		}

		public IList<IPersonSelectorBuiltIn> GetBuiltIn(DateOnlyPeriod dateOnlyPeriod, PersonSelectorField loadType, Guid optionalColumnId)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonSelectorUserDefined> GetUserDefinedTab(DateOnly onDate, Guid value)
		{
			throw new NotImplementedException();
		}

		public IList<IUserDefinedTabLight> GetUserDefinedTabs()
		{
			throw new NotImplementedException();
		}

		public IList<IUserDefinedTabLight> GetOptionalColumnTabs()
		{
			throw new NotImplementedException();
		}
	}
}