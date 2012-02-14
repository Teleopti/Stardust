using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/05/")]
    public class WorkflowControlSetDto : Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowControlSetDto"/> class.
        /// </summary>
        public WorkflowControlSetDto()
        {
            AllowedPreferenceShiftCategories = new List<ShiftCategoryDto>();
            AllowedPreferenceAbsences = new List<AbsenceDto>();
            AllowedPreferenceDayOffs = new List<DayOffInfoDto>();
        }

        /// <summary>
        /// Gets or sets the activity enabled for creating activity restrictions.
        /// </summary>
        /// <remarks>Null value means that activity restrictions are disabled.</remarks>
        [DataMember]
        public ActivityDto AllowedPreferenceActivity { get; set; }

        /// <summary>
        /// Gets or sets the period to enter preferences for.
        /// </summary>
        [DataMember]
        public DateOnlyPeriodDto PreferencePeriod { get; set; }

    /// <summary>
    /// Gets or sets the period when entering preferences is allowed.
    /// </summary>
        [DataMember]
        public DateOnlyPeriodDto PreferenceInputPeriod { get; set; }

        /// <summary>
        /// Gets or sets the shift categories available for preferences.
        /// </summary>
        [DataMember]
        public ICollection<ShiftCategoryDto> AllowedPreferenceShiftCategories { get; private set; }

        /// <summary>
        /// Gets or sets the days off available for perferences.
        /// </summary>
        [DataMember]
        public ICollection<DayOffInfoDto> AllowedPreferenceDayOffs { get; private set; }
        
        /// <summary>
        /// Gets or sets the period to enter student availability for.
        /// </summary>
        [DataMember]
        public DateOnlyPeriodDto StudentAvailabilityPeriod { get; set; }

        /// <summary>
        /// Gets or sets the period when entering student availabilities is allowed.
        /// </summary>
        [DataMember]
        public DateOnlyPeriodDto StudentAvailabilityInputPeriod { get; set; }

        /// <summary>
        /// Gets or sets the absences available for preferences.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1)]
        public ICollection<AbsenceDto> AllowedPreferenceAbsences { get; private set; }

		/// <summary>
		/// Gets or sets the date for latest published schedules.
		/// </summary>
		[DataMember(IsRequired = false, Order = 2)]
		public DateTime? SchedulesPublishedToDate { get; set; }
    }
}