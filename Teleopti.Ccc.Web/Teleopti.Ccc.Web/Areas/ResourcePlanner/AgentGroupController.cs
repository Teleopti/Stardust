using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class AgentGroupController : ApiController
	{
		private readonly IAgentGroupRepository _agentGroupRepository;
		private readonly IAgentGroupModelPersister _agentGroupModelPersister;

		public AgentGroupController(IAgentGroupRepository agentGroupRepository,IAgentGroupModelPersister agentGroupModelPersister)
		{
			_agentGroupRepository = agentGroupRepository;
			_agentGroupModelPersister = agentGroupModelPersister;
		}

		[UnitOfWork, HttpPost, Route("api/ResourcePlanner/AgentGroup")]
		public virtual object Create(AgentGroupModel model)
		{
			_agentGroupModelPersister.Persist(model);
			return new { model.Id };
		}

		[UnitOfWork, HttpGet, Route("api/ResourcePlanner/AgentGroup")]
		public virtual AgentGroupModel[] List()
		{
			var result = _agentGroupRepository.LoadAll();
			return result.Select(x => new AgentGroupModel
			{
				Id = x.Id.Value,
				Name = x.Name
			}).ToArray();
		}

		[UnitOfWork, HttpGet, Route("api/ResourcePlanner/AgentGroup/{id}")]
		public virtual AgentGroupModel Get(Guid id)
		{
			var result = _agentGroupRepository.Get(id);
			return new AgentGroupModel
			{
				Id = result.Id.Value,
				Name = result.Name
			};
		}
	}

}