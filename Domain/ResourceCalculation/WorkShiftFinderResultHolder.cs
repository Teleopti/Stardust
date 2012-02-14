﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IWorkShiftFinderResultHolder
    {
        void AddResults(IList<IWorkShiftFinderResult> results, DateTime theDateTime);
        IList<IWorkShiftFinderResult> GetResults();
        bool LastResultIsSuccessful { get; }
        IList<IWorkShiftFinderResult> GetResults(bool latestOnly);
        IList<IWorkShiftFinderResult> GetResults(bool latestOnly, bool notSuccessfulOnly);
        void AddFilterToResult(IPerson person, DateOnly theDate, string message);
        void AddFilterToResult(IPerson person, DateOnly theDate, string message, string key);
        void AddFilterToResult(IPerson person, DateOnly theDate, string message, string key, int shiftsBefore, int shiftsAfter);
        void SetResultSuccessful(IPerson person, DateOnly theDate, string newMessage);
        void Clear();
        void Clear(IPerson person, DateOnly theDate);
    }

    public class WorkShiftFinderResultHolder : IWorkShiftFinderResultHolder
    {
		private readonly HashSet<IWorkShiftFinderResult> _finderResultList;
        private DateTime _lastFinderResultDate;

        public WorkShiftFinderResultHolder()
        {
            _finderResultList = new HashSet<IWorkShiftFinderResult>();
        }

        public void Clear()
        {
            _finderResultList.Clear();
        }

        public void Clear(IPerson person, DateOnly theDate)
        {
            IWorkShiftFinderResult result = GetResultForPersonAndDate(person, theDate);
            _finderResultList.Remove(result);
        }

        public void AddResults(IList<IWorkShiftFinderResult> results, DateTime theDateTime)
        {
            foreach (IWorkShiftFinderResult result in results)
            {
                result.SchedulingDateTime = theDateTime;
            	_finderResultList.Add(result);
            }
			
            //_finderResultList.AddRange(results);
            if (results.Count > 0 )
                _lastFinderResultDate = theDateTime;
        }

        public IList<IWorkShiftFinderResult> GetResults()
        {
            return GetResults(false, false);
        }

        public bool LastResultIsSuccessful
        {
            get { return GetResults(true, true).Count == 0; }
        }

        public IList<IWorkShiftFinderResult> GetResults(bool latestOnly)
        {
            return GetResults(latestOnly, false);
        }

        public IList<IWorkShiftFinderResult> GetResults(bool latestOnly, bool notSuccessfulOnly)
        {
            if (latestOnly & !notSuccessfulOnly)
                return FilterOnLatest();

            if (!latestOnly & notSuccessfulOnly)
                return FilterOnSuccess();

            if (latestOnly & notSuccessfulOnly)
                return FilterOnSuccessAndLatest();

            return _finderResultList.ToList();
        }

        private IList<IWorkShiftFinderResult> FilterOnLatest()
        {
            return (from sd in _finderResultList where sd.SchedulingDateTime == _lastFinderResultDate select sd).ToList();
        }

        private IList<IWorkShiftFinderResult> FilterOnSuccess()
        {
            return (from sd in _finderResultList where sd.Successful == false select sd).ToList();
        }

        private IList<IWorkShiftFinderResult> FilterOnSuccessAndLatest()
        {
            return (from sd in _finderResultList where sd.Successful == false & sd.SchedulingDateTime == _lastFinderResultDate select sd).ToList();
        }

		public void AddFilterToResult(IPerson person, DateOnly theDate, string message)
		{
			AddFilterToResult(person, theDate, message, Guid.NewGuid().ToString());
		}
		
		public void AddFilterToResult(IPerson person, DateOnly theDate, string message, string key)
		{
			AddFilterToResult(person, theDate, message, key, 0, 0);
		}

		public void AddFilterToResult(IPerson person, DateOnly theDate, string message, string key, int shiftsBefore, int shiftsAfter)
    	{
    		IWorkShiftFinderResult result = GetResultForPersonAndDate(person, theDate);

			result.AddFilterResults(new WorkShiftFilterResult(message, shiftsBefore, shiftsAfter, key));
    	}

		public void SetResultSuccessful(IPerson person, DateOnly theDate, string newMessage)
		{
			IWorkShiftFinderResult result = GetResultForPersonAndDate(person, theDate);
			result.Successful = true;
			if(!string.IsNullOrEmpty(newMessage))
				result.AddFilterResults(new WorkShiftFilterResult(newMessage, 0, 0));
		}

        private IWorkShiftFinderResult GetResultForPersonAndDate(IPerson person, DateOnly theDate)
		{
			IWorkShiftFinderResult result = _finderResultList.Where(workShiftFinderResult => workShiftFinderResult.Person.Equals(person) && workShiftFinderResult.ScheduleDate.Equals(theDate)).FirstOrDefault();
			if (result == null)
			{
				result = new WorkShiftFinderResult(person, theDate);
				_finderResultList.Add(result);
			}
    		return result;
		}
    }
}
