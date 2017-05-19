using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IWorkShiftFinderResultHolder
    {
        void AddResults(IList<WorkShiftFinderResult> results, DateTime theDateTime);
        IList<WorkShiftFinderResult> GetResults();
        bool LastResultIsSuccessful { get; }
        IList<WorkShiftFinderResult> GetResults(bool latestOnly);
        IList<WorkShiftFinderResult> GetResults(bool latestOnly, bool notSuccessfulOnly);
        void AddFilterToResult(IPerson person, DateOnly theDate, string message);
        void AddFilterToResult(IPerson person, DateOnly theDate, string message, int shiftsBefore, int shiftsAfter);
        void SetResultSuccessful(IPerson person, DateOnly theDate, string newMessage);
        void Clear();
        void Clear(IPerson person, DateOnly theDate);
		bool AlwaysShowTroubleshoot { get; set; }
    }

    public class WorkShiftFinderResultHolder : IWorkShiftFinderResultHolder
    {
		private readonly HashSet<WorkShiftFinderResult> _finderResultList;
        private DateTime _lastFinderResultDate;

        public WorkShiftFinderResultHolder()
        {
            _finderResultList = new HashSet<WorkShiftFinderResult>();
        }

        public void Clear()
        {
            _finderResultList.Clear();
	        AlwaysShowTroubleshoot = false;
        }

        public void Clear(IPerson person, DateOnly theDate)
        {
            var result = getResultForPersonAndDate(person, theDate);
            _finderResultList.Remove(result);
        }

	    public bool AlwaysShowTroubleshoot { get; set; }

	    public void AddResults(IList<WorkShiftFinderResult> results, DateTime theDateTime)
        {
            foreach (WorkShiftFinderResult result in results)
            {
                result.SchedulingDateTime = theDateTime;
            	_finderResultList.Add(result);
            }
			
            //_finderResultList.AddRange(results);
            if (results.Count > 0 )
                _lastFinderResultDate = theDateTime;
        }

        public IList<WorkShiftFinderResult> GetResults()
        {
            return GetResults(false, false);
        }

        public bool LastResultIsSuccessful => GetResults(true, true).Count == 0;

	    public IList<WorkShiftFinderResult> GetResults(bool latestOnly)
        {
            return GetResults(latestOnly, false);
        }

        public IList<WorkShiftFinderResult> GetResults(bool latestOnly, bool notSuccessfulOnly)
        {
            if (latestOnly & !notSuccessfulOnly)
                return filterOnLatest();

            if (!latestOnly & notSuccessfulOnly)
                return filterOnSuccess();

            if (latestOnly & notSuccessfulOnly)
                return filterOnSuccessAndLatest();

            return _finderResultList.ToList();
        }

        private IList<WorkShiftFinderResult> filterOnLatest()
        {
            return (from sd in _finderResultList where sd.SchedulingDateTime == _lastFinderResultDate select sd).ToList();
        }

        private IList<WorkShiftFinderResult> filterOnSuccess()
        {
            return (from sd in _finderResultList where sd.Successful == false select sd).ToList();
        }

        private IList<WorkShiftFinderResult> filterOnSuccessAndLatest()
        {
            return (from sd in _finderResultList where sd.Successful == false & sd.SchedulingDateTime == _lastFinderResultDate select sd).ToList();
        }

		public void AddFilterToResult(IPerson person, DateOnly theDate, string message)
		{
			AddFilterToResult(person, theDate, message, 0, 0);
		}

		public void AddFilterToResult(IPerson person, DateOnly theDate, string message, int shiftsBefore, int shiftsAfter)
    	{
    		var result = getResultForPersonAndDate(person, theDate);

			result.AddFilterResults(new WorkShiftFilterResult(message, shiftsBefore, shiftsAfter));
    	}

		public void SetResultSuccessful(IPerson person, DateOnly theDate, string newMessage)
		{
			var result = getResultForPersonAndDate(person, theDate);
			result.Successful = true;
			if(!string.IsNullOrEmpty(newMessage))
				result.AddFilterResults(new WorkShiftFilterResult(newMessage, 0, 0));
		}

        private WorkShiftFinderResult getResultForPersonAndDate(IPerson person, DateOnly theDate)
		{
			WorkShiftFinderResult result = _finderResultList.FirstOrDefault(workShiftFinderResult => workShiftFinderResult.Person.Equals(person) && workShiftFinderResult.ScheduleDate.Equals(theDate));
			if (result == null)
			{
				result = new WorkShiftFinderResult(person, theDate);
				_finderResultList.Add(result);
			}
    		return result;
		}
    }
}
