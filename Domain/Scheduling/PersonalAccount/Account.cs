﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.PersonalAccount
{
    /// <summary>
    /// Account
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-08-22
    /// </remarks>
    public abstract class Account : AggregateEntity, IAccount
    {
        private DateOnly _startDate;
        private TimeSpan _usedInScheduler;
        private TimeSpan? _usedOutsideScheduler;
        private TimeSpan _latestCalculatedBalance; //eg latest calculated used

        public virtual TimeSpan Extra { get; set; }
        public virtual TimeSpan Accrued { get; set; }
        public virtual TimeSpan BalanceIn { get; set; }
        public virtual TimeSpan BalanceOut { get; set; }

        private static readonly TimeSpan DefaultMaxPeriodLength = TimeSpan.FromDays(3600);

        protected Account(){}

        protected Account(DateOnly startDate)
        {
            _startDate = startDate;
        }

        //Should only be called from nHib when loading and saving
        public virtual TimeSpan LatestCalculatedBalance
        {
            get
            {
                if (_usedOutsideScheduler.HasValue)
                {
                    return _usedOutsideScheduler.Value + _usedInScheduler;
                }
                return _latestCalculatedBalance;
            }
            set { _latestCalculatedBalance = value; }
        }

        public virtual DateOnly StartDate
        {
            get { return _startDate; }
            set { _startDate = value;}
        }

        public virtual TimeSpan Remaining
        {
            get
            {
                return BalanceIn.Add(Accrued).Add(Extra).Subtract(LatestCalculatedBalance).Subtract(BalanceOut);
            }
        }

        public virtual bool IsExceeded
        {
            get
            {
                TimeSpan remaining = Remaining;
                return remaining < TimeSpan.Zero;
            }
        }

        public virtual void CalculateUsed(IScheduleRepository repository, IScenario scenario)
        {
            CalculateUsed(repository, scenario, new PersonAccountProjectionService(this, null));
        }

		private void CalculateUsed(IScheduleRepository repository, IScenario scenario, IPersonAccountProjectionService projectionServiceForPersonAccount)
		{
			IList<IScheduleDay> scheduleDays = projectionServiceForPersonAccount.CreateProjection(repository, scenario);
			_usedInScheduler = TimeSpan.Zero;
			_usedOutsideScheduler = null;
			TimeSpan result = Owner.Absence.Tracker.TrackForReset(Owner.Absence, scheduleDays);
			LatestCalculatedBalance = result;
		}

        public virtual DateOnlyPeriod Period()
        {
            return new DateOnlyPeriod(StartDate, endDate());
        }

        public virtual void Track(TimeSpan timeOrDaysFromLoadedSchedule)
        {
            _usedInScheduler = timeOrDaysFromLoadedSchedule;

            if (!_usedOutsideScheduler.HasValue)
            {
                _usedOutsideScheduler = TimeSpan.Zero;
                if(_latestCalculatedBalance > _usedInScheduler)
                    _usedOutsideScheduler = _latestCalculatedBalance - _usedInScheduler;
            }

        }

        public virtual IAccount FindEarliestPersonAccount()
        {
            //The list is sorted descending!
            return Owner.AccountCollection().LastOrDefault(o => o.StartDate < StartDate);
        }

        public virtual IAccount FindPreviousPersonAccount()
        {
            //The list is sorted descending!
            return Owner.AccountCollection().FirstOrDefault(o => o.StartDate < StartDate);
        }

        public virtual object Clone()
        {
            return NoneEntityClone();
        }

        private DateOnly endDate()
        {
            DateTime endDate;
            if (Owner == null)
            {
                endDate = maxEndDate();                
            }
            else
            {
                var accounts = new List<IAccount>(Owner.AccountCollection());
                var indexOfThis = accounts.IndexOf(this);
                if (indexOfThis < 1)
                {
                    endDate = maxEndDate();
                } 
                else
                {
                    var nextStartDate = accounts[indexOfThis - 1].StartDate;
                    if (nextStartDate == StartDate)
                    {
                        endDate = nextStartDate;
                    } else if (nextStartDate > StartDate)
                    {
                        endDate = nextStartDate.AddDays(-1);
                    } else
                    {
                        throw new NotImplementedException(
                            "Invalid StartDate. PersonAbsenceAccount.AccountCollection is probably not ordered correctly");
                    }
                }
            }
			if (Owner != null && Owner.Person.TerminalDate.HasValue)
			{
				var terminateDate = Owner.Person.TerminalDate.Value;
				if (terminateDate < endDate)
					endDate = terminateDate;
				if (terminateDate < StartDate)
					endDate = StartDate;
			}

            return new DateOnly(endDate);
        }

        private DateTime maxEndDate()
        {
            return StartDate.Date.Add(DefaultMaxPeriodLength);
        }

        public virtual IPersonAbsenceAccount Owner
        {
            get { return (IPersonAbsenceAccount) Parent; }
        }

        public virtual IAccount NoneEntityClone()
        {
            var retobj = (IAccount)MemberwiseClone();
            retobj.SetId(null);

            return retobj;
        }

        public virtual IAccount EntityClone()
        {
            return (IAccount)MemberwiseClone();
        }
    }
}