using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAssignmentRepositoryLegacy : FakePersonAssignmentRepository
	{
		public FakePersonAssignmentRepositoryLegacy() : base(new FakeStorage())
		{
		}

		public FakePersonAssignmentRepositoryLegacy(IPersonAssignment assignment) : base(new FakeStorage())
		{
			Add(assignment);
		}
	}

	// feel free to continue implementing as see fit
	// im all for keeping it stupid (in the same manner as an .IgnoreArguments()) until smartness is required
	public class FakePersonAssignmentRepository : IPersonAssignmentRepository
	{
		private readonly FakeStorage _storage;

		public FakePersonAssignmentRepository(FakeStorage storage)
		{
			_storage = storage;
		}
		
		public void Has(IPersonAssignment personAssignment)
		{
			Add(personAssignment);
		}

		public void Has(IPerson agent, IScenario scenario, IActivity activity, IShiftCategory shiftCategory, DateOnlyPeriod period, TimePeriod timePeriod, string source=null)
		{
			foreach (var date in period.DayCollection())
			{
				var ass = new PersonAssignment(agent, scenario, date);
				ass.AddActivity(activity, timePeriod);
				ass.SetShiftCategory(shiftCategory);
				ass.Source = source;
				Add(ass);
			}
		}

		public void Has(IPerson agent, IScenario scenario, IActivity activity, IShiftCategory shiftCategory, DateOnly date, TimePeriod timePeriod, string source = null)
		{
			Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(date, date), timePeriod, source);
		}

		public void Has(IPerson agent, IScenario scenario, IDayOffTemplate dayOffTemplate, DateOnly date)
		{
			var ass = new PersonAssignment(agent, scenario, date);
			ass.SetDayOff(dayOffTemplate);
			Add(ass);
		}



		public void Add(IPersonAssignment entity)
		{
			_storage.Add(entity);
		}

		public void Remove(IPersonAssignment entity)
		{
			_storage.Remove(entity);
		}

		public IPersonAssignment Get(Guid id)
		{
			return _storage.Get<IPersonAssignment>(id);
		}

		public IList<IPersonAssignment> LoadAll()
		{
			return _storage.LoadAll<IPersonAssignment>().ToList();
		}

		public IPersonAssignment Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IPersonAssignment LoadAggregate(Guid id)
		{
			return _storage.LoadAll<IPersonAssignment>().First(x => x.Id == id);
		}

		public ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			return _storage.LoadAll<IPersonAssignment>().Where(ass => persons.Any(x => ass.Person.Equals(x)) && ass.BelongsToPeriod(period) && ass.Scenario.Equals(scenario)).ToList();
		}

		public ICollection<IPersonAssignment> Find(DateOnlyPeriod period, IScenario scenario)
		{
			return _storage.LoadAll<IPersonAssignment>().Where(ass => ass.BelongsToPeriod(period) && ass.Scenario.Equals(scenario)).ToList();
		}

		public IEnumerable<DateScenarioPersonId> FetchDatabaseVersions(DateOnlyPeriod period, IScenario scenario, IPerson person)
		{
			throw new NotImplementedException();
		}

		public DateTime GetScheduleLoadedTime()
		{
			return DateTime.UtcNow;
		}

		public ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario, string source)
		{
			return Find(persons, period, scenario).Where(s => s.Source == source).ToList();
		}

		public IPersonAssignment GetSingle(DateOnly dateOnly)
		{
			return _storage.LoadAll<IPersonAssignment>().Single(pa => pa.Date == dateOnly);
		}

		public IPersonAssignment GetSingle(DateOnly dateOnly, IPerson agent)
		{
			return _storage.LoadAll<IPersonAssignment>().Single(pa => pa.Date == dateOnly && pa.Person.Equals(agent));
		}

		public void Clear()
		{
			foreach (var personAssignment in _storage.LoadAll<IPersonAssignment>())
			{
				_storage.Remove(personAssignment);
			}
		}
	}
}