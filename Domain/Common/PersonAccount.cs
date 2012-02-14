using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Nongeneric class for mapping PersonAccount[T] for Nhibernate
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-08-22
    /// </remarks>
    public abstract class PersonAccount : AggregateEntity, IPersonAccount, ICloneable
    {
        private DateOnly _startDate;
        private Description _trackingDescription;
        private IAbsence _trackingAbsence;
        private TimeSpan _accrued;
        private TimeSpan _extra;
        private TimeSpan _balanceIn;

        private static readonly TimeSpan DefaultMaxPeriodLength = TimeSpan.FromDays(3600);

        protected PersonAccount(){}

        protected PersonAccount(DateOnly startDate, IAbsence trackingAbsence)
        {
            _trackingAbsence = trackingAbsence;
            _startDate = startDate;
        }

        protected PersonAccount(DateOnly startDate, IAbsence trackingAbsence, TimeSpan balanceIn, TimeSpan accrued, TimeSpan extra)
        {
            _trackingAbsence = trackingAbsence;
            _startDate = startDate;
            _balanceIn = balanceIn;
            _accrued = accrued;
            _extra = extra;
        }

        public virtual TimeSpan LatestCalculatedBalance { get; set; }

        public virtual DateOnly StartDate
        {
            get { return _startDate; }
            set { _startDate = value;}
        }

        public virtual Description TrackingDescription
        {
            get { return _trackingDescription; }
            set { _trackingDescription = value; }
        }


        public virtual TimeSpan BalanceOut
        {
            get { return BalanceIn.Add(Accrued).Add(Extra).Subtract(LatestCalculatedBalance); }
        }


        public virtual IAbsence TrackingAbsence
        {
            get { return _trackingAbsence; }
            set { _trackingAbsence = value; }
        }

        public virtual bool IsExceeded
        {
            get { return (Accrued + Extra + BalanceIn) < LatestCalculatedBalance; }
        }

        public virtual void CalculateUsed(IScheduleRepository repository,ISchedule loadedSchedule, IScenario scenario)
        {
            CalculateUsed(repository, loadedSchedule, scenario, new PersonAccountProjectionService(this, loadedSchedule));
        }

        public virtual void CalculateUsed(IScheduleRepository repository, ISchedule loadedSchedule, IScenario scenario,IPersonAccountProjectionService projectionServiceForPersonAccount)
        {
            IVisualLayerCollection layers = projectionServiceForPersonAccount.CreateProjection(repository, scenario);
            TrackingAbsence.Tracker.Track(this, TrackingAbsence, layers);  
        }

        public virtual DateOnlyPeriod Period()
        {
            return new DateOnlyPeriod(StartDate,EndDate());
        }


        public abstract void Track(TimeSpan timeOrDays);
        public abstract void CalculateBalanceIn();


        #region ICloneable Members

        public virtual object Clone()
        {
            PersonAccount retobj = (PersonAccount)MemberwiseClone();
            retobj.SetId(null);

            return retobj;
        }

       
        #endregion

        /// <summary>
        /// Ends the StartDatTime from the next PersonAccount, if its the last it will 
        /// </summary>
        /// <returns></returns>
        private DateOnly EndDate()
        {
            IList<IPersonAccount> accounts = ParentPerson.PersonAccountCollection;

            IEnumerable<DateOnly> accountsWithSameAbsence = from b in accounts
                                                            where b.TrackingAbsence.Equals(TrackingAbsence)
                                                                  && b.StartDate > StartDate
                                                            orderby b.StartDate
                                                            select b.StartDate.AddDays(-1);

            return accountsWithSameAbsence.DefaultIfEmpty(StartDate.AddDays((int) DefaultMaxPeriodLength.TotalDays)).First();
        }

        protected virtual IPerson ParentPerson
        {
            get { return Parent as IPerson; }
        }

        public virtual TimeSpan Extra
        {
            get { return _extra; }
            set { _extra = value; }
        }

        
        public virtual TimeSpan Accrued
        {
            get { return _accrued; }
            set { _accrued = value; }
        }

        public virtual TimeSpan BalanceIn
        {
            get { return _balanceIn; }
            set { _balanceIn = value; }
        }

        public static IPersonAccount CreatePersonAccount(DateOnly startDate, IAbsence absence)
        {
            if (absence.Tracker != null)
            {
                if (absence.Tracker.Equals(Tracker.CreateTimeTracker()) || absence.Tracker.Equals(Tracker.CreateCompTracker()))
                    return new PersonAccountTime(startDate, absence);
            }
            return new PersonAccountDay(startDate, absence);
        }
    }  
}
