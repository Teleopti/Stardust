using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    public class Person : VersionedAggregateRoot, IPerson, IDeleteTag, IAggregateRootWithEvents
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
        private IApplicationAuthenticationInfo _applicationAuthenticationInfo;
		private readonly IList<IOptionalColumnValue> _optionalColumnValueCollection = new List<IOptionalColumnValue>();
		private IAuthenticationInfo _authenticationInfo;
	    private ISet<IAgentBadge> _badges = new HashSet<IAgentBadge>();

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

        public virtual ITeam MyTeam(DateOnly theDate)
        {
            IPersonPeriod per = Period(theDate);
            return per == null ? null : per.Team;
        }

        public virtual ReadOnlyCollection<IPerson> PersonsInHierarchy(IEnumerable<IPerson> candidates, DateOnlyPeriod period)
        {
            IList<IPerson> ret = new List<IPerson>();
            if (candidates.Contains(this))
                ret.Add(this);
            return new ReadOnlyCollection<IPerson>(ret);
        }

		public virtual void ActivatePerson(IPersonAccountUpdater personAccountUpdater)
	    {
		    TerminalDate = null;
            personAccountUpdater.Update(this);
	    }

		public virtual void TerminatePerson(DateOnly terminalDate, IPersonAccountUpdater personAccountUpdater)
	    {
		    TerminalDate = terminalDate;
			personAccountUpdater.Update(this);
	    }

	    public virtual DateOnly? TerminalDate
        {
            get { return _terminalDate; }
            protected set
            {
	            if (_terminalDate != value)
	            {
					
		            var valueBefore = _terminalDate.HasValue ? _terminalDate.Value.Date : (DateTime?) null;
		            var personPeriodsBefore = gatherPersonPeriodDetails();
		            _terminalDate = value;
					var valueAfter = _terminalDate.HasValue ? _terminalDate.Value.Date : (DateTime?)null;

					if (valueBefore.HasValue && !valueAfter.HasValue)
					{
						AddEvent(new PersonReactivatedEvent
							{
								PersonId = Id.GetValueOrDefault(),
								PersonPeriodsBefore = personPeriodsBefore,
								PersonPeriodsAfter = gatherPersonPeriodDetails(),
								PreviousTerminationDate = valueBefore.Value
							});
					}
					else
					{
						AddEvent(new PersonTerminatedEvent
							{
								PersonId = Id.GetValueOrDefault(),
								PersonPeriodsBefore = personPeriodsBefore,
								PersonPeriodsAfter = gatherPersonPeriodDetails(),
								PreviousTerminationDate = valueBefore,
								TerminationDate = valueAfter.Value
							});
					}
	            }
            }
        }

		public virtual void ChangeTeam(ITeam team, IPersonPeriod personPeriod)
		{
			Guid? teamBefore = null;
			Guid? teamAfter = null;

			if (personPeriod.Team != null)
			{
				teamBefore = personPeriod.Team.Id;
			}
			if (team != null)
			{
				teamAfter = team.Id;
			}
			personPeriod.Team = team;
			AddEvent(new PersonTeamChangedEvent
				{
					PersonId = Id.GetValueOrDefault(),
					StartDate = personPeriod.StartDate,
					OldTeam = teamBefore,
					NewTeam = teamAfter
				});
		}

		public virtual void AddSkill(IPersonSkill personSkill, IPersonPeriod personPeriod)
		{
			InParameter.NotNull("personSkill", personSkill);
			InParameter.NotNull("personPeriod", personPeriod);

			var skillsBefore = gatherSkillDetails(personPeriod);

			var modify = personPeriod.PersonSkillCollection.FirstOrDefault(s => s.Skill.Equals(personSkill.Skill)) as IPersonSkillModify;
			if (modify == null)
			{
				((IPersonPeriodModifySkills)personPeriod).AddPersonSkill(personSkill);
			}
			else
			{
				modify.Active = personSkill.Active;
				modify.SkillPercentage = personSkill.SkillPercentage;
			}
			AddEvent(new PersonSkillAddedEvent
				{
					PersonId = Id.GetValueOrDefault(),
					SkillId = personSkill.Skill.Id.GetValueOrDefault(),
					StartDate = personPeriod.StartDate,
					EndDate = personPeriod.EndDate(),
					Proficiency = personSkill.SkillPercentage.Value,
					SkillActive = personSkill.Active,
					SkillsBefore = skillsBefore
				});
		}

		public virtual void ResetPersonSkills(IPersonPeriod personPeriod)
		{
			InParameter.NotNull("personPeriod", personPeriod);

			var skillsBefore = gatherSkillDetails(personPeriod);

			var modify = personPeriod as IPersonPeriodModifySkills;
			if (modify != null)
			{
				modify.ResetPersonSkill();
			}
			AddEvent(new PersonSkillResetEvent
			{
				PersonId = Id.GetValueOrDefault(),
				StartDate = personPeriod.StartDate,
				SkillsBefore = skillsBefore
			});
		}

		public virtual void AddExternalLogOn(IExternalLogOn externalLogOn, IPersonPeriod personPeriod)
		{
			var modify = personPeriod as IPersonPeriodModifyExternalLogon;
			if (modify == null) return;
			modify.AddExternalLogOn(externalLogOn);
			addPersonActivityStartingEvent();
		}

		public virtual void ResetExternalLogOn(IPersonPeriod personPeriod)
	   {
			var modify = personPeriod as IPersonPeriodModifyExternalLogon;
			if (modify == null) return;
			modify.ResetExternalLogOn();
			addPersonActivityStartingEvent();
	   }

	   public virtual void RemoveExternalLogOn(IExternalLogOn externalLogOn, IPersonPeriod personPeriod)
	   {
			var modify = personPeriod as IPersonPeriodModifyExternalLogon;
		   if (modify == null) return;
		   modify.RemoveExternalLogOn(externalLogOn);
		   addPersonActivityStartingEvent();
	   }

	    public virtual void AddBadge(IAgentBadge agentBadge, int silverToBronzeBadgeRate, int goldToSilverBadgeRate)
	    {
		    if (_badges.Any(x => x.BadgeType == agentBadge.BadgeType))
		    {
			    var existedBadge = _badges.Single(x => x.BadgeType == agentBadge.BadgeType);
			    existedBadge.BronzeBadge += agentBadge.BronzeBadge;
			    existedBadge.SilverBadge += agentBadge.SilverBadge;
			    existedBadge.GoldBadge += agentBadge.GoldBadge;
			    existedBadge.LastCalculatedDate = agentBadge.LastCalculatedDate;
		    }
		    else
		    {
			    agentBadge.SetParent(this);
			    _badges.Add(agentBadge);
		    }
		    //Update according to rate.
		    var badge = _badges.Single(x => x.BadgeType == agentBadge.BadgeType);
		    if (badge.BronzeBadge >= silverToBronzeBadgeRate)
		    {
			    badge.SilverBadge = badge.SilverBadge + badge.BronzeBadge/silverToBronzeBadgeRate;
			    badge.BronzeBadge = badge.BronzeBadge%silverToBronzeBadgeRate;
		    }

		    if (badge.SilverBadge >= goldToSilverBadgeRate)
		    {
			    badge.GoldBadge = badge.GoldBadge + badge.SilverBadge/goldToSilverBadgeRate;
			    badge.SilverBadge = badge.SilverBadge%goldToSilverBadgeRate;
		    }
	    }

	    // adding this event so servicebus and rta do a check if person should be monitored in rta
		private void addPersonActivityStartingEvent()
		{
			AddEvent(new PersonActivityStarting
			{
				PersonId = Id.GetValueOrDefault()
			});
		}

	    public virtual void ActivateSkill(ISkill skill, IPersonPeriod personPeriod)
		{
			InParameter.NotNull("skill", skill);
			InParameter.NotNull("personPeriod", personPeriod);

			var skillsBefore = gatherSkillDetails(personPeriod);

			var personSkill = personPeriod.PersonSkillCollection.FirstOrDefault(s => skill.Equals(s.Skill)) as IPersonSkillModify;
			if (personSkill == null) return;
			if (personSkill.Active) return;

			personSkill.Active = true;

			AddEvent(new PersonSkillActivatedEvent
				{
					PersonId = Id.GetValueOrDefault(),
					SkillId = skill.Id.GetValueOrDefault(),
					StartDate = personPeriod.StartDate,
					EndDate = personPeriod.EndDate(),
					SkillsBefore = skillsBefore
				});
		}

		public virtual void DeactivateSkill(ISkill skill, IPersonPeriod personPeriod)
		{
			InParameter.NotNull("skill", skill);
			InParameter.NotNull("personPeriod", personPeriod);

			var skillsBefore = gatherSkillDetails(personPeriod);

			var personSkill = personPeriod.PersonSkillCollection.FirstOrDefault(s => skill.Equals(s.Skill)) as IPersonSkillModify;
			if (personSkill == null) return;

			personSkill.Active = false;

			AddEvent(new PersonSkillDeactivatedEvent
				{
					PersonId = Id.GetValueOrDefault(),
					SkillId = skill.Id.GetValueOrDefault(),
					StartDate = personPeriod.StartDate,
					EndDate = personPeriod.EndDate(),
					SkillsBefore = skillsBefore
				});
		}

		public virtual void RemoveSkill(ISkill skill, IPersonPeriod personPeriod)
		{
			InParameter.NotNull("skill",skill);
			InParameter.NotNull("personPeriod",personPeriod);
			var personSkill = personPeriod.PersonSkillCollection.FirstOrDefault(s => skill.Equals(s.Skill));
			if (personSkill != null)
			{
				var skillsBefore = gatherSkillDetails(personPeriod);

				((IPersonPeriodModifySkills)personPeriod).DeletePersonSkill(personSkill);
				AddEvent(new PersonSkillRemovedEvent
					{
						PersonId = Id.GetValueOrDefault(),
						SkillId = skill.Id.GetValueOrDefault(),
						StartDate = personPeriod.StartDate,
						EndDate = personPeriod.EndDate(),
						Proficiency = personSkill.SkillPercentage.Value,
						SkillActive = personSkill.Active,
						SkillsBefore = skillsBefore
					});
			}
		}

		public virtual void ChangeSkillProficiency(ISkill skill, Percent proficiency, IPersonPeriod personPeriod)
		{
			InParameter.NotNull("skill", skill);
			InParameter.NotNull("personPeriod", personPeriod);
			IPersonSkillModify personSkill = (IPersonSkillModify)personPeriod.PersonSkillCollection.FirstOrDefault(s => skill.Equals(s.Skill));
			if (personSkill != null)
			{
				var skillsBefore = gatherSkillDetails(personPeriod);

				personSkill.SkillPercentage = proficiency;

				AddEvent(new PersonSkillProficiencyChangedEvent
					{
						PersonId = Id.GetValueOrDefault(),
						SkillId = skill.Id.GetValueOrDefault(),
						StartDate = personPeriod.StartDate,
						EndDate = personPeriod.EndDate(),
						SkillsBefore = skillsBefore,
						ProficiencyAfter = proficiency.Value
					});
			}
		}

        public virtual Name Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual IPermissionInformation PermissionInformation
        {
            get { return _permissionInformation; }
        }

        public virtual IList<IPersonPeriod> PersonPeriodCollection
        {
            get
            {
                return new ReadOnlyCollection<IPersonPeriod>(InternalPersonPeriodCollection.ToList());
            }
        }

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
                return _personSchedulePeriodCollection.Values;
            }
        }

        public virtual IList<ISchedulePeriod> PersonSchedulePeriodCollection
        {

            get { return new ReadOnlyCollection<ISchedulePeriod>(InternalSchedulePeriodCollection.ToList()); }
        }

        public virtual string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public virtual string Note
        {
            get { return _note; }
            set { _note = value; }
        }

        public virtual bool IsAgent(DateOnly theDate)
        {
	        return !isTerminated(theDate) && _personPeriodCollection.Keys.Any(k => k <= theDate);
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

        public virtual void AddPersonPeriod(IPersonPeriod period)
        {
            InParameter.NotNull("period", period);

            if (!_personPeriodCollection.ContainsKey(period.StartDate))
            {
	            var personPeriodsBefore = gatherPersonPeriodDetails();
                period.SetParent(this);
                _personPeriodCollection.Add(period.StartDate, period);

	            AddEvent(new PersonPeriodAddedEvent
		            {
			            PersonId = Id.GetValueOrDefault(),
			            StartDate = period.StartDate.Date,
			            PersonPeriodsBefore = personPeriodsBefore,
			            PersonPeriodsAfter = gatherPersonPeriodDetails()
		            });
            }
        }

		public virtual void ChangeSchedulePeriodStartDate(DateOnly startDate, ISchedulePeriod schedulePeriod)
		{
			InParameter.NotNull("schedulePeriod", schedulePeriod);

			var startDateBefore = schedulePeriod.DateFrom;
			_personSchedulePeriodCollection.Remove(startDateBefore);
			while (_personSchedulePeriodCollection.ContainsKey(startDate))
			{
				startDate = startDate.AddDays(1);
			}
			schedulePeriod.DateFrom = startDate;
			_personSchedulePeriodCollection.Add(startDate, schedulePeriod);
		}

        public virtual void RemoveSchedulePeriod(ISchedulePeriod period)
        {
            InParameter.NotNull("period", period);
            _personSchedulePeriodCollection.Remove(period.DateFrom);
        }

        public virtual void DeletePersonPeriod(IPersonPeriod period)
        {
            InParameter.NotNull("period", period);

	        var personPeriodsBefore = gatherPersonPeriodDetails();
            _personPeriodCollection.Remove(period.StartDate);

	        AddEvent(new PersonPeriodRemovedEvent
		        {
			        PersonId = Id.GetValueOrDefault(),
			        StartDate = period.StartDate.Date,
			        PersonPeriodsBefore = personPeriodsBefore,
			        PersonPeriodsAfter = gatherPersonPeriodDetails()
		        });
        }

		public virtual void ChangePersonPeriodStartDate(DateOnly startDate, IPersonPeriod personPeriod)
		{
			InParameter.NotNull("personPeriod", personPeriod);

			var startDateBefore = personPeriod.StartDate;
			var personPeriodsBefore = gatherPersonPeriodDetails();
			_personPeriodCollection.Remove(startDateBefore);
			while (_personPeriodCollection.ContainsKey(startDate))
			{
				startDate = startDate.AddDays(1);
			}
			personPeriod.StartDate = startDate;
			_personPeriodCollection.Add(startDate, personPeriod);
			
			AddEvent(new PersonPeriodStartDateChangedEvent
				{
					PersonId = Id.GetValueOrDefault(),
					NewStartDate = startDate.Date,
					OldStartDate = startDateBefore.Date,
					PersonPeriodsBefore = personPeriodsBefore,
					PersonPeriodsAfter = gatherPersonPeriodDetails()
				});
		}

		private ICollection<PersonPeriodDetail> gatherPersonPeriodDetails()
		{
			var personPeriods = InternalPersonPeriodCollection;
			if (personPeriods == null) return new List<PersonPeriodDetail>();
			return 
				personPeriods.Select(
					p =>
					new PersonPeriodDetail
						{
							StartDate = p.StartDate,
							EndDate = p.EndDate(),
							TeamId = p.Team.Id.GetValueOrDefault(),
							PersonSkillDetails = gatherSkillDetails(p)
						}).ToList();
		}

		private ICollection<PersonSkillDetail> gatherSkillDetails(IPersonPeriod personPeriod)
		{
			return personPeriod.PersonSkillCollection.Select(p => new PersonSkillDetail
				{
					Active = p.Active,
					Proficiency = p.SkillPercentage.Value,
					SkillId = p.Skill.Id.GetValueOrDefault()
				}).ToList();
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

        public virtual void RemoveAllPersonPeriods()
        {
            _personPeriodCollection.Clear();
        }

        public virtual void RemoveAllSchedulePeriods()
        {
            _personSchedulePeriodCollection.Clear();
        }

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

        private bool isTerminated(DateOnly dateOnly)
        {
	        if (!_terminalDate.HasValue) return false;

	        return dateOnly > _terminalDate;
        }

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

        public virtual IPersonPeriod NextPeriod(IPersonPeriod period)
        {
            return InternalPersonPeriodCollection.OrderBy(p => p.StartDate.Date).FirstOrDefault(p => p.StartDate > period.StartDate);
        }

        public virtual IPersonPeriod PreviousPeriod(IPersonPeriod period)
        {
            return InternalPersonPeriodCollection.OrderByDescending(p => p.StartDate.Date).FirstOrDefault(p => p.StartDate < period.StartDate);
        }

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

        public virtual IChangePasswordResultInfo ChangePassword(string oldPassword, string newPassword, ILoadPasswordPolicyService loadPasswordPolicyService, IUserDetail userDetail)
        {
            if (_applicationAuthenticationInfo == null)
                _applicationAuthenticationInfo = new ApplicationAuthenticationInfo();

			IPasswordPolicy policy = new PasswordPolicy(loadPasswordPolicyService);
			var checkBruteForce= new CheckBruteForce(policy);
			var encryption = new OneWayEncryption();
			if (CheckNewPassword(newPassword, encryption))
				return new ChangePasswordResultInfo
					{
						IsAuthenticationSuccessful = true,
						IsSuccessful = false
					};
			if (!CheckOldPassword(oldPassword, userDetail, encryption, checkBruteForce))
				return new ChangePasswordResultInfo
					{
						IsAuthenticationSuccessful = false,
						IsSuccessful = false
					};

			var changePassword = new ChangePasswordResultInfo
		        {
			        IsAuthenticationSuccessful = true
		        };

            if (policy.CheckPasswordStrength(newPassword))
            {
                _applicationAuthenticationInfo.Password = newPassword;
                userDetail.RegisterPasswordChange();
				changePassword.IsSuccessful = true;
				return changePassword;
            }
			changePassword.IsSuccessful = false;
			return changePassword;
        }

		private bool CheckOldPassword(string oldPassword, IUserDetail userDetail, OneWayEncryption encryption, CheckBruteForce checkBruteForce)
	    {
		    if (encryption.EncryptString(oldPassword) == _applicationAuthenticationInfo.Password)
		    {
				return true;
		    }
			checkBruteForce.Check(userDetail);
		    return false;
	    }

	    private bool CheckNewPassword(string newPassword, OneWayEncryption encryption)
	    {
		    return encryption.EncryptString(newPassword) == _applicationAuthenticationInfo.Password;
	    }

	    public virtual IAuthenticationInfo AuthenticationInfo
		{
			get
			{
				return _authenticationInfo;
			}
			set { _authenticationInfo = value; }
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

	    public virtual IVirtualSchedulePeriod VirtualSchedulePeriod(DateOnly dateOnly)
        {
            var splitChecker = new VirtualSchedulePeriodSplitChecker(this);
            return new VirtualSchedulePeriod(this, dateOnly, splitChecker);
        }

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
		    _applicationAuthenticationInfo = null;
			_authenticationInfo = null;
		    var personPeriodsBefore = gatherPersonPeriodDetails();
		    _isDeleted = true;

		    AddEvent(new PersonDeletedEvent
			    {
				    PersonId = Id.GetValueOrDefault(),
				    PersonPeriodsBefore = personPeriodsBefore
			    });
	    }

	    public virtual ReadOnlyCollection<IOptionalColumnValue> OptionalColumnValueCollection
		{
			get
			{
				return new ReadOnlyCollection<IOptionalColumnValue>(_optionalColumnValueCollection);
			}
		}

	    public virtual ISet<IAgentBadge> Badges
	    {
			get { return _badges; }
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

        public virtual TimeSpan AverageWorkTimeOfDay(DateOnly dateOnly)
        {
            var personPeriod = Period(dateOnly);
            if (personPeriod == null) return TimeSpan.Zero;
            var contract = personPeriod.PersonContract.Contract;
            switch (contract.WorkTimeSource)
            {
                case WorkTimeSource.FromContract:
                    return contract.WorkTime.AvgWorkTimePerDay;
                case WorkTimeSource.FromSchedulePeriod:
                    {
                        var schedulePeriod = VirtualSchedulePeriod(dateOnly);
                        return schedulePeriod == null
                                   ? WorkTime.DefaultWorkTime.AvgWorkTimePerDay
                                   : schedulePeriod.AverageWorkTimePerDay;
                    }
            }
            return TimeSpan.Zero;
        }

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ 431;
		}
    }
}
