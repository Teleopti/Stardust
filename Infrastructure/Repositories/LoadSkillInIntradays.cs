using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class LoadSkillInIntradays : ILoadAllSkillInIntradays
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly ISkillTypeInfoProvider _skillTypeInfoProvider;

		public LoadSkillInIntradays(ICurrentUnitOfWork currentUnitOfWork,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider, ISkillTypeInfoProvider skillTypeInfoProvider)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_skillTypeInfoProvider = skillTypeInfoProvider;
		}

		public IEnumerable<SkillInIntraday> Skills()
		{
			var skillRepo = SkillRepository.DONT_USE_CTOR(_currentUnitOfWork.Current());
			var skills = skillRepo.FindSkillsWithAtLeastOneQueueSource();

			return map(skills);
		}

		
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