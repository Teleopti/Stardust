
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class WorkShiftFinderResult : IWorkShiftFinderResult
    {

        private readonly IPerson _person;
        private readonly DateOnly _scheduleDate;
    	private readonly HashSet<IWorkShiftFilterResult> _filterResult;

        public WorkShiftFinderResult(IPerson person, DateOnly scheduleDate)
        {
            _person = person;
            _scheduleDate = scheduleDate;
			_filterResult = new HashSet<IWorkShiftFilterResult>();
        	//Successful = true;
        }

        protected WorkShiftFinderResult(){}

		public bool Successful { get; set; }
		public DateTime SchedulingDateTime { get; set; }
		public bool StoppedOnOverstaffing { get; set; }

    	public void AddFilterResults(IWorkShiftFilterResult filterResult)
		{
    		_filterResult.Add(filterResult);
    	}

    	public IPerson Person
        {
            get { return _person; }
        }
        public Tuple<Guid,DateOnly> PersonDateKey
        {
            get { return new Tuple<Guid, DateOnly>(Person.Id.GetValueOrDefault(), ScheduleDate); }
        }

        public string PersonName
        {
            get { return Person.Name.ToString(NameOrderOption.FirstNameLastName); }
        }

        public DateOnly ScheduleDate
        {
            get { return _scheduleDate; }
        }

        public ReadOnlyCollection<IWorkShiftFilterResult>  FilterResults
        {
            get
            {
				return new ReadOnlyCollection<IWorkShiftFilterResult>(_filterResult.ToList());
            }
        }

		public override int GetHashCode()
		{
			return PersonDateKey.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var ent = obj as IWorkShiftFinderResult;
			return ent != null && Equals(ent);
		}

		public virtual bool Equals(IWorkShiftFinderResult other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;

			return (GetHashCode() == other.GetHashCode());
		}

    }
}
