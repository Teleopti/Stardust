using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    public class Person : AggregateRoot, IPerson, IDeleteTag
    {
        private Name _name;
        private readonly IPermissionInformation _permissionInformation;
        private readonly IDictionary<DateOnly, IPersonPeriod> _personPeriodCollection;
        private readonly IDictionary<DateOnly, ISchedulePeriod> _personSchedulePeriodCollection;
        private string _email;
        private string _note;
        private string _employmentNumber;
        private DateOnly? _terminalDate;
        private readonly IPersonWriteProtectionInfo _personWriteProtection;
        private bool _builtIn;
        private bool _isDeleted;
        private static readonly DateOnly MaxDate = new DateOnly(DateTime.MaxValue);
        private IWorkflowControlSet _workflowControlSet;
        private DayOfWeek _firstDayOfWeek;
        private IWindowsAuthenticationInfo _windowsAuthenticationInfo;
        private IApplicationAuthenticationInfo _applicationAuthenticationInfo;
		private readonly IList<IOptionalColumnValue> _optionalColumnValueCollection = new List<IOptionalColumnValue>();

        public Person()
        {
            _permissionInformation = new PermissionInformation(this);
            _personPeriodCollection = new SortedList<DateOnly, IPersonPeriod>();
            _personSchedulePeriodCollection = new SortedList<DateOnly, ISchedulePeriod>();
            _email = string.Empty;
            _note = string.Empty;
            _employmentNumber = string.Empty;
            _terminalDate = null;
            _personWriteProtection = new PersonWriteProtectionInfo(this);
            _firstDayOfWeek = DayOfWeek.Monday; //1
        }

        /// <summary>
        /// Gets the person's team at the given time.
        /// </summary>
        /// <param name="theDate">The date.</param>
        /// <returns></returns>
        public virtual ITeam MyTeam(DateOnly theDate)
        {
            IPersonPeriod per = Period(theDate);
            if (per == null) return null;

            return per.Team;
        }

        /// <summary>
        /// Collects all the persons in the IBusinessUnitHierarchy entity.
        /// </summary>
        /// <value></value>
        /// <returns>All persons in the hierarchy.</returns>
        public virtual ReadOnlyCollection<IPerson> PersonsInHierarchy(IEnumerable<IPerson> candidates, DateOnlyPeriod period)
        {
            IList<IPerson> ret = new List<IPerson>();
            if (candidates.Contains(this))
                ret.Add(this);
            return new ReadOnlyCollection<IPerson>(ret);
        }

        /// <summary>
        /// Terminal date
        /// </summary>
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-04
        /// </remarks>
        public virtual DateOnly? TerminalDate
        {
            get { return _terminalDate; }
            set { _terminalDate = value; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        public virtual Name Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets the permission related information.
        /// </summary>
        /// <value>The permission information.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        public virtual IPermissionInformation PermissionInformation
        {
            get { return _permissionInformation; }
        }

        /// <summary>
        /// Gets or sets the person period collecion. Sorted, earliest first.
        /// </summary>
        /// <value>The person period collecion.</value>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        public virtual IList<IPersonPeriod> PersonPeriodCollection
        {
            get
            {
                
                return new ReadOnlyCollection<IPersonPeriod>(InternalPersonPeriodCollection.ToList());
            }
        }

        /// <summary>
        /// Gets the internal person period collection. Note! This is added to improve performance where person periods are needed within this class.
        /// </summary>
        /// <value>The internal person period collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-01-08
        /// </remarks>
        private IEnumerable<IPersonPeriod> InternalPersonPeriodCollection
        {
            get
            {
                var terminalDateOrMax = TerminalDate.GetValueOrDefault(MaxDate);
                return _personPeriodCollection.Values.Where(p => p.StartDate <= terminalDateOrMax);
            }
        }

        private IEnumerable<ISchedulePeriod> InternalSchedulePeriodCollection
        {
            get
            {
                //Todo! Shouldn't this one be dependent on the terminal date as well?
                //var terminalDateOrMax = TerminalDate.GetValueOrDefault(MaxDate);
                //return _personSchedulePeriodCollection.Values.Where(p => p.DateFrom <= terminalDateOrMax);
                return _personSchedulePeriodCollection.Values;
            }
        }

        /// <summary>
        /// Gets the person schedule period collection
        /// </summary>
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-07
        /// </remarks>
        public virtual IList<ISchedulePeriod> PersonSchedulePeriodCollection
        {

            get { return new ReadOnlyCollection<ISchedulePeriod>(InternalSchedulePeriodCollection.ToList()); }
        }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-29
        /// </remarks>
        public virtual string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-01-22
        /// </remarks>
        public virtual string Note
        {
            get { return _note; }
            set { _note = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this person is an agent.
        /// </summary>
        public virtual bool IsAgent(DateOnly theDate)
        {
            return (Period(theDate) != null);
        }

        public virtual bool BuiltIn
        {
            get { return _builtIn; }
            set { _builtIn = value;}
        }

        public virtual string EmploymentNumber
        {
            get { return _employmentNumber; }
            set { _employmentNumber = value; }
        }



        /// <summary>
        /// Adds the person period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        public virtual void AddPersonPeriod(IPersonPeriod period)
        {
            InParameter.NotNull("period", period);

            if (!_personPeriodCollection.ContainsKey(period.StartDate))
            {
                period.SetParent(this);
                _personPeriodCollection.Add(period.StartDate, period);
            }
        }

        public virtual bool IsOkToAddPersonPeriod(DateOnly dateOnly)
        {
            return !_personPeriodCollection.ContainsKey(dateOnly);
        }

        public virtual bool IsOkToAddSchedulePeriod(DateOnly dateOnly)
        {
            return !_personSchedulePeriodCollection.ContainsKey(dateOnly);
        }

        public virtual void RemoveSchedulePeriod(ISchedulePeriod period)
        {
            InParameter.NotNull("period", period);
            _personSchedulePeriodCollection.Remove(period.DateFrom);
        }

        public virtual void DeletePersonPeriod(IPersonPeriod period)
        {
            InParameter.NotNull("period", period);
            _personPeriodCollection.Remove(period.StartDate);
        }

        public virtual void AddSchedulePeriod(ISchedulePeriod period)
        {
            InParameter.NotNull("period", period);

            if (_personSchedulePeriodCollection.ContainsKey(period.DateFrom)) return;
            period.SetParent(this);
            _personSchedulePeriodCollection.Add(period.DateFrom, period);
        }


        public virtual IPersonPeriod Period(DateOnly dateOnly)
        {
            IPersonPeriod period = null;

            if (isTerminated(dateOnly))
                return period;

            foreach (IPersonPeriod personPeriod in InternalPersonPeriodCollection)
            {
                if (!(personPeriod.StartDate > dateOnly))
                    period = personPeriod;
                else
                    break;
            }
            return period;
        }

        public virtual IList<IPersonPeriod> PersonPeriods(DateOnlyPeriod datePeriod)
        {
            IList<IPersonPeriod> retList = new List<IPersonPeriod>();
            
            if (isTerminated(datePeriod.StartDate)) return retList;

            IPersonPeriod period = null;
            foreach (IPersonPeriod p in InternalPersonPeriodCollection.OrderBy(d => d.StartDate))
            {
                if (p.StartDate > datePeriod.EndDate) break;
                
                if (p.StartDate< datePeriod.StartDate)
                {
                    period = p;
                    continue;
                }

                if (p.StartDate == datePeriod.StartDate)
                {
                    period = null;
                    retList.Add(p);
                }

                if (p.StartDate>datePeriod.StartDate)
                {
                    retList.Add(p);
                }
            }

            if (period != null)
            {
                if (!retList.Contains(period))
                    retList.Insert(0, period);
            }

            return retList;
        }

        /// <summary>
        /// Removes all person periods.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-02
        /// </remarks>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-02
        /// </remarks>
        public virtual void RemoveAllPersonPeriods()
        {
            _personPeriodCollection.Clear();
        }



        /// <summary>
        /// Removes all schedule periods.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-03
        /// </remarks>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-03
        /// </remarks>
        public virtual void RemoveAllSchedulePeriods()
        {
            _personSchedulePeriodCollection.Clear();
        }


        /// <summary>
        /// Returns the person period for a specific date.
        /// If no period is found, null is returned.
        /// </summary>
        /// <param name="dateOnly"></param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-07
        /// </remarks>
        public virtual ISchedulePeriod SchedulePeriod(DateOnly dateOnly)
        {
            ISchedulePeriod period = null;

            if (isTerminated(dateOnly))
                return period;

            TimeSpan minVal = TimeSpan.MaxValue;

            //get list with periods where startdate is less than inparam date
            IList<ISchedulePeriod> periods = PersonSchedulePeriodCollection.Where(s => s.DateFrom <= dateOnly.Date
                || (s.DateFrom.Date == dateOnly.Date)).ToList();

            //find period
            foreach (ISchedulePeriod p in periods)
            {
                if ((p.DateFrom.Date == dateOnly.Date))
                {
                    // Latest period is startdate equal to given date.
                    return p;
                }
                //get diff between inpara and startdate
                TimeSpan diff = dateOnly.Date.Subtract(p.DateFrom);

                //check against smallest diff and check that inparam is greater than startdate
                if (diff < minVal && diff.TotalMinutes >= 0)
                {
                    minVal = diff;
                    period = p;
                }
            }

            return period;
        }

		public virtual DateOnly? SchedulePeriodStartDate(DateOnly requestDate)
		{
			ISchedulePeriod schedulePeriod = SchedulePeriod(requestDate);
	
			if (schedulePeriod == null)
			{
				IPersonPeriod personPeriod = Period(requestDate);
				if (personPeriod == null)
					return null;
				return personPeriod.StartDate;
			}
			return schedulePeriod.DateFrom;
		}

        /// <summary>
        /// Determines whether the specified date is terminated.
        /// </summary>
        /// <param name="dateOnly">The date.</param>
        /// <returns>
        /// 	<c>true</c> if the specified date is terminated; otherwise, <c>false</c>.
        /// </returns>
        private bool isTerminated(DateOnly dateOnly)
        {
            if (_terminalDate.HasValue)
            {
                if (dateOnly > _terminalDate)
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Gets the person schedule periods within a period
        /// </summary>
        /// <param name="timePeriod"></param>
        /// <returns></returns>
        public virtual IList<ISchedulePeriod> PersonSchedulePeriods(DateOnlyPeriod timePeriod)
        {
            IList<ISchedulePeriod> retList = new List<ISchedulePeriod>();
            
            if (isTerminated(timePeriod.StartDate))
                return retList;

            TimeSpan minVal = TimeSpan.MaxValue;

            //get list with periods where startdate is less than inparam end date
            var periods = PersonSchedulePeriodCollection.Where(s => s.DateFrom <= timePeriod.EndDate);
            ISchedulePeriod period = null;

            foreach (ISchedulePeriod p in periods)
            {
                //get all that are contained
                if (timePeriod.Contains(p.DateFrom))
                {
                    retList.Add(p);
                }

                //get the first period
                //get diff between inpara and startdate
               // TimeSpan diff = timePeriod.StartDateTime.Subtract(p.DateFrom);
				TimeSpan diff = timePeriod.StartDate.Date.Subtract(p.DateFrom.Date);

                //check against smallest diff and check that inparam is greater than startdate
                if (diff < minVal && diff.TotalMinutes >= 0)
                {
                    minVal = diff;
                    period = p;
                }
            }

            if (period != null)
            {
                if (!retList.Contains(period))
                    retList.Add(period);
            }

            return retList.OrderBy(s => s.DateFrom.Date).ToList();
        }

		public virtual IList<ISchedulePeriod> PhysicalSchedulePeriods(DateOnlyPeriod period)
		{
			IList<ISchedulePeriod> retList = new List<ISchedulePeriod>();

			foreach (var schedulePeriod in PersonSchedulePeriodCollection)
			{
				var from = schedulePeriod.DateFrom;
				var to = schedulePeriod.RealDateTo();
				var physicalPeriod = new DateOnlyPeriod(from, to);

				if(period.Intersection(physicalPeriod) != null)
					retList.Add(schedulePeriod);
			}

			return retList.OrderBy(s => s.DateFrom.Date).ToList();
		}

        /// <summary>
        /// Gets the next period
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public virtual IPersonPeriod NextPeriod(IPersonPeriod period)
        {
            return InternalPersonPeriodCollection.OrderBy(p => p.StartDate.Date).FirstOrDefault(p => p.StartDate > period.StartDate);
        }

        /// <summary>
        /// Gets the previous period
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public virtual IPersonPeriod PreviousPeriod(IPersonPeriod period)
        {
            return InternalPersonPeriodCollection.OrderByDescending(p => p.StartDate.Date).FirstOrDefault(p => p.StartDate < period.StartDate);
        }

        /// <summary>=
        /// Gets the next schedule period
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public virtual ISchedulePeriod NextSchedulePeriod(ISchedulePeriod period)
        {
            ISchedulePeriod ret = null;

            DateTime nextDate = DateTime.MaxValue;

            foreach (ISchedulePeriod p in PersonSchedulePeriodCollection)
            {
                if (p.DateFrom > period.DateFrom)
                {
                    if (p.DateFrom.Date < nextDate)
                    {
                        nextDate = p.DateFrom.Date;
                        ret = p;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Gets the person rotation day restriction.
        /// </summary>
        /// <param name="personRestrictions">The person restrictions.</param>
        /// <param name="currentDate">The date.</param>
        /// <returns></returns>
        /// ///
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-09-11
        /// /// </remarks>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-11    
        /// /// </remarks>
        public virtual IList<IRotationRestriction> GetPersonRotationDayRestrictions(IEnumerable<IPersonRotation> personRestrictions, DateOnly currentDate)
        {
            // filter on person
            IEnumerable<IPersonRotation> filtered = personRestrictions.Where(r => r.Person.Equals(this));
            // order on startdate, newest first
            IOrderedEnumerable<IPersonRotation> sorted = filtered.OrderByDescending(n2 => n2.StartDate);

            foreach (var rotation in sorted)
            {
                if (rotation.StartDate <= currentDate)
                {
                    IRotationDay ret = rotation.GetRotationDay(currentDate);
                    if(ret != null)
                        return ret.RestrictionCollection;
                }

            }
            return new List<IRotationRestriction>();
        }

    	public virtual IVirtualSchedulePeriod VirtualSchedulePeriodOrNext(DateOnly dateOnly)
    	{
    		var dateToUse = dateOnly;
			if (!PersonSchedulePeriodCollection.IsEmpty())
			{
				var startOfFirstPeriod = PersonSchedulePeriodCollection[0].DateFrom;
				if (startOfFirstPeriod > dateToUse)
				{
					dateToUse = startOfFirstPeriod;
				}
			}
    		return VirtualSchedulePeriod(dateToUse);
    	}

    	public virtual bool ChangePassword(string newPassword, ILoadPasswordPolicyService loadPasswordPolicyService, IUserDetail userDetail)
        {
            if (_applicationAuthenticationInfo == null)
                _applicationAuthenticationInfo = new ApplicationAuthenticationInfo();
            IPasswordPolicy policy = new PasswordPolicy(loadPasswordPolicyService);
            if (policy.CheckPasswordStrength(newPassword))
            {
                _applicationAuthenticationInfo.Password = newPassword;
                userDetail.RegisterPasswordChange();
                return true;
            }
            return false;
        }

        public virtual bool ChangePassword(string oldPassword, string newPassword, ILoadPasswordPolicyService loadPasswordPolicyService, IUserDetail userDetail)
        {
            if (_applicationAuthenticationInfo == null)
                _applicationAuthenticationInfo = new ApplicationAuthenticationInfo();

            if (!checkOldPassword(oldPassword, newPassword, _applicationAuthenticationInfo)) return false;

            IPasswordPolicy policy = new PasswordPolicy(loadPasswordPolicyService);
            if (policy.CheckPasswordStrength(newPassword))
            {
                _applicationAuthenticationInfo.Password = newPassword;
                userDetail.RegisterPasswordChange();
                return true;
            }
            return false;
        }

        public virtual IWindowsAuthenticationInfo WindowsAuthenticationInfo
        {
            get
            {
                return _windowsAuthenticationInfo;
            }
            set { _windowsAuthenticationInfo = value; }
        }

        public virtual IApplicationAuthenticationInfo ApplicationAuthenticationInfo
        {
            get
            {
                return _applicationAuthenticationInfo;
            }
            set
            {
                _applicationAuthenticationInfo = value;
            }
        }

        private bool checkOldPassword(string oldPassword, string newPassword, IApplicationAuthenticationInfo authenticationInfo)
        {
            var encryption = new OneWayEncryption();
            if (encryption.EncryptString(oldPassword) != authenticationInfo.Password || encryption.EncryptString(newPassword) == authenticationInfo.Password) return false;
            return true;
        }

        public virtual IVirtualSchedulePeriod VirtualSchedulePeriod(DateOnly dateOnly)
        {
            var splitChecker = new VirtualSchedulePeriodSplitChecker(this);
            return new VirtualSchedulePeriod(this, dateOnly, splitChecker);
        }

        /// <summary>
        /// Gets the person availability day restriction.
        /// </summary>
        /// <param name="personRestrictions">The person restrictions.</param>
        /// <param name="currentDate">The current date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Ola
        /// Created date: 2008-10-20
        /// </remarks>
        public virtual IAvailabilityRestriction GetPersonAvailabilityDayRestriction(IEnumerable<IPersonAvailability> personRestrictions, DateOnly currentDate)
        {
            // filter on person
            IEnumerable<IPersonAvailability> filtered = personRestrictions.Where(r => r.Person.Equals(this));
            // order on startdate, newest first
            IOrderedEnumerable<IPersonAvailability> sorted = filtered.OrderByDescending(n2 => n2.StartDate);

            foreach (var availability in sorted)
            {
                if (availability.StartDate <= currentDate)
                {
                    return availability.GetAvailabilityDay(currentDate).Restriction;
                }

            }
            return null;
        }

        public virtual int Seniority
        {
            get
            {
                int days = 0;

				var today = DateOnly.Today;
                foreach (IPersonPeriod personPeriod in InternalPersonPeriodCollection)
                {
                    DateOnlyPeriod period = personPeriod.Period;

                    if (period.StartDate <= today)
                    {
                        if (period.EndDate >= today)
                            period = new DateOnlyPeriod(period.StartDate, today);

                        days += (int)period.EndDate.Date.Subtract(period.StartDate.Date).TotalDays;
                    }
                }

                return days / 30;
            }
        }

        public virtual IPersonWriteProtectionInfo PersonWriteProtection
        {
            get
            {
                return _personWriteProtection;
            }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual IWorkflowControlSet WorkflowControlSet
        {
            get
            {
                var workflowControlSet = _workflowControlSet as IDeleteTag;
                if (workflowControlSet != null && workflowControlSet.IsDeleted)
                    return null;
                return _workflowControlSet;
            }
            set {
                _workflowControlSet = value;
            }
        }

        public virtual DayOfWeek FirstDayOfWeek
        {
            get { return _firstDayOfWeek; }
            set { _firstDayOfWeek = value; }
        }

        public virtual void SetDeleted()
        {
            _windowsAuthenticationInfo = null;
            _applicationAuthenticationInfo = null;
           
            _isDeleted = true;
        }

		public virtual ReadOnlyCollection<IOptionalColumnValue> OptionalColumnValueCollection
		{
			get
			{
				return new ReadOnlyCollection<IOptionalColumnValue>(_optionalColumnValueCollection);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public virtual void AddOptionalColumnValue(IOptionalColumnValue value, IOptionalColumn column)
		{
			InParameter.NotNull("value", value);
			InParameter.NotNull("column", column);

			var colValue = GetColumnValue(column);
			if (colValue == null)
			{
				value.SetParent(column);
				value.ReferenceObject = this;
				_optionalColumnValueCollection.Add(value);
			}
			else
			{
				colValue.Description = value.Description;
			}
		}

		public virtual void RemoveOptionalColumnValue(IOptionalColumnValue value)
		{
			InParameter.NotNull("value", value);

			_optionalColumnValueCollection.Remove(value);
		}

		public virtual IOptionalColumnValue GetColumnValue(IOptionalColumn column)
		{
			IOptionalColumnValue result = _optionalColumnValueCollection.FirstOrDefault(v => v.Parent.Equals(column));
			return result;
		}

        public TimeSpan WorkTime(DateOnly dateOnly)
        {
            var personPeriod = Period(dateOnly);
            if(personPeriod == null) return TimeSpan.Zero;
            var contract = personPeriod.PersonContract.Contract;
            if (contract.IsWorkTimeFromContract)
                return contract.WorkTime.AvgWorkTimePerDay;
            if (contract.IsWorkTimeFromSchedulePeriod)
                return SchedulePeriod(dateOnly).AverageWorkTimePerDay;
            return TimeSpan.Zero;
        }
    }
}
