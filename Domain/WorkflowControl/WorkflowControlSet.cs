using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class WorkflowControlSet : VersionedAggregateRootWithBusinessUnit, IWorkflowControlSet, IDeleteTag
	{
		private string _name = string.Empty;
		private bool _isDeleted;
		private IList<IAbsenceRequestOpenPeriod> _absenceRequestOpenPeriods;
		private ISet<ISkill> _mustMatchSkills;
		private ISet<IShiftCategory> _allowedPreferenceShiftCategories;
		private ISet<IDayOffTemplate> _allowedPreferenceDayOffs;
		private ISet<IAbsence> _allowedPreferenceAbsences;
		private ISet<IAbsence> _allowedAbsencesForReport;
		private DateOnly preferencePeriodFromDate;
		private DateOnly preferencePeriodToDate;
		private DateOnly preferenceInputFromDate;
		private DateOnly preferenceInputToDate;
		private DateOnly studentAvailabilityPeriodFromDate;
		private DateOnly studentAvailabilityPeriodToDate;
		private DateOnly studentAvailabilityInputFromDate;
		private DateOnly studentAvailabilityInputToDate;
		private bool _autoGrantShiftTradeRequest;
		private bool _anonymousTrading;
		private bool _lockTrading;
		private bool _absenceRequestWaitlistEnabled;
		private WaitlistProcessOrder _absenceRequestWaitlistProcessOrder;
		private int? _absenceRequestCancellationThreshold;
		private int? _absenceRequestExpiredThreshold;
		private DateTime? _schedulePublishedToDate;
		private int? _writeProtection;
		private TimeSpan _shiftTradeTargetTimeFlexibility;
		private MinMax<int> _shiftTradeOpenPeriodDaysForward;
		private IActivity _allowedPreferenceActivity;
		private int fairnessTypeAsInt = (int)FairnessType.EqualNumberOfShiftCategory;

		public WorkflowControlSet()
		{
			_absenceRequestOpenPeriods = new List<IAbsenceRequestOpenPeriod>();
			_allowedPreferenceShiftCategories = new HashSet<IShiftCategory>();
			_allowedPreferenceDayOffs = new HashSet<IDayOffTemplate>();
			_allowedPreferenceAbsences = new HashSet<IAbsence>();
			_allowedAbsencesForReport = new HashSet<IAbsence>();
			_mustMatchSkills = new HashSet<ISkill>();
			preferencePeriodFromDate = new DateOnly(DateHelper.MinSmallDateTime);
			preferencePeriodToDate = new DateOnly(DateHelper.MaxSmallDateTime);
			preferenceInputFromDate = new DateOnly(DateHelper.MinSmallDateTime);
			preferenceInputToDate = new DateOnly(DateHelper.MaxSmallDateTime);
			studentAvailabilityPeriodFromDate = new DateOnly(DateHelper.MinSmallDateTime);
			studentAvailabilityPeriodToDate = new DateOnly(DateHelper.MaxSmallDateTime);
			studentAvailabilityInputFromDate = new DateOnly(DateHelper.MinSmallDateTime);
			studentAvailabilityInputToDate = new DateOnly(DateHelper.MaxSmallDateTime);
		}

		public WorkflowControlSet(string description)
			: this()
		{
			_name = description;
		}

		public virtual FairnessType GetFairnessType()
		{
			return (FairnessType)fairnessTypeAsInt;
		}

		public virtual void SetFairnessType(FairnessType fairnessType)
		{
			fairnessTypeAsInt = (int)fairnessType;
		}

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual IActivity AllowedPreferenceActivity
		{
			get { return _allowedPreferenceActivity; }
			set { _allowedPreferenceActivity = value; }
		}

		public virtual void AddOpenAbsenceRequestPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			absenceRequestOpenPeriod.SetParent(this);
			_absenceRequestOpenPeriods.Add(absenceRequestOpenPeriod);
		}

		public virtual IOpenAbsenceRequestPeriodExtractor GetExtractorForAbsence(IAbsence absence)
		{
			return new OpenAbsenceRequestPeriodExtractor(this, absence);
		}

		public virtual void MovePeriodDown(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var index = _absenceRequestOpenPeriods.IndexOf(absenceRequestOpenPeriod);
			if (index < _absenceRequestOpenPeriods.Count - 1)
			{
				_absenceRequestOpenPeriods.Remove(absenceRequestOpenPeriod);
				_absenceRequestOpenPeriods.Insert(index + 1, absenceRequestOpenPeriod);
			}
		}

		public virtual void MovePeriodUp(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var index = _absenceRequestOpenPeriods.IndexOf(absenceRequestOpenPeriod);
			if (index > 0)
			{
				_absenceRequestOpenPeriods.Remove(absenceRequestOpenPeriod);
				_absenceRequestOpenPeriods.Insert(index - 1, absenceRequestOpenPeriod);
			}
		}

		public virtual int RemoveOpenAbsenceRequestPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			var orderIndex = _absenceRequestOpenPeriods.IndexOf(absenceRequestOpenPeriod);
			_absenceRequestOpenPeriods.Remove(absenceRequestOpenPeriod);

			return orderIndex;
		}

		public virtual void InsertPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod, int orderIndex)
		{
			absenceRequestOpenPeriod.SetParent(this);
			_absenceRequestOpenPeriods.Insert(orderIndex, absenceRequestOpenPeriod);
		}

		public virtual ReadOnlyCollection<IAbsenceRequestOpenPeriod> AbsenceRequestOpenPeriods { get { return new ReadOnlyCollection<IAbsenceRequestOpenPeriod>(_absenceRequestOpenPeriods); } }

		public virtual bool IsDeleted { get { return _isDeleted; } }

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		public virtual IWorkflowControlSet NoneEntityClone()
		{
			var clone = entityCloneInternal(p => p.NoneEntityClone());
			clone.SetId(null);
			return clone;
		}

		private IWorkflowControlSet entityCloneInternal(Func<IAbsenceRequestOpenPeriod, IAbsenceRequestOpenPeriod> periodCreator)
		{
			var clone = (WorkflowControlSet)MemberwiseClone();
			clone._absenceRequestOpenPeriods = new List<IAbsenceRequestOpenPeriod>();
			foreach (var openPeriod in _absenceRequestOpenPeriods)
			{
				var periodClone = periodCreator(openPeriod);
				periodClone.SetParent(clone);
				clone._absenceRequestOpenPeriods.Add(periodClone);
			}
			clone._allowedPreferenceDayOffs = new HashSet<IDayOffTemplate>(_allowedPreferenceDayOffs);
			clone._allowedPreferenceShiftCategories = new HashSet<IShiftCategory>(_allowedPreferenceShiftCategories);
			clone._allowedPreferenceAbsences = new HashSet<IAbsence>(_allowedPreferenceAbsences);
			clone._allowedAbsencesForReport = new HashSet<IAbsence>(_allowedAbsencesForReport);
			clone._mustMatchSkills = new HashSet<ISkill>(_mustMatchSkills);
			return clone;
		}

		public virtual IWorkflowControlSet EntityClone()
		{
			return entityCloneInternal(p => p.EntityClone());
		}

		public virtual DateTime? SchedulePublishedToDate
		{
			get { return _schedulePublishedToDate; }
			set { _schedulePublishedToDate = value; }
		}

		public virtual DateOnlyPeriod PreferencePeriod
		{
			get { return new DateOnlyPeriod(preferencePeriodFromDate, preferencePeriodToDate); }
			set
			{
				preferencePeriodFromDate = value.StartDate;
				preferencePeriodToDate = value.EndDate;
			}
		}

		public virtual DateOnlyPeriod PreferenceInputPeriod
		{
			get { return new DateOnlyPeriod(preferenceInputFromDate, preferenceInputToDate); }
			set
			{
				preferenceInputFromDate = value.StartDate;
				preferenceInputToDate = value.EndDate;
			}
		}

		public virtual DateOnlyPeriod StudentAvailabilityPeriod
		{
			get { return new DateOnlyPeriod(studentAvailabilityPeriodFromDate, studentAvailabilityPeriodToDate); }
			set
			{
				studentAvailabilityPeriodFromDate = value.StartDate;
				studentAvailabilityPeriodToDate = value.EndDate;
			}
		}

		public virtual DateOnlyPeriod StudentAvailabilityInputPeriod
		{
			get { return new DateOnlyPeriod(studentAvailabilityInputFromDate, studentAvailabilityInputToDate); }
			set
			{
				studentAvailabilityInputFromDate = value.StartDate;
				studentAvailabilityInputToDate = value.EndDate;
			}
		}

		public virtual int? WriteProtection
		{
			get { return _writeProtection; }
			set { _writeProtection = value; }
		}

		public virtual TimeSpan ShiftTradeTargetTimeFlexibility
		{
			get { return _shiftTradeTargetTimeFlexibility; }
			set { _shiftTradeTargetTimeFlexibility = value; }
		}

		public virtual IEnumerable<ISkill> MustMatchSkills { get { return _mustMatchSkills; } }

		public virtual void AddSkillToMatchList(ISkill skill)
		{
			if (!_mustMatchSkills.Contains(skill))
				_mustMatchSkills.Add(skill);
		}

		public virtual void RemoveSkillFromMatchList(ISkill skill)
		{
			if (_mustMatchSkills.Contains(skill))
				_mustMatchSkills.Remove(skill);
		}

		public virtual MinMax<int> ShiftTradeOpenPeriodDaysForward
		{
			get { return _shiftTradeOpenPeriodDaysForward; }
			set { _shiftTradeOpenPeriodDaysForward = value; }
		}

		public virtual void AddAllowedPreferenceShiftCategory(IShiftCategory shiftCategory)
		{
			_allowedPreferenceShiftCategories.Add(shiftCategory);
		}

		public virtual void RemoveAllowedPreferenceShiftCategory(IShiftCategory shiftCategory)
		{
			if (_allowedPreferenceShiftCategories.Contains(shiftCategory))
				_allowedPreferenceShiftCategories.Remove(shiftCategory);
		}

		public virtual IEnumerable<IShiftCategory> AllowedPreferenceShiftCategories
		{
			get { return _allowedPreferenceShiftCategories; }
			set { _allowedPreferenceShiftCategories = new HashSet<IShiftCategory>(new List<IShiftCategory>(value)); }
		}

		public virtual void AddAllowedPreferenceDayOff(IDayOffTemplate dayOff)
		{
			_allowedPreferenceDayOffs.Add(dayOff);
		}

		public virtual void RemoveAllowedPreferenceDayOff(IDayOffTemplate dayOff)
		{
			if (_allowedPreferenceDayOffs.Contains(dayOff))
				_allowedPreferenceDayOffs.Remove(dayOff);
		}

		public virtual IEnumerable<IDayOffTemplate> AllowedPreferenceDayOffs
		{
			get { return _allowedPreferenceDayOffs; }
			set { _allowedPreferenceDayOffs = new HashSet<IDayOffTemplate>(new List<IDayOffTemplate>(value)); }
		}

		public virtual bool AutoGrantShiftTradeRequest
		{
			get { return _autoGrantShiftTradeRequest; }
			set { _autoGrantShiftTradeRequest = value; }
		}

		public virtual bool AnonymousTrading
		{
			get { return _anonymousTrading; }
			set { _anonymousTrading = value; }
		}

		public virtual IEnumerable<IAbsence> AllowedPreferenceAbsences
		{
			get { return _allowedPreferenceAbsences; }
			set { _allowedPreferenceAbsences = new HashSet<IAbsence>(new List<IAbsence>(value)); }
		}

		public virtual void AddAllowedPreferenceAbsence(IAbsence absence)
		{
			_allowedPreferenceAbsences.Add(absence);
		}

		public virtual void RemoveAllowedPreferenceAbsence(IAbsence absence)
		{
			if (_allowedPreferenceAbsences.Contains(absence))
				_allowedPreferenceAbsences.Remove(absence);
		}

		public virtual IEnumerable<IAbsence> AllowedAbsencesForReport
		{
			get { return _allowedAbsencesForReport; }
			set { _allowedAbsencesForReport = new HashSet<IAbsence>(new List<IAbsence>(value)); }
		}

		public virtual bool LockTrading
		{
			get { return _lockTrading; }
			set { _lockTrading = value; }
		}

		public virtual bool AbsenceRequestWaitlistEnabled
		{
			get { return _absenceRequestWaitlistEnabled; }
			set { _absenceRequestWaitlistEnabled = value; }
		}

		public virtual WaitlistProcessOrder AbsenceRequestWaitlistProcessOrder
		{
			get { return _absenceRequestWaitlistProcessOrder; }
			set { _absenceRequestWaitlistProcessOrder = value; }
		}

		public virtual int? AbsenceRequestCancellationThreshold
		{
			get { return _absenceRequestCancellationThreshold; }
			set { _absenceRequestCancellationThreshold = value; }
		}

		public virtual int? AbsenceRequestExpiredThreshold
		{
			get { return _absenceRequestExpiredThreshold; }
			set { _absenceRequestExpiredThreshold = value; }
		}

		public virtual void AddAllowedAbsenceForReport(IAbsence absence)
		{
			_allowedAbsencesForReport.Add(absence);
		}

		public virtual void RemoveAllowedAbsenceForReport(IAbsence absence)
		{
			if (_allowedAbsencesForReport.Contains(absence))
				_allowedAbsencesForReport.Remove(absence);
		}

		public virtual bool WaitlistingIsEnabled(IAbsenceRequest absenceRequest)
		{
			if (AbsenceRequestWaitlistEnabled)
			{
				return GetMergedAbsenceRequestOpenPeriod(absenceRequest).AbsenceRequestProcess is GrantAbsenceRequest;
			}

			return false;
		}

		public virtual IAbsenceRequestOpenPeriod GetMergedAbsenceRequestOpenPeriod(IAbsenceRequest absenceRequest)
		{
			 var agentTimeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(agentTimeZone);

			return getMergedOpenPeriods(absenceRequest, dateOnlyPeriod);
		}

		public virtual bool IsAbsenceRequestValidatorEnabled<T>(TimeZoneInfo timeZone)
			where T : IAbsenceRequestValidator
		{
			var expectedValidatorType = typeof(T);
			var today = new DateOnly(TimeZoneHelper.ConvertFromUtc(DateTime.UtcNow, timeZone));
			var validOpenPeriods = AbsenceRequestOpenPeriods.Where(
				p => p.OpenForRequestsPeriod.Contains(today) && p.GetPeriod(today).Contains(today)
					 && p.AbsenceRequestProcess.GetType() != typeof(DenyAbsenceRequest));

			return validOpenPeriods.Any(p =>
				p.StaffingThresholdValidator != null && p.StaffingThresholdValidator.GetType() == expectedValidatorType);
		}

		private IAbsenceRequestOpenPeriod getMergedOpenPeriods(IAbsenceRequest absenceRequest, DateOnlyPeriod dateOnlyPeriod)
		{
			var extractor = GetExtractorForAbsence(absenceRequest.Absence);
			extractor.ViewpointDate = ServiceLocatorForEntity.Now.LocalDateOnly();

			var openPeriods = extractor.Projection.GetProjectedPeriods(dateOnlyPeriod,
				absenceRequest.Person.PermissionInformation.Culture(), absenceRequest.Person.PermissionInformation.UICulture());
			return new AbsenceRequestOpenPeriodMerger().Merge(openPeriods);
		}
	}
}
