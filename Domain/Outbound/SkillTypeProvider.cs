using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public class SkillTypeProvider : ISkillTypeProvider
	{
		private readonly ISkillTypeRepository _skillTypeRepository;

		public SkillTypeProvider(ISkillTypeRepository skillTypeRepository)
		{
			_skillTypeRepository = skillTypeRepository;
		}

		public ISkillType Outbound()
		{
			return _skillTypeRepository.FindAll().First(s => s.Description.Name == "SkillTypeOutbound");
		}

		public ISkillType InboundTelephony()
		{
			return _skillTypeRepository.FindAll().First(s => s.Description.Name == "SkillTypeInboundTelephony");
		}
	}
}