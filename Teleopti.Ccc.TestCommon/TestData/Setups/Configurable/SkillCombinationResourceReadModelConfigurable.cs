using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SkillCombinationResourceReadModelConfigurable : IDataSetup
	{
		private readonly string _skill;
		private readonly DateTime _theDate;

		public SkillCombinationResourceReadModelConfigurable(string skill, DateTime theDate)
		{
			_skill = skill;
			_theDate = theDate;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillRepo = new SkillRepository(currentUnitOfWork);
			var theSkill = skillRepo.LoadAll().SingleOrDefault(x => x.Name == _skill);
			
			var currentBu = new FakeCurrentBusinessUnit();
			currentBu.FakeBusinessUnit(BusinessUnitFactory.CreateWithId(theSkill.BusinessUnit.Id.GetValueOrDefault()));
			var skillComboList = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = _theDate.Date.AddHours(8),
					EndDateTime = _theDate.Date.AddHours(8).AddMinutes(15),
					Resource = 4,
					SkillCombination = new HashSet<Guid> {theSkill.Id.GetValueOrDefault()}
				}
			};
			var updateStaffingDateSetting = new UpdateStaffingLevelReadModelStartDate();
			updateStaffingDateSetting.RememberStartDateTime(skillComboList.Min(x=>x.StartDateTime));
			var skillCombinationReadModel = new SkillCombinationResourceRepository(new MutableNow(_theDate.Date.AddHours(7)), currentUnitOfWork, currentBu, new FakeStardustJobFeedback(), updateStaffingDateSetting, new SkillCombinationResourcesWithoutBpoToggleOn( currentBu, currentUnitOfWork));
			skillCombinationReadModel.PersistSkillCombinationResource(_theDate.Date.AddHours(7),skillComboList);
			
		}
	}
}