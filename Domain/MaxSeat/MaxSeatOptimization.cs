using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MaxSeat
{
	//bara testeri, testara just nu...
	public class MaxSeatOptimization
	{
		private readonly MaxSeatSkillCreator _maxSeatSkillCreator;
		private readonly ResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IScheduleStorage _scheduleStorage;

		public MaxSeatOptimization(MaxSeatSkillCreator maxSeatSkillCreator, 
														ResourceCalculationContextFactory resourceCalculationContextFactory,
														IScheduleStorage scheduleStorage)
		{
			_maxSeatSkillCreator = maxSeatSkillCreator;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_scheduleStorage = scheduleStorage;
		}

		public void Optimize(DateOnlyPeriod period, IEnumerable<Person> persons, IScenario scenario)
		{
			/*
			 * Mr Klagge - i mitt huvud nåt sånt här...
			 * 1. Vi genererar maxseatskills på samma sätt som schedulingscreen gjort förrut till att börja med. 
			 * 2. skapar res-beräkningscontext enbart med maxseatskills (andra skills kan vi skita i enligt anders för nu)
			 * 3. resursberäkna rubbet
			 * 3. Störst behov -> högst maxseat (tror jag) -> lägg ut skift enligt detta
			 * 4. Goto 3 tills... Nåt.
			 */
			var skills = Enumerable.Empty<ISkill>();
			var generatedMaxSeatSkills = _maxSeatSkillCreator.CreateMaxSeatSkills(period, scenario, persons.ToArray(), skills);
			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(persons, new ScheduleDictionaryLoadOptions(false, false, false), period, scenario);
			using (_resourceCalculationContextFactory.Create(schedules, generatedMaxSeatSkills.SkillsToAddToStateholder))
			{
				//res beräkna
				//kolla största behov (=högst maxseat)
				//byt skift
				//repeat
			}

		}
	}
}