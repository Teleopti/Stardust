using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents an SchedulePartDto object which contains schedule information for one day.
    /// </summary>
	[DebuggerDisplay("{Date?.DateTime.ToShortDateString()} - {PersonId}")]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class SchedulePartDto : Dto
    {
        private ICollection<PersonAbsenceDto> _personAbsenceCollection = new List<PersonAbsenceDto>();
        private ICollection<PersonAssignmentDto> _personAssignmentCollection = new List<PersonAssignmentDto>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulePartDto"/> class.
        /// </summary>
        public SchedulePartDto()
        {
            ProjectedLayerCollection = new List<ProjectedLayerDto>();
            PersonMeetingCollection = new List<PersonMeetingDto>();
        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        [DataMember]
        public DateOnlyDto Date { get; set; }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the time zone id.
        /// </summary>
        [DataMember]
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets the day off.
        /// </summary>
        /// <remarks>If PersonDayOff is null there is no day off specified for this day.</remarks>
        [DataMember]
        public PersonDayOffDto PersonDayOff { get; set; }

        /// <summary>
        /// Gets the projected layer collection.
        /// </summary>
        /// <remarks>The projection is read only and contains the merged layers from main shift, personal shifts, overtime shifts, meetings and absences.</remarks>
        [DataMember]
        public ICollection<ProjectedLayerDto> ProjectedLayerCollection { get; private set; }

        /// <summary>
        /// Gets the collection of person assigments.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The setter is used in serialization."), DataMember]
        public ICollection<PersonAssignmentDto> PersonAssignmentCollection
        {
            get { return _personAssignmentCollection; }
            private set
            {
                if (value!=null)
                {
                    _personAssignmentCollection = new List<PersonAssignmentDto>(value);
                }
            }
        }

        /// <summary>
        /// Gets the collection of person absences.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The setter is used in serialization."), DataMember]
        public ICollection<PersonAbsenceDto> PersonAbsenceCollection
        {
            get { return _personAbsenceCollection; }
            private set
            {
                if (value!=null)
                {
                    _personAbsenceCollection = new List<PersonAbsenceDto>(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the student availability for this day.
        /// </summary>
        /// <remarks>This information is read only in this context and will not be saved when saving schedules.</remarks>
        [DataMember]
		[Obsolete("This is not exposing details anymore. Use designated functions.")]
        public StudentAvailabilityDayDto StudentAvailabilityDay { get; set; }

        /// <summary>
        /// Gets or sets the preference for this day.
        /// </summary>
        /// <remarks>This information is read only in this context and will not be saved when saving schedules.</remarks>
        [DataMember]
		[Obsolete("This is not exposing details anymore. Use designated functions.")]
        public PreferenceRestrictionDto PreferenceRestriction { get; set; }

        /// <summary>
        /// Gets or sets the meetings involving the current person for this day.
        /// </summary>
        /// <remarks>This information is read only in this context and will not be saved when saving schedules.</remarks>
        [DataMember]
        public ICollection<PersonMeetingDto> PersonMeetingCollection { get; private set; }

        /// <summary>
        /// Gets or sets the contract time.
        /// </summary>
        /// <remarks>This value is set with 0001-01-01 00:00 as base line date. 9 hours of contract time is returned as 0001-01-01 09:00.</remarks>
        [DataMember]
        public DateTime ContractTime { get; set; }
		
		/// <summary>
		/// Gets or sets the work time.
		/// </summary>
		/// <remarks>This value is set with 0001-01-01 00:00 as base line date. 9 hours of contract time is returned as 0001-01-01 09:00.</remarks>
		[DataMember(IsRequired = false, Order = 2)]
		public DateTime WorkTime { get; set; }
		
		/// <summary>
		/// Gets or sets the paid time.
		/// </summary>
		/// <remarks>This value is set with 0001-01-01 00:00 as base line date. 9 hours of contract time is returned as 0001-01-01 09:00.</remarks>
		[DataMember(IsRequired = false, Order = 2)]
		public DateTime PaidTime { get; set; }


        /// <summary>
        /// Gets or sets the period for the current day.
        /// </summary>
        /// <remarks>This is not the period for the shift.</remarks>
        [DataMember]
        public DateTimePeriodDto LocalPeriod { get; set; }

        /// <summary>
        /// Gets or sets an indication if this is a full day absence.
        /// </summary>
        [DataMember]
        public bool IsFullDayAbsence { get; set; }

		/// <summary>
		/// Gets or sets the schedule tag.
		/// </summary>
		[DataMember]
		public ScheduleTagDto ScheduleTag { get; set; }

    }
}