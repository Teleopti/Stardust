using System;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Settings
{
    public class AbsenceView : EntityContainer<IAbsence>
    {
        private TrackerView _tracker;
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

        public Description Description
        {
            get { return ContainedEntity.Description; }
            set { ContainedEntity.Description = value; }
        }

		public Color DisplayColor
        {
            get { return ContainedEntity.DisplayColor; }
            set { ContainedEntity.DisplayColor = value; }
        }

        public string PayrollCode
        {
            get { return ContainedEntity.PayrollCode ; }
            set{ ContainedEntity.PayrollCode = value;}
        }

        public string UpdatedTimeInUserPerspective
        {
            get { return _localizer.UpdatedTimeInUserPerspective(ContainedEntity); }
        }
        public string CreatedTimeInUserPerspective
        {
            get { return _localizer.CreatedTimeInUserPerspective(ContainedEntity); }
        }

        public bool InContractTime
        {
            get { return ContainedEntity.InContractTime; }
            set { ContainedEntity.InContractTime = value; }
        }

        public bool InWorkTime
        {
            get { return ContainedEntity.InWorkTime; }
            set { ContainedEntity.InWorkTime = value; }
        }

        public bool InPaidTime
        {
            get { return ContainedEntity.InPaidTime; }
            set { ContainedEntity.InPaidTime = value; }
        }

        public byte Priority
        {
            get { return ContainedEntity.Priority; }
            set { ContainedEntity.Priority = value; }
        }

        public bool Requestable
        {
            get { return ContainedEntity.Requestable; }
            set { ContainedEntity.Requestable = value; }
        }
        public bool Confidential
        {
            get { return ContainedEntity.Confidential; }
            set { ContainedEntity.Confidential = value; }
        }

        public TrackerView Tracker
        {
            get
            {
	            if (ContainedEntity.Tracker == null)
		            _tracker.Tracker = TrackerView.DefaultTracker;

	            return _tracker;
            }
            set
            {
                _tracker.Tracker = value.Tracker;

                ContainedEntity.Tracker = value.Tracker.Equals(TrackerView.DefaultTracker) ? null : value.Tracker;
            }
        }

        public string UpdatedBy
        {
            get
            {
	            return ContainedEntity.UpdatedBy != null ? ContainedEntity.UpdatedBy.Name.ToString() : string.Empty;
            }
        }

        public DateTime? UpdatedOn
        {
            get { return ContainedEntity.UpdatedOn; }
        }

        public IBusinessUnit BusinessUnit
        {
            get
            {
                return ContainedEntity.BusinessUnit;
            }
        }

        public string CreatedBy
        {
            get
            {
	            return ContainedEntity.CreatedBy != null ? ContainedEntity.CreatedBy.Name.ToString() : string.Empty;
            }
        }
        public string CreatedOn
        {
            get
            {
	            return ContainedEntity.CreatedOn.HasValue ? ContainedEntity.CreatedOn.Value.ToString(CultureInfo.CurrentCulture) : string.Empty;
            }
        }

        public bool IsTrackerDisabled { get; private set; }

        public AbsenceView(IAbsence absence, bool disableTracker)
        {
            setContainedEntity(absence);
            IsTrackerDisabled = disableTracker;
        }

        private void setContainedEntity(IAbsence absence)
        {
            ContainedEntity = absence.EntityClone();
            ContainedOriginalEntity = absence;
            _tracker = new TrackerView(ContainedEntity.Tracker);
        }

        public IAbsence ContainedOriginalEntity { get; private set; }

        public void UpdateAfterMerge(IAbsence updatedAbsence)
        {
            setContainedEntity(updatedAbsence);
        }


        public void ResetAbsenceState(IAbsence absence, bool disableTracker)
        {
	        setContainedEntity(absence ?? ContainedOriginalEntity);
	        IsTrackerDisabled = disableTracker;
        }
    }
}
