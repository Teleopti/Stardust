using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.SimpleSample.Model;
using Teleopti.Ccc.Sdk.SimpleSample.ViewModel;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class BusinessHierarchyRepository
    {
        private IList<BusinessHierarchyModel> _siteCollection;

        public void Initialize(ITeleoptiOrganizationService teleoptiOrganizationService)
        {
            var sites =
                teleoptiOrganizationService.GetSitesOnBusinessUnit(new BusinessUnitDto { Id = AuthenticationMessageHeader.BusinessUnit });
            _siteCollection = sites.Select(s =>
                                               {
                                                   var container = new BusinessHierarchyModel { Site = new SiteModel(s) };
                                                   var teams = teleoptiOrganizationService.GetTeamsOnSite(s);
                                                   foreach (var teamDto in teams)
                                                   {
                                                       container.TeamCollection.Add(teamDto);
                                                   }
                                                   return container;
                                               }).ToList();
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
    }
}