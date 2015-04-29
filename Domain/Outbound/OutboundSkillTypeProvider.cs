

using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{

	public class OutboundSkillTypeProvider : IOutboundSkillTypeProvider
	{
		private readonly ISkillTypeRepository _skillTypeRepository;

		public OutboundSkillTypeProvider(ISkillTypeRepository skillTypeRepository)
		{
			_skillTypeRepository = skillTypeRepository;
		}

		public ISkillType OutboundSkillType()
		{
			return _skillTypeRepository.FindAll().First(s => s.Description.Name == "SkillTypeOutbound");
		}
	}
}