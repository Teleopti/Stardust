using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IExportToScenarioAccountPersister
	{
		bool Persist(IScenario exportScenario,
			IUnitOfWorkFactory uowFactory,
			IEnumerable<IPerson> persons,
			IDictionary<IPerson,
			IPersonAccountCollection> allPersonAccounts,
			Dictionary<IPerson, HashSet<IAbsence>> involvedAbsences,
			ICollection<DateOnly> datesToExport);
	}

	public class ExportToScenarioAccountPersister : IExportToScenarioAccountPersister
	{
		private readonly IPersonAccountPersister _personAccountPersister;

		public ExportToScenarioAccountPersister(IPersonAccountPersister personAccountPersister)
		{
			_personAccountPersister = personAccountPersister;
		}

		public bool Persist(IScenario exportScenario, 
			IUnitOfWorkFactory uowFactory, 
			IEnumerable<IPerson> persons, 
			IDictionary<IPerson, 
			IPersonAccountCollection> allPersonAccounts,
			Dictionary<IPerson, HashSet<IAbsence>> involvedAbsences,
			ICollection<DateOnly> datesToExport)
		{
			if (!exportScenario.DefaultScenario) return false;

			var persisted = false;

			using (var uow = uowFactory.CreateAndOpenUnitOfWork())
			{
				var repositoryFactory = new RepositoryFactory();
				var currentUnitOfWork = new ThisUnitOfWork(uow);
				var service = new TraceableRefreshService(new ThisCurrentScenario(exportScenario), new ScheduleStorage(currentUnitOfWork, repositoryFactory, new PersistableScheduleDataPermissionChecker(), new ScheduleStorageRepositoryWrapper(repositoryFactory, currentUnitOfWork)));
				var refreshedPersonAbsenceAccounts = new List<IPersonAbsenceAccount>();
				
				foreach (var person in persons)
				{
					if (!allPersonAccounts.ContainsKey(person)) continue;
					if (!involvedAbsences.ContainsKey(person)) continue;
					var personAbsences = involvedAbsences[person];

					var personAccountCollection = allPersonAccounts[person];
					var accountsWithinSelection = datesToExport.SelectMany(dateOnly => personAccountCollection.Find(dateOnly)).Distinct().ToList();

					foreach (var account in accountsWithinSelection)
					{
						var personAbsenceAccount = account.Parent as IPersonAbsenceAccount;
						if (personAbsenceAccount == null) continue;
						if (!personAbsences.Contains(personAbsenceAccount.Absence)) continue;

						service.Refresh(account);

						refreshedPersonAbsenceAccounts.Add(personAbsenceAccount);
					}
				}

				if (refreshedPersonAbsenceAccounts.Count > 0)
				{
					persisted = _personAccountPersister.Persist(refreshedPersonAbsenceAccounts);
				}
			}

			return persisted;
		}	
	}
}