using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public abstract class IntradayControllerBase : ApiController
	{
		private readonly ISkillAreaRepository _skillAreaRepository;

		protected IntradayControllerBase(ISkillAreaRepository skillAreaRepository)
		{
			_skillAreaRepository = skillAreaRepository;
		}

		protected Guid[] GetSkillsFromSkillArea(Guid skillAreaId)
		{
			var skillArea = _skillAreaRepository.Get(skillAreaId);
			return skillArea?.Skills.Select(skill => skill.Id).ToArray() ?? new Guid[0];
		}
	}
}