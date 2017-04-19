using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class LoadSkillInIntradays : ILoadAllSkillInIntradays
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;

		public LoadSkillInIntradays(ICurrentUnitOfWork currentUnitOfWork, ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
		}

		public IEnumerable<SkillInIntraday> Skills()
		{
			var skillRepo = new SkillRepository(_currentUnitOfWork.Current());
			var skills = skillRepo.FindSkillsWithAtLeastOneQueueSource();

			return skills.Select(skill => new SkillInIntraday
				{
					Id = skill.Id.Value,
					Name = skill.Name,
					IsDeleted = ((Skill) skill).IsDeleted,
					DoDisplayData = _supportedSkillsInIntradayProvider.CheckSupportedSkill(skill),
					SkillType = skill.SkillType.Description.Name
			})
				.OrderBy(s => s.Name)
				.ToList();
		}
	}
}