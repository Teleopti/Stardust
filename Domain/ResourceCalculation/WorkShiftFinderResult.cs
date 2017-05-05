
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Holds the result of a try to find a workshift on a Person and Date
    /// </summary>
    /// ///
    /// <remarks>
    /// Created by: Ola
    /// Created date: 2008-09-23
    /// /// </remarks>
    public class WorkShiftFinderResult
    {
        private readonly IPerson _person;
	    private readonly HashSet<WorkShiftFilterResult> _filterResult;

        public WorkShiftFinderResult(IPerson person, DateOnly scheduleDate)
        {
            _person = person;
            ScheduleDate = scheduleDate;
			_filterResult = new HashSet<WorkShiftFilterResult>();

	        PersonDateKey = new Tuple<Guid, DateOnly>(Person.Id.GetValueOrDefault(), ScheduleDate);
        }
		
		public bool Successful { get; set; }
		public DateTime SchedulingDateTime { get; set; }
		public bool StoppedOnOverstaffing { get; set; }

    	public void AddFilterResults(WorkShiftFilterResult filterResult)
		{
    		_filterResult.Add(filterResult);
    	}

    	public IPerson Person => _person;

	    public Tuple<Guid,DateOnly> PersonDateKey { get; }

	    public string PersonName => Person.Name.ToString(NameOrderOption.FirstNameLastName);

	    public DateOnly ScheduleDate { get; }

	    public ReadOnlyCollection<WorkShiftFilterResult>  FilterResults => new ReadOnlyCollection<WorkShiftFilterResult>(_filterResult.ToList());

	    public override int GetHashCode()
		{
			return PersonDateKey.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var ent = obj as WorkShiftFinderResult;
			return ent != null && Equals(ent);
		}

		public virtual bool Equals(WorkShiftFinderResult other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;

			return PersonDateKey.Equals(other.PersonDateKey);
		}

    }
}
