using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class LoaderForResourceCalculation 
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentTeleoptiPrincipal _principal;
		private readonly IPersonRepository _personRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IDisableDeletedFilter _disableDeletedFilter;
		private IScenario _scenario;
		private ICollection<IPerson> _agents;

		public LoaderForResourceCalculation(IScenarioRepository scenarioRepository,
					ISkillDayLoadHelper skillDayLoadHelper,
					IScheduleStorage scheduleStorage,
					ICurrentTeleoptiPrincipal principal,
					IPersonRepository personRepository,
					IAbsenceRepository absenceRepository,
					IActivityRepository activityRepository,
					IShiftCategoryRepository shiftCategoryRepository,
					ISkillRepository skillRepository,
					IDisableDeletedFilter disableDeletedFilter, ISkillTypeRepository skillTypeRepository)
		{
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_scheduleStorage = scheduleStorage;
			_principal = principal;
			_personRepository = personRepository;
			_absenceRepository = absenceRepository;
			_activityRepository = activityRepository;
			_shiftCategoryRepository = shiftCategoryRepository;
			_skillRepository = skillRepository;
			_disableDeletedFilter = disableDeletedFilter;
			_skillTypeRepository = skillTypeRepository;
		}

		protected IScenario FetchScenario()
		{
			return _scenarioRepository.LoadDefaultScenario();
		}

		public ResourceCalculationData ResourceCalculationData(DateOnlyPeriod period, bool doIntraIntervalCalculation = true)
		{
			var skills = skillsToUse(_agents, period).ToArray();
			var skillDays = SkillDays(period, skills);
			var schedules = Schedules(period);
			return new ResourceCalculationData(schedules,skillDays,true, doIntraIntervalCalculation);
		}

		[TestLog]
		protected virtual IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays(DateOnlyPeriod period, ICollection<ISkill> skills  )
		{
			return _skillDayLoadHelper.LoadSchedulerSkillDays(period, skills, _scenario);
		}
			
		[TestLog]
		protected virtual IScheduleDictionary Schedules( DateOnlyPeriod period)
		{
			var timeZone = _principal.Current().Regional.TimeZone;
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);
			return _scheduleStorage.FindSchedulesForPersons(_scenario,
				_agents, new ScheduleDictionaryLoadOptions(false, false, true) {LoadAgentDayScheduleTags = false}, dateTimePeriod, _agents, false);
		}

		public void PreFillInformation(DateOnlyPeriod period)
		{
			PreFill(period);
		}

		[TestLog]
		protected virtual void PreFill(DateOnlyPeriod period)
		{
			_scenario = _scenarioRepository.LoadDefaultScenario();
			
			using (_disableDeletedFilter.Disable())
			{
				_activityRepository.LoadAll();
				_skillTypeRepository.LoadAll();
				_skillRepository.LoadAllSkills();
				
				_absenceRepository.LoadAll();
				_shiftCategoryRepository.LoadAll();
			}	
			_skillRepository.FindAllWithSkillDays(period);
			_agents = _personRepository.FindAllAgentsQuiteLight(period);
		}

		private static IEnumerable<ISkill> skillsToUse(IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var agentSkills = new HashSet<ISkill>();
			foreach (var agent in agents)
			{
				foreach (var personPeriod in agent.PersonPeriods(period))
				{
					foreach (var personSkill in personPeriod.PersonSkillCollection)
					{
						if (personSkill.Active)
						{
							if (personSkill.Skill is IChildSkill childSkill && !agentSkills.Any(x => x.Equals(childSkill.ParentSkill)))
							{
								agentSkills.Add(childSkill.ParentSkill);
							}
							agentSkills.Add(personSkill.Skill);
						}
					}

				}
			}

			return agentSkills;
		}
	}
	
}