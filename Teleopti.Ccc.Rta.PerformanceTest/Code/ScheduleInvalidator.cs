using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class ScheduleInvalidator
	{
		private readonly DataCreator _data;
		private readonly IPersonRepository _persons;
		private readonly ICurrentScheduleReadModelPersister _currentSchedules;

		public ScheduleInvalidator(
			DataCreator data,
			IPersonRepository persons,
			ICurrentScheduleReadModelPersister currentSchedules)
		{
			_data = data;
			_persons = persons;
			_currentSchedules = currentSchedules;
		}

		[TestLog]
		public virtual void InvalidateSchedules(int scheduleChangesPercent)
		{
			var agents = (int) (_data.LogonsWorking().Count()*(scheduleChangesPercent/100f));
			var personIds = LoadPersonIds(agents);
			Invalidate(personIds);
		}

		[TestLog]
		[ReadModelUnitOfWork]
		protected virtual void Invalidate(IEnumerable<Guid> personIds)
		{
			personIds.ForEach(x =>
			{
				_currentSchedules.Invalidate(x);
			});
		}

		[TestLog]
		[UnitOfWork]
		protected virtual IEnumerable<Guid> LoadPersonIds(int count)
		{
			return _persons.LoadAll()
				.Take(count)
				.Select(x => x.Id.Value)
				.ToArray();
		}
	}
}