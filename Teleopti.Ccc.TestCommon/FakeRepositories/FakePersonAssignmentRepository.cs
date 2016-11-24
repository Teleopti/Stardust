﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	// feel free to continue implementing as see fit
	// im all for keeping it stupid (in the same manner as an .IgnoreArguments()) until smartness is required
	public class FakePersonAssignmentRepository : IPersonAssignmentRepository
	{
		private readonly IList<IPersonAssignment> _personAssignments = new List<IPersonAssignment>();

		public FakePersonAssignmentRepository()
		{
			
		}

		public FakePersonAssignmentRepository(IPersonAssignment personAssignment)
		{
			_personAssignments.Add(personAssignment);
		}




		public void Has(IPersonAssignment personAssignment)
		{
			Add(personAssignment);
		}

		public void Has(IPerson agent, IScenario scenario, IActivity activity, IShiftCategory shiftCategory, DateOnlyPeriod period, TimePeriod timePeriod)
		{
			foreach (var date in period.DayCollection())
			{
				var ass = new PersonAssignment(agent, scenario, date);
				ass.AddActivity(activity, timePeriod);
				ass.SetShiftCategory(shiftCategory);
				Add(ass);
			}
		}

		public void Has(IPerson agent, IScenario scenario, IActivity activity, IShiftCategory shiftCategory, DateOnly date, TimePeriod timePeriod)
		{
			Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(date, date), timePeriod);
		}

		public void Has(IPerson agent, IScenario scenario, IDayOffTemplate dayOffTemplate, DateOnly date)
		{
			var ass = new PersonAssignment(agent, scenario, date);
			ass.SetDayOff(dayOffTemplate);
			Add(ass);
		}



		public void Add(IPersonAssignment entity)
		{
			_personAssignments.Add(entity);
		}

		public void Remove(IPersonAssignment entity)
		{
			_personAssignments.Remove(entity);
		}

		public IPersonAssignment Get(Guid id)
		{
			return _personAssignments.FirstOrDefault(x => x.Id == id);
		}

		public IList<IPersonAssignment> LoadAll()
		{
			return _personAssignments;
		}

		public IPersonAssignment Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public IPersonAssignment LoadAggregate(Guid id)
		{
			return _personAssignments.First(x => x.Id == id);
		}

		public IPersonAssignment LoadAggregate(PersonAssignmentKey id)
		{
			throw new NotImplementedException();
		}

		public ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			return _personAssignments.Where(ass => persons.Any(x => ass.Person.Equals(x)) && ass.BelongsToPeriod(period) && ass.Scenario.Equals(scenario)).ToList();
		}

		public ICollection<IPersonAssignment> Find(DateOnlyPeriod period, IScenario scenario)
		{
			return new Collection<IPersonAssignment>(_personAssignments);
		}

		public IEnumerable<DateScenarioPersonId> FetchDatabaseVersions(DateOnlyPeriod period, IScenario scenario, IPerson person)
		{
			throw new NotImplementedException();
		}

		public DateTime GetScheduleLoadedTime()
		{
			return DateTime.UtcNow;
		}
		
		public IPersonAssignment GetSingle(DateOnly dateOnly)
		{
			return _personAssignments.Single(pa => pa.Date == dateOnly);
		}

		public IPersonAssignment GetSingle(DateOnly dateOnly, IPerson agent)
		{
			return _personAssignments.Single(pa => pa.Date == dateOnly && pa.Person.Equals(agent));
		}

		public void Clear()
		{
			_personAssignments.Clear();
		}

	}
}