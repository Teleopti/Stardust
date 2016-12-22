using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ScheduleForecastSkillReadModelConfigurable : IDataSetup
	{
		private readonly string _skill;
		private readonly DateTime _theDate;

		public ScheduleForecastSkillReadModelConfigurable(string skill, DateTime theDate)
		{
			_skill = skill;
			_theDate = theDate;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillRepo = new SkillRepository(currentUnitOfWork);
			var theSkill = skillRepo.LoadAll().SingleOrDefault(x => x.Name == _skill);

			var repo = new ScheduleForecastSkillReadModelRepository(currentUnitOfWork, new MutableNow(DateTime.UtcNow));
			repo.Persist(new List<SkillStaffingInterval> {
				new SkillStaffingInterval
			{
				SkillId = theSkill.Id.Value,
				StartDateTime = _theDate.Date.AddHours(8),
				EndDateTime = _theDate.Date.AddHours(8).AddMinutes(15),
				Forecast = 5,
				StaffingLevel = 4
			} }, DateTime.Now );
		}
	}
}