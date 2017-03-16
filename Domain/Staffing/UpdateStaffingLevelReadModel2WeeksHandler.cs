﻿using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class UpdateStaffingLevelReadModel2WeeksHandler : IHandleEvent<UpdateStaffingLevelReadModel2WeeksEvent>, IRunOnStardust
	{
		private readonly INow _now;
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;

		public UpdateStaffingLevelReadModel2WeeksHandler(INow now, CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory, 
			LoaderForResourceCalculation loaderForResourceCalculation, IResourceCalculation resourceCalculation, ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_now = now;
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
			_loaderForResourceCalculation = loaderForResourceCalculation;
			_resourceCalculation = resourceCalculation;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateStaffingLevelReadModel2WeeksEvent @event)
		{
			var updateThingy= new UpdateStaffingLevelReadModelOnlySkillCombinationResources(_now, _cascadingResourceCalculationContextFactory,
				_loaderForResourceCalculation,_resourceCalculation, _skillCombinationResourceRepository);
			updateThingy.Update(new DateTimePeriod(_now.UtcDateTime().AddDays(-1).AddHours(-1), _now.UtcDateTime().AddDays(@event.Days).AddHours(1)));
		}
	}
}
