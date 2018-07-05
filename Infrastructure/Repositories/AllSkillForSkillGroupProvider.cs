using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AllSkillForSkillGroupProvider : IAllSkillForSkillGroupProvider
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillTypeInfoProvider _skillTypeInfoProvider;

		public AllSkillForSkillGroupProvider(ISkillRepository skillRepository, ISkillTypeRepository skillTypeRepository,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider, ISkillTypeInfoProvider skillTypeInfoProvider)
		{
			_skillRepository = skillRepository;
			_skillTypeRepository = skillTypeRepository;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillTypeInfoProvider = skillTypeInfoProvider;
		}

		public IEnumerable<SkillInIntraday> AllExceptSubSkills()
		{
			_skillTypeRepository.LoadAll();
			var skills = _skillRepository.LoadAll();
			skills = skills.Where(x => !(x is IChildSkill));
			return map(skills);
		}

		//living with that for now
		private List<SkillInIntraday> map(IEnumerable<ISkill> skills)
		{
			return skills.Select(skill =>
				{
					var skillTypeInfo = _skillTypeInfoProvider.GetSkillTypeInfo(skill);
					var skillInIntraday = new SkillInIntraday
					{
						Id = skill.Id.Value,
						Name = skill.Name,
						IsDeleted = ((Skill)skill).IsDeleted,
						DoDisplayData = _supportedSkillsInIntradayProvider.CheckSupportedSkill(skill),
						SkillType = skill.SkillType.Description.Name,
						IsMultisiteSkill = skill is MultisiteSkill,
						ShowAbandonRate = skillTypeInfo.SupportsAbandonRate,
						ShowReforecastedAgents = skillTypeInfo.SupportsReforecastedAgents
					};
					return skillInIntraday;
				})
				.OrderBy(s => s.Name)
				.ToList();
		}
	}
}