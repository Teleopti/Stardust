using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class AgentGroupController : ApiController
	{
		private readonly IAgentGroupModelPersister _agentGroupModelPersister;
		private readonly IFetchAgentGroupModel _fetchAgentGroupModel;

		public AgentGroupController(IAgentGroupModelPersister agentGroupModelPersister, IFetchAgentGroupModel fetchAgentGroupModel)
		{
			_agentGroupModelPersister = agentGroupModelPersister;
			_fetchAgentGroupModel = fetchAgentGroupModel;
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/agentgroup")]
		public virtual IHttpActionResult Create(AgentGroupModel model)
		{
			_agentGroupModelPersister.Persist(model);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/agentgroup")]
		public virtual IHttpActionResult List()
		{
			return Ok(_fetchAgentGroupModel.FetchAll());
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/agentgroup/{id}")]
		public virtual IHttpActionResult Get(Guid id)
		{
			return Ok(_fetchAgentGroupModel.Fetch(id));
		}
	}

}