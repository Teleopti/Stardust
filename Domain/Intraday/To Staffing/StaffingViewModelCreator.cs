using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.Intraday.To_Staffing
{
	public class StaffingViewModelCreator
	{
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		public StaffingViewModelCreator(ISkillCombinationResourceRepository skillCombinationResourceRepository, ISkillForecastReadModelRepository skillForecastReadModelRepository)
		{
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}

		public ScheduledStaffingViewModel Load(Guid[] skillIdList, DateOnly? dateInLocalTime = null,
			bool useShrinkage = false)
		{
			return new ScheduledStaffingViewModel();
		}
	}
}
