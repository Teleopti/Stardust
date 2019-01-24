using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Domain.Common
{
	[DebuggerDisplay("{Name}, {Id.ToString().ToUpper()}")]
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
		private bool _isDeleted;
		private static readonly DateOnly MaxDate = new DateOnly(DateTime.MaxValue);
		private IWorkflowControlSet _workflowControlSet;
		private DayOfWeek _firstDayOfWeek;
		private readonly IList<IOptionalColumnValue> _optionalColumnValueCollection = new List<IOptionalColumnValue>();
		public static readonly DateOnly DefaultTerminalDate = new DateOnly(2059, 12, 31);

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

		public override IEnumerable<IEvent> PopAllEvents() =>
			base.PopAllEvents()
				.KeepLastOfType<PersonNameChangedEvent>()
				.KeepLastOfType<PersonPeriodChangedEvent>()
				.KeepLastOfType<OptionalColumnValueChangedEvent>()
				.KeepLastOfType<PersonTerminalDateChangedEvent>();

		public virtual ITeam MyTeam(DateOnly theDate)
		{
			var per = Period(theDate);
			return per?.Team;
		}

		public virtual void ActivatePerson(IPersonAccountUpdater personAccountUpdater)
		{
			terminate(null);
			personAccountUpdater.Update(this);
		}

		public virtual void TerminatePerson(DateOnly terminalDate, IPersonAccountUpdater personAccountUpdater, IPersonLeavingUpdater personLeavingUpdater = null)
		{
			terminate(terminalDate);
			(personLeavingUpdater ?? new DummyPersonLeavingUpdater()).Execute(terminalDate, this);
			personAccountUpdater.Update(this);
		}

		public virtual DateOnly? TerminalDate => _terminalDate;

		private void terminate(DateOnly? value)
		{
			DateOnly? valueToSet = null;
			if (value != null)
				valueToSet = value.Value > DefaultTerminalDate ? DefaultTerminalDate : value;
			if (_terminalDate == valueToSet) return;

			var valueBefore = _terminalDate?.Date;
			_terminalDate = valueToSet;
			var valueAfter = _terminalDate?.Date;

			AddEvent(() => new PersonTerminalDateChangedEvent
			{
				PersonId = Id.GetValueOrDefault(),
				PreviousTerminationDate = valueBefore,
				TerminationDate = valueAfter
			});
		}

		public virtual void ChangeTeam(ITeam team, IPersonPeriod personPeriod)
		{
			personPeriod.Team = team;
			AddEvent(() =>
			{
				var info = currentAssociationInfo(ServiceLocatorForEntity.Now);
				return new PersonTeamChangedEvent
				{
					PersonId = Id.GetValueOrDefault(),
					CurrentBusinessUnitId = info.BusinessUnitId,
					CurrentSiteId = info.SiteId,
					CurrentSiteName = info.SiteName,
					CurrentTeamId = info.TeamId,
					CurrentTeamName = info.TeamName,
					ExternalLogons = info.ExternalLogons
				};
			});
		}

		private personAssociationInfo currentAssociationInfo(INow now)
		{
			var nowInAgentTimeZone = now.CurrentLocalDate(PermissionInformation.DefaultTimeZone());
			var period = Period(nowInAgentTimeZone);
			var info = new personAssociationInfo();
			if (period?.Team != null)
			{
				info.TeamId = period.Team.Id.GetValueOrDefault();
				info.TeamName = period.Team.Description.Name;
				if (period.Team.Site != null)
				{
					info.SiteId = period.Team.Site.Id.GetValueOrDefault();
					info.SiteName = period.Team.Site.Description.Name;
					if (period.Team.Site.BusinessUnit != null)
					{
						info.BusinessUnitId = period.Team.Site.BusinessUnit.Id.GetValueOrDefault();
					}
				}
			}

			info.ExternalLogons =
			(
				period?.ExternalLogOnCollection
					.Select(x => new ExternalLogon
					{
						UserCode = x.AcdLogOnOriginalId,
						DataSourceId = x.DataSourceId
					}) ?? Enumerable.Empty<ExternalLogon>()
			).ToArray();
			return info;
		}

		private class personAssociationInfo
		{
			public Guid? BusinessUnitId { get; set; }
			public Guid? SiteId { get; set; }
			public string SiteName { get; set; }
			public Guid? TeamId { get; set; }
			public string TeamName { get; set; }
			public IEnumerable<ExternalLogon> ExternalLogons { get; set; }
		}

		public virtual void AddSkill(ISkill skill, DateOnly personPeriodDate)
		{
			AddSkill(new PersonSkill(skill, new Percent(1)), Period(personPeriodDate));
		}

		public virtual void AddSkill(ISkill skill, IPersonPeriod personPeriod)
		{
			AddSkill(new PersonSkill(skill, new Percent(1)), personPeriod);
		}

		public virtual void AddSkill(IPersonSkill personSkill, IPersonPeriod personPeriod)
		{
			InParameter.NotNull(nameof(personSkill), personSkill);
			InParameter.NotNull(nameof(personPeriod), personPeriod);

			if (!(personPeriod.PersonSkillCollection.FirstOrDefault(s => s.Skill.Equals(personSkill.Skill)) is IPersonSkillModify modify))
			{
				((IPersonPeriodModifySkills)personPeriod).AddPersonSkill(personSkill);
			}
			else
			{
				modify.Active = personSkill.Active;
				modify.SkillPercentage = personSkill.SkillPercentage;
			}
		}

		public virtual void ResetPersonSkills(IPersonPeriod personPeriod)
		{
			InParameter.NotNull(nameof(personPeriod), personPeriod);

			var modify = personPeriod as IPersonPeriodModifySkills;
			modify?.ResetPersonSkill();
		}

		public virtual void AddExternalLogOn(IExternalLogOn externalLogOn, IPersonPeriod personPeriod)
		{
			if (!(personPeriod is IPersonPeriodModifyExternalLogon modify)) return;
			modify.AddExternalLogOn(externalLogOn);

			addPersonPeriodChangedEvent();
		}

		public virtual void ResetExternalLogOn(IPersonPeriod personPeriod)
		{
			if (!(personPeriod is IPersonPeriodModifyExternalLogon modify)) return;
			modify.ResetExternalLogOn();

			addPersonPeriodChangedEvent();
		}

		public virtual void RemoveExternalLogOn(IExternalLogOn externalLogOn, IPersonPeriod personPeriod)
		{
			if (!(personPeriod is IPersonPeriodModifyExternalLogon modify)) return;
			modify.RemoveExternalLogOn(externalLogOn);

			addPersonPeriodChangedEvent();
		}

		public virtual bool IsTerminated(DateOnly? date = null)
		{
			date = date ?? DateOnly.Today;
			return TerminalDate.HasValue && TerminalDate.Value < date;
		}

		public virtual void ActivateSkill(ISkill skill, IPersonPeriod personPeriod)
		{
			InParameter.NotNull(nameof(skill), skill);
			InParameter.NotNull(nameof(personPeriod), personPeriod);

			if (!(personPeriod.PersonSkillCollection.FirstOrDefault(s => skill.Equals(s.Skill)) is IPersonSkillModify personSkill)) return;
			if (personSkill.Active) return;

			personSkill.Active = true;
		}

		public virtual void DeactivateSkill(ISkill skill, IPersonPeriod personPeriod)
		{
			InParameter.NotNull(nameof(skill), skill);
			InParameter.NotNull(nameof(personPeriod), personPeriod);

			if (!(personPeriod.PersonSkillCollection.FirstOrDefault(s => skill.Equals(s.Skill)) is IPersonSkillModify personSkill)) return;

			personSkill.Active = false;
		}

		public virtual void RemoveSkill(ISkill skill, IPersonPeriod personPeriod)
		{
			InParameter.NotNull(nameof(skill), skill);
			InParameter.NotNull(nameof(personPeriod), personPeriod);
			var personSkill = personPeriod.PersonSkillCollection.FirstOrDefault(s => skill.Equals(s.Skill));
			if (personSkill != null)
				((IPersonPeriodModifySkills)personPeriod).DeletePersonSkill(personSkill);
		}

		public virtual void ChangeSkillProficiency(ISkill skill, Percent proficiency, IPersonPeriod personPeriod)
		{
			InParameter.NotNull(nameof(skill), skill);
			InParameter.NotNull(nameof(personPeriod), personPeriod);
			IPersonSkillModify personSkill = (IPersonSkillModify)personPeriod.PersonSkillCollection.FirstOrDefault(s => skill.Equals(s.Skill));
			if (personSkill != null)
				personSkill.SkillPercentage = proficiency;
		}

		public virtual Name Name => _name;

		public virtual void SetName(Name value)
		{
			AddEvent(() => new PersonNameChangedEvent
			{
				PersonId = Id.GetValueOrDefault(),
				FirstName = value.FirstName,
				LastName = value.LastName
			});

			_name = value;
		}

		public virtual IPermissionInformation PermissionInformation => _permissionInformation;

		public virtual IList<IPersonPeriod> PersonPeriodCollection => new ReadOnlyCollection<IPersonPeriod>(InternalPersonPeriodCollection.ToList());

		private IEnumerable<IPersonPeriod> InternalPersonPeriodCollection
		{
			get
			{
				var terminalDateOrMax = TerminalDate.GetValueOrDefault(MaxDate);
				return _personPeriodCollection.Values.Where(p => p.StartDate <= terminalDateOrMax);
			}
		}

		private IEnumerable<ISchedulePeriod> InternalSchedulePeriodCollection => _personSchedulePeriodCollection.Values;

		public virtual IList<ISchedulePeriod> PersonSchedulePeriodCollection => new ReadOnlyCollection<ISchedulePeriod>(InternalSchedulePeriodCollection.ToList());

		public virtual string Email
		{
			get { return _email; }
			set { _email = value; }
		}

		public virtual string Note
		{
			get { return _note; }
			set { _note = value.Trim(); }
		}

		public virtual bool IsAgent(DateOnly theDate)
		{
			return !isTerminated(theDate) && _personPeriodCollection.Keys.Any(k => k <= theDate);
		}

		public virtual void SetEmploymentNumber(string value)
		{
			AddEvent(() => new PersonEmploymentNumberChangedEvent
			{
				PersonId = Id.GetValueOrDefault(),
				EmploymentNumber = _employmentNumber
			});

			_employmentNumber = value;
		}

		public virtual string EmploymentNumber => _employmentNumber;

		public virtual void AddSchedulePeriod(ISchedulePeriod period)
		{
			InParameter.NotNull(nameof(period), period);

			if (_personSchedulePeriodCollection.ContainsKey(period.DateFrom)) return;
			period.SetParent(this);
			_personSchedulePeriodCollection.Add(period.DateFrom, period);
		}

		public virtual void RemoveSchedulePeriod(ISchedulePeriod period)
		{
			InParameter.NotNull(nameof(period), period);
			_personSchedulePeriodCollection.Remove(period.DateFrom);
		}

		public virtual void ChangeSchedulePeriodStartDate(DateOnly startDate, ISchedulePeriod schedulePeriod)
		{
			InParameter.NotNull(nameof(schedulePeriod), schedulePeriod);

			var startDateBefore = schedulePeriod.DateFrom;
			_personSchedulePeriodCollection.Remove(startDateBefore);
			while (_personSchedulePeriodCollection.ContainsKey(startDate))
			{
				startDate = startDate.AddDays(1);
			}

			schedulePeriod.DateFrom = startDate;
			_personSchedulePeriodCollection.Add(startDate, schedulePeriod);
		}

		public virtual void RemoveAllSchedulePeriods()
		{
			_personSchedulePeriodCollection.Clear();
		}

		public virtual void AddPersonPeriod(IPersonPeriod period)
		{
			InParameter.NotNull(nameof(period), period);

			if (!_personPeriodCollection.ContainsKey(period.StartDate))
			{
				period.SetParent(this);
				_personPeriodCollection.Add(period.StartDate, period);
				addPersonPeriodChangedEvent();
			}
		}

		private void addPersonPeriodChangedEvent()
		{
			AddEvent(() =>
			{
				var info = currentAssociationInfo(ServiceLocatorForEntity.Now);
				return new PersonPeriodChangedEvent
				{
					PersonId = Id.GetValueOrDefault(),
					CurrentBusinessUnitId = info.BusinessUnitId,
					CurrentSiteId = info.SiteId,
					CurrentSiteName = info.SiteName,
					CurrentTeamId = info.TeamId,
					CurrentTeamName = info.TeamName,
					ExternalLogons = info.ExternalLogons
				};
			});
		}

		public virtual void DeletePersonPeriod(IPersonPeriod period)
		{
			InParameter.NotNull(nameof(period), period);

			var previousPeriod = PreviousPeriod(period);

			AddPersonEmploymentChangeEvent(new PersonEmploymentChangedEvent()
			{
				PersonId = Id.GetValueOrDefault(),
				FromDate = previousPeriod?.StartDate ?? period.StartDate
			});

			_personPeriodCollection.Remove(period.StartDate);
			addPersonPeriodChangedEvent();
		}

		public virtual void ChangePersonPeriodStartDate(DateOnly startDate, IPersonPeriod personPeriod)
		{
			InParameter.NotNull(nameof(personPeriod), personPeriod);

			var startDateBefore = personPeriod.StartDate;
			_personPeriodCollection.Remove(startDateBefore);
			while (_personPeriodCollection.ContainsKey(startDate))
			{
				startDate = startDate.AddDays(1);
			}

			personPeriod.StartDate = startDate;
			_personPeriodCollection.Add(startDate, personPeriod);

			addPersonPeriodChangedEvent();
		}

		public virtual void RemoveAllPeriodsAfter(DateOnly date)
		{
			var personPeriods = _personPeriodCollection.Keys.Where(d => date < d).ToArray();
			foreach (var personPeriod in personPeriods)
			{
				_personPeriodCollection.Remove(personPeriod);
			}

			var schedulePeriods = _personSchedulePeriodCollection.Keys.Where(d => date < d).ToArray();
			foreach (var schedulePeriod in schedulePeriods)
			{
				_personSchedulePeriodCollection.Remove(schedulePeriod);
			}

			addPersonPeriodChangedEvent();
		}

		public virtual bool IsExternalAgent => false;

		public virtual void RemoveAllPersonPeriods()
		{
			_personPeriodCollection.Clear();

			addPersonPeriodChangedEvent();
		}

		public virtual IPersonPeriod Period(DateOnly date)
		{
			IPersonPeriod period = null;

			if (isTerminated(date))
				return null;

			foreach (IPersonPeriod personPeriod in InternalPersonPeriodCollection)
			{
				if (!(personPeriod.StartDate > date))
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

				if (p.StartDate < datePeriod.StartDate)
				{
					period = p;
					continue;
				}

				if (p.StartDate == datePeriod.StartDate)
				{
					period = null;
					retList.Add(p);
				}

				if (p.StartDate > datePeriod.StartDate)
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

		public virtual ISchedulePeriod SchedulePeriod(DateOnly dateOnly)
		{
			ISchedulePeriod period = null;

			if (isTerminated(dateOnly))
				return null;

			//get list with periods where startdate is less than inparam date
			IList<ISchedulePeriod> periods = PersonSchedulePeriodCollection.Where(s => s.DateFrom <= dateOnly).ToArray();

			//find period
			foreach (ISchedulePeriod p in periods)
			{
				if ((p.DateFrom == dateOnly))
				{
					// Latest period is startdate equal to given date.
					return p;
				}

				if (period == null || period.DateFrom < p.DateFrom)
				{
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

			//get list with periods where startdate is less than inparam end date
			var periods = PersonSchedulePeriodCollection.Where(s => s.DateFrom <= timePeriod.EndDate);
			ISchedulePeriod period = null;
			TimeSpan minVal = TimeSpan.MaxValue;
			foreach (ISchedulePeriod p in periods)
			{
				if (timePeriod.Contains(p.DateFrom))
				{
					retList.Add(p);
				}

				//get diff between inpara and startdate
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

		public virtual IPersonPeriod NextPeriod(IPersonPeriod period)
		{
			return InternalPersonPeriodCollection.OrderBy(p => p.StartDate.Date).FirstOrDefault(p => p.StartDate > period.StartDate);
		}

		public virtual IPersonPeriod PreviousPeriod(IPersonPeriod period)
		{
			return InternalPersonPeriodCollection.OrderByDescending(p => p.StartDate.Date).FirstOrDefault(p => p.StartDate < period.StartDate);
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
					if (ret != null)
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

		public virtual IVirtualSchedulePeriod VirtualSchedulePeriod(DateOnly dateOnly)
		{
			return innerVirtualSchedulePeriod(Period(dateOnly), SchedulePeriod(dateOnly), dateOnly);
		}

		private IVirtualSchedulePeriod innerVirtualSchedulePeriod(IPersonPeriod personPeriod, ISchedulePeriod schedulePeriod, DateOnly date)
		{
			var splitChecker = new VirtualSchedulePeriodSplitChecker(this);
			return new VirtualSchedulePeriod(this, date, personPeriod, schedulePeriod, splitChecker);
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

						days += period.DayCount();
					}
				}

				return days / 30;
			}
		}

		public virtual IPersonWriteProtectionInfo PersonWriteProtection => _personWriteProtection;

		public virtual bool IsDeleted => _isDeleted;

		public virtual IWorkflowControlSet WorkflowControlSet
		{
			get
			{
				if (_workflowControlSet is IDeleteTag workflowControlSet && workflowControlSet.IsDeleted)
					return null;
				return _workflowControlSet;
			}
			set { _workflowControlSet = value; }
		}

		public virtual DayOfWeek FirstDayOfWeek
		{
			get { return _firstDayOfWeek; }
			set { _firstDayOfWeek = value; }
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
			AddEvent(new PersonDeletedEvent
			{
				PersonId = Id.GetValueOrDefault()
			});
		}

		public virtual ReadOnlyCollection<IOptionalColumnValue> OptionalColumnValueCollection => new ReadOnlyCollection<IOptionalColumnValue>(_optionalColumnValueCollection);

		public virtual void SetOptionalColumnValue(IOptionalColumnValue value, IOptionalColumn column)
		{
			InParameter.NotNull(nameof(value), value);
			InParameter.NotNull(nameof(column), column);

			var colValue = GetColumnValue(column);
			if (colValue == null)
			{
				value.SetParent(this);
				value.ReferenceObject = column;
				_optionalColumnValueCollection.Add(value);
			}
			else
			{
				colValue.Description = value.Description;
			}

			AddEvent(() => new OptionalColumnValueChangedEvent
			{
				PersonId = Id.GetValueOrDefault()
			});
		}

		public virtual void RemoveOptionalColumnValue(IOptionalColumnValue value)
		{
			InParameter.NotNull(nameof(value), value);

			_optionalColumnValueCollection.Remove(value);

			AddEvent(() => new OptionalColumnValueChangedEvent
			{
				PersonId = Id.GetValueOrDefault()
			});
		}

		public virtual IOptionalColumnValue GetColumnValue(IOptionalColumn column)
		{
			IOptionalColumnValue result = _optionalColumnValueCollection.FirstOrDefault(v => v.ReferenceObject.Equals(column));
			return result;
		}

		public virtual PersonWorkDay[] AverageWorkTimes(DateOnlyPeriod period)
		{
			var personPeriods = PersonPeriods(period).Select(p => new { Period = new DateOnlyPeriod(p.StartDate, p.EndDate()), p });
			var schedulePeriods = PersonSchedulePeriods(period).Select(p => new { Period = new DateOnlyPeriod(p.DateFrom, p.RealDateTo()), p });
			var days = period.DayCollection();

			return days.Select(
					d =>
						new
						{
							Day = d,
							PersonPeriod = personPeriods.FirstOrDefault(p => p.Period.Contains(d))
						})
				.Select(
					m =>
					{
						var schedulePeriod = schedulePeriods.FirstOrDefault(s => s.Period.Contains(m.Day));
						if (m.PersonPeriod == null) return new PersonWorkDay(m.Day);
						return new PersonWorkDay(m.Day,
							new Lazy<TimeSpan>(() => calculateAverageWorkTime(m.PersonPeriod.p, schedulePeriod?.p, m.Day)),
							m.PersonPeriod.p.PersonContract.Contract.WorkTimeSource,
							m.PersonPeriod.p.PersonContract.PartTimePercentage.Percentage,
							isWorkDay(m.PersonPeriod.p.PersonContract.ContractSchedule, m.Day));
					}).ToArray();
		}


		private bool isWorkDay(IContractSchedule contractSchedule, DateOnly day)
		{
			var schedulePeriodStartDate = SchedulePeriodStartDate(day);
			return contractSchedule != null && schedulePeriodStartDate.HasValue && contractSchedule.IsWorkday(schedulePeriodStartDate.Value, day, FirstDayOfWeek);
		}

		private TimeSpan calculateAverageWorkTime(IPersonPeriod personPeriod, ISchedulePeriod schedulePeriod, DateOnly day)
		{
			var averageWorkTimePerDay = TimeSpan.Zero;
			var contract = personPeriod.PersonContract.Contract;
			switch (contract.WorkTimeSource)
			{
				case WorkTimeSource.FromContract:
					averageWorkTimePerDay = contract.WorkTime.AvgWorkTimePerDay;
					break;
				case WorkTimeSource.FromSchedulePeriod:
					var virtualSchedulePeriod = innerVirtualSchedulePeriod(personPeriod, schedulePeriod, day);
					averageWorkTimePerDay = schedulePeriod == null
						? WorkTime.DefaultWorkTime.AvgWorkTimePerDay
						: virtualSchedulePeriod.AverageWorkTimePerDay;
					break;
			}

			return averageWorkTimePerDay;
		}

		public virtual PersonWorkDay AverageWorkTimeOfDay(DateOnly dateOnly)
		{
			var personPeriod = Period(dateOnly);
			if (personPeriod == null) return new PersonWorkDay(dateOnly);

			var contract = personPeriod.PersonContract.Contract;
			var contractSchedule = personPeriod.PersonContract.ContractSchedule;
			var partTimePercentage = personPeriod.PersonContract.PartTimePercentage;
			return new PersonWorkDay(dateOnly, new Lazy<TimeSpan>(() => calculateAverageWorkTime(personPeriod, SchedulePeriod(dateOnly), dateOnly)),
				contract.WorkTimeSource,
				partTimePercentage?.Percentage ?? new Percent(1d),
				isWorkDay(contractSchedule, dateOnly));
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ 431;
		}

		public virtual ISiteOpenHour SiteOpenHour(DateOnly dateOnly)
		{
			var myTeam = MyTeam(dateOnly);
			if (myTeam != null)
			{
				var siteOpenHours = myTeam.Site.OpenHourCollection;
				foreach (var siteOpenHour in siteOpenHours)
				{
					if (siteOpenHour.WeekDay == dateOnly.DayOfWeek)
						return siteOpenHour;
				}
			}

			return null;
		}

		public virtual void AddPersonEmploymentChangeEvent(PersonEmploymentChangedEvent @event)
		{
			AddEvent(() => @event);
		}
	}
}