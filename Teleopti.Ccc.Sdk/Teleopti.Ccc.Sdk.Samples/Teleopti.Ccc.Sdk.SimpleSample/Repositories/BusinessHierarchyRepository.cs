using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.SimpleSample.Model;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class BusinessHierarchyRepository
    {
    	private readonly ITeleoptiOrganizationService _teleoptiOrganizationService;
    	private IList<BusinessHierarchyModel> _siteCollection;
    	private BusinessUnitDto _businessUnit;

    	public BusinessHierarchyRepository(ITeleoptiOrganizationService teleoptiOrganizationService)
    	{
    		_teleoptiOrganizationService = teleoptiOrganizationService;
    	}

    	public void Initialize()
        {
            var sites =
                _teleoptiOrganizationService.GetSitesOnBusinessUnit(new BusinessUnitDto { Id = AuthenticationMessageHeader.BusinessUnit });
            _siteCollection = sites.Select(s =>
                                               {
                                                   var container = new BusinessHierarchyModel { Site = new SiteModel(s) };
                                                   var teams = _teleoptiOrganizationService.GetTeamsOnSite(s);
                                                   foreach (var teamDto in teams)
                                                   {
                                                       container.TeamCollection.Add(teamDto);
                                                   }
                                                   return container;
                                               }).ToList();
    		_businessUnit = _teleoptiOrganizationService.GetBusinessUnitsByQuery(new GetCurrentBusinessUnitQueryDto()).First();
        }

        public IEnumerable<BusinessHierarchyModel> AllSites()
        {
            return _siteCollection;
        }

        public BusinessHierarchyModel GetSiteByTeam(Guid id)
        {
            var site =
                _siteCollection.FirstOrDefault(
                    o => o.TeamCollection.Any(t => t.Id == id));

            return site ?? new BusinessHierarchyModel { Site = new SiteModel(null) };
        }

		public BusinessUnitDto GetBusinessUnit()
		{
			return _businessUnit;
		}
    }
}