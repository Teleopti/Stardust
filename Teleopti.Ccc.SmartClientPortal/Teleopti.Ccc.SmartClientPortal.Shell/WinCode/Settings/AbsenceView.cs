using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings
{
    /// <summary>
    /// Represents the adapter for the absence.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-12-02
    /// </remarks>
    public class AbsenceView : EntityContainer<IAbsence>
    {
        /// <summary>
        /// Holds the tracker adapter relevant for the tracker of the hoding absence.
        /// </summary>
        private TrackerView _tracker;
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

        /// <summary>
        /// Gets or sets the description of the absence.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public Description Description
        {
            get { return ContainedEntity.Description; }
            set { ContainedEntity.Description = value; }
        }

        /// <summary>
        /// Gets or sets the display color.
        /// </summary>
        /// <value>The display color.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public Color DisplayColor
        {
            get { return ContainedEntity.DisplayColor; }
            set { ContainedEntity.DisplayColor = value; }
        }

        /// <summary>
        /// Gets or sets the payroll code.
        /// </summary>
        /// <value>The payroll code.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2009-03-03
        /// </remarks>
        public string PayrollCode
        {
            get { return ContainedEntity.PayrollCode ; }
            set{ ContainedEntity.PayrollCode = value;}
        }

        public string UpdatedTimeInUserPerspective
        {
            get { return _localizer.UpdatedTimeInUserPerspective(ContainedEntity); }
        }
				//public string CreatedTimeInUserPerspective
				//{
				//		get { return _localizer.CreatedTimeInUserPerspective(ContainedEntity); }
				//}

        /// <summary>
        /// Gets or sets a value indicating whether [in contract time].
        /// </summary>
        /// <value><c>true</c> if [in contract time]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public bool InContractTime
        {
            get { return ContainedEntity.InContractTime; }
            set { ContainedEntity.InContractTime = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [in work time].
        /// </summary>
        /// <value><c>true</c> if [in work time]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public bool InWorkTime
        {
            get { return ContainedEntity.InWorkTime; }
            set { ContainedEntity.InWorkTime = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [in paid time].
        /// </summary>
        /// <value><c>true</c> if [in paid time]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public bool InPaidTime
        {
            get { return ContainedEntity.InPaidTime; }
            set { ContainedEntity.InPaidTime = value; }
        }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>The priority.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public byte Priority
        {
            get { return ContainedEntity.Priority; }
            set { ContainedEntity.Priority = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AbsenceView"/> is requestable.
        /// </summary>
        /// <value><c>true</c> if requestable; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the tracker.
        /// </summary>
        /// <value>The tracker.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public TrackerView Tracker
        {
            get
            {
                if (ContainedEntity.Tracker == null)
                {
                    _tracker.Tracker = TrackerView.DefaultTracker;
                }

                return _tracker;
            }
            set
            {
                _tracker.Tracker = value.Tracker;

                ContainedEntity.Tracker = value.Tracker.Equals(TrackerView.DefaultTracker) ? null : value.Tracker;
            }
        }

        /// <summary>
        /// Gets the updated by.
        /// </summary>
        /// <value>The updated by.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public string UpdatedBy
        {
            get
            {
                if (ContainedEntity.UpdatedBy != null)
                    return ContainedEntity.UpdatedBy.Name.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the updated on.
        /// </summary>
        /// <value>The updated on.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public DateTime? UpdatedOn
        {
            get { return ContainedEntity.UpdatedOn; }
        }

        /// <summary>
        /// Gets the business unit.
        /// </summary>
        /// <value>The business unit.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public IBusinessUnit BusinessUnit
        {
            get
            {
                return ContainedEntity.GetOrFillWithBusinessUnit_DONTUSE();
            }
        }

		/// <summary>
        /// Gets or sets a value indicating whether the tracker of this absence is disabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the tracker of this absence is disabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsTrackerDisabled { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceView"/> class.
        /// </summary>
        /// <param name="absence">The absence.</param>
        /// <param name="disableTracker">Tells is <see cref="Tracker"/> should be disabled.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-12-02
        /// </remarks>
        public AbsenceView(IAbsence absence, bool disableTracker)
        {
            // Sets the properties
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
            if (absence != null)
            {
                setContainedEntity(absence);
            }
            else
            {
                setContainedEntity(ContainedOriginalEntity);
            }
            IsTrackerDisabled = disableTracker;
        }
    }
}
