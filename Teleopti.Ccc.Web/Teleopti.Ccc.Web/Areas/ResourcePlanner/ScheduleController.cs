using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly FullScheduling _fullScheduling;
		private readonly IAgentGroupRepository _agentGroupRepository;
		private readonly IAgentGroupStaffLoader _agentGroupStaffLoader;

		public ScheduleController(FullScheduling fullScheduling, IAgentGroupRepository agentGroupRepository, IAgentGroupStaffLoader agentGroupStaffLoader)
		{
			_fullScheduling = fullScheduling;
			_agentGroupRepository = agentGroupRepository;
			_agentGroupStaffLoader = agentGroupStaffLoader;
		}

		//remove me when we move scheduling/optimization out of http request
		[HttpPost, Route("api/ResourcePlanner/KeepAlive")]
		public virtual void KeepAlive()
		{
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/FixedStaff")]
		public virtual IHttpActionResult FixedStaff([FromBody] StaffSchedulingInput input)
		{
			var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));
			return Ok(_fullScheduling.DoScheduling(period));
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/AgentGroup")]
		public virtual IHttpActionResult AgentGroup([FromBody] AgentGroupStaffSchedulingInput input)
		{
			var agentGroup = _agentGroupRepository.Load(input.AgentGroupId);
			var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));
			var people = _agentGroupStaffLoader.Load(period, agentGroup);
			return Ok(_fullScheduling.DoScheduling(period, people.AllPeople.Select(x => x.Id.Value)));
		}
	}
}