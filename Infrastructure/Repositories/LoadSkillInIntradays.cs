using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class LoadSkillInIntradays : ILoadAllSkillInIntradays
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public LoadSkillInIntradays(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
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
					DoDisplayData = checkDisplay(skill)
				})
				.OrderBy(s => s.Name)
				.ToList();
		}

		private bool checkDisplay(ISkill skill)
		{
			var isMultisiteSkill = skill.GetType() == typeof(MultisiteSkill);

			return !isMultisiteSkill && skill.SkillType.Description.Name.Equals("SkillTypeInboundTelephony", StringComparison.InvariantCulture);
		}
	}
}