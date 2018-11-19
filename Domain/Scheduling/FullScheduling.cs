using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullScheduling
	{
		private readonly SchedulingCommandHandler _schedulingCommandHandler;
		private readonly IPersonRepository _personRepository;
		private readonly SchedulingInformationProvider _schedulingInformationProvider;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		private readonly FullSchedulingResult _fullSchedulingResult;

		public FullScheduling(SchedulingCommandHandler schedulingCommandHandler, 
			IPersonRepository personRepository,
			SchedulingInformationProvider schedulingInformationProvider,
			ISchedulingOptionsProvider schedulingOptionsProvider, 
			FullSchedulingResult fullSchedulingResult)
		{
			_schedulingCommandHandler = schedulingCommandHandler;
			_personRepository = personRepository;
			_schedulingInformationProvider = schedulingInformationProvider;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_fullSchedulingResult = fullSchedulingResult;
		}

		//runDayOffOptimization here for test purposes
		public FullSchedulingResultModel DoSchedulingAndDO(Guid planningPeriodId, bool runDayOffOptimization = true)
		{			
			var schedulingInformation = _schedulingInformationProvider.GetInfoFromPlanningPeriod(planningPeriodId);
			var schedulingOptions = _schedulingOptionsProvider.Fetch(null);
			var agents = LoadAgents(schedulingInformation.Period, schedulingInformation.PersonIds).ToArray();
			_schedulingCommandHandler.Execute(new SchedulingCommand
			{
				Period = schedulingInformation.Period,
				FromWeb = true,
				ScheduleWithoutPreferencesForFailedAgents = true,
				AgentsToSchedule = agents,
				PlanningPeriodId = planningPeriodId,
				RunDayOffOptimization = runDayOffOptimization
			});
			return _fullSchedulingResult.Create(schedulingInformation.Period, agents, schedulingInformation.PlanningGroupId, schedulingOptions.UsePreferences);
		}

		[TestLog]
		[UnitOfWork]
		protected virtual IEnumerable<IPerson> LoadAgents(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			var allPeople = _personRepository.FindAllAgentsLight(period);
			return allPeople.Where(x => people.Contains(x.Id.Value));			
		}
	}
}