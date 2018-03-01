using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Scheduling
{
	public class FillSchedulerStateHolderFromDatabaseWithScheduleReadRetries : FillSchedulerStateHolderFromDatabase
	{
		private const int maxNumberOfTries = 3;
		private readonly ILog log = LogManager.GetLogger(typeof(FillSchedulerStateHolderFromDatabaseWithScheduleReadRetries));
		
		public FillSchedulerStateHolderFromDatabaseWithScheduleReadRetries(PersonalSkillsProvider personalSkillsProvider, IScenarioRepository scenarioRepository, ISkillDayLoadHelper skillDayLoadHelper, IFindSchedulesForPersons findSchedulesForPersons, IRepositoryFactory repositoryFactory, IPersonRepository personRepository, ISkillRepository skillRepository, ICurrentUnitOfWork currentUnitOfWork, IUserTimeZone userTimeZone, ExternalStaffProvider externalStaffProvider) : base(personalSkillsProvider, scenarioRepository, skillDayLoadHelper, findSchedulesForPersons, repositoryFactory, personRepository, skillRepository, currentUnitOfWork, userTimeZone, externalStaffProvider)
		{
		}

		protected override void FillSchedules(ISchedulerStateHolder schedulerStateHolderTo, IScenario scenario, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			var numberOfTries = 0;
			while (true)
			{
				try
				{
					numberOfTries++;
					base.FillSchedules(schedulerStateHolderTo, scenario, agents, period);
					return;
				}
				catch (DataSourceException e)
				{
					if (numberOfTries < maxNumberOfTries)
					{
						log.Warn($"Failed to read schedule. Attempt {numberOfTries} - retrying... {e}");	
					}
					else
					{
						log.Warn($"Failed to read schedule. Attempt {numberOfTries} - giving up... {e}");
						throw;
					}
				}				
			}
		}
	}
}