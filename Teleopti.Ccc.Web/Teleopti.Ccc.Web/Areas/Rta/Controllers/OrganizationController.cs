using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Monitor;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class OrganizationController : ApiController
	{
		private readonly OrganizationViewModelBuilder _organizationViewModelBuilder;

		public OrganizationController(OrganizationViewModelBuilder organizationViewModelBuilder)
		{
			_organizationViewModelBuilder = organizationViewModelBuilder;
		}
		
		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("api/Sites/Organization")]
		public virtual IHttpActionResult GetOrganization()
		{
			return Ok(_organizationViewModelBuilder.Build());
		}

		[UnitOfWork, ReadModelUnitOfWork, HttpGet, Route("api/Sites/OrganizationForSkills")]
		public virtual IHttpActionResult GetOrganizationForSkills([FromUri]Guid[] skillIds)
		{
			return Ok(_organizationViewModelBuilder.BuildForSkills(skillIds));
		}
		
	}
}