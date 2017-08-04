using System.Linq;
using System.Web.Http;
using Castle.Core.Internal;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.RtaTool;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.All)]
	public class RtaToolController : ApiController
	{
		private readonly IRtaToolViewModelBuilder _viewModelBuilder;
		private readonly IRtaStateGroupRepository _stateGroups;
	
		public RtaToolController(IRtaToolViewModelBuilder viewModelBuilder, IRtaStateGroupRepository stateGroups)
		{
			_viewModelBuilder = viewModelBuilder;
			_stateGroups = stateGroups;
		}

		[UnitOfWork, ReadModelUnitOfWork, AnalyticsUnitOfWork,  HttpGet, Route("RtaTool/Agents/For")]
		public virtual IHttpActionResult GetAgents()
		{
			return Ok(_viewModelBuilder.Build());
		}

		[UnitOfWork, HttpGet, Route("RtaTool/PhoneStates/For")]
		public virtual IHttpActionResult GetPhoneStates()
		{
			var stateGroups = _stateGroups.LoadAllCompleteGraph().Where(x => !x.StateCollection.IsNullOrEmpty()).ToArray();
			
			var availableStateGroup = stateGroups.FirstOrDefault(x => x.Available);
			var defaultStateGroup = stateGroups.FirstOrDefault(x => x.DefaultStateGroup);
			var loggedOutStateGroup = stateGroups.FirstOrDefault(x => x.IsLogOutState);
			var result = new[] { availableStateGroup, defaultStateGroup, loggedOutStateGroup };
			var quasiRandom = stateGroups
				.Except(result)
				.OrderBy(x => x.Name).Take(3);
			
			return Ok(
				result
				.Where(x => x != null)
				.Concat(quasiRandom)
				.Select(x => new
				{
					Name = x.Name,
					Code = x.StateCollection.First().StateCode
				}));
		}
	}
}