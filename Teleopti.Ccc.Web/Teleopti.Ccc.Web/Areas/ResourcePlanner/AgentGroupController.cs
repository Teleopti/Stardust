using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Web.Areas.ResourcePlanner.Models;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class AgentGroupController : ApiController
	{
		private readonly IAgentGroupRepository _agentGroupRepository;

		public AgentGroupController(IAgentGroupRepository agentGroupRepository)
		{
			_agentGroupRepository = agentGroupRepository;
		}

		[UnitOfWork, HttpPost, Route("api/ResourcePlanner/AgentGroup")]
		public virtual object Create(CreateAgentGroupModel model)
		{
			var agentGroup = new AgentGroup
			{
				Name = model.Name
			};
			_agentGroupRepository.Add(agentGroup);
			return new {agentGroup.Id };
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