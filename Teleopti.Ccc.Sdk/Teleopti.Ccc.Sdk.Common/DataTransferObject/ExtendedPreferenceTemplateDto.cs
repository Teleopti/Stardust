using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Template for extended preferences.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ExtendedPreferenceTemplateDto : Dto, IPreferenceContainerDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedPreferenceTemplateDto"/> class.
        /// </summary>
        public ExtendedPreferenceTemplateDto()
        {
            ActivityRestrictionCollection = new List<ActivityRestrictionDto>();
        }

        /// <summary>
        /// Gets or sets the display color.
        /// </summary>
        [DataMember]
        public ColorDto DisplayColor { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the shift category.
        /// </summary>
        /// <remarks>Cannot be set in conjunction with DayOff.</remarks>
        [DataMember]
        public ShiftCategoryDto ShiftCategory { get; set; }

        /// <summary>
        /// Gets or sets the day off.
        /// </summary>
        /// <remarks>Cannot be set in conjunction with ShiftCategory.</remarks>
        [DataMember]
        public DayOffInfoDto DayOff { get; set; }


        /// <summary>
        /// Gets the collection of activity restrictions.
        /// </summary>
        [DataMember]
        public ICollection<ActivityRestrictionDto> ActivityRestrictionCollection { get; private set; }

        /// <summary>
        /// Gets or sets the start time preferences.
        /// </summary>
        [DataMember]
        public TimeLimitationDto StartTimeLimitation { get; set; }

        /// <summary>
        /// Gets or sets the end time preferences.
        /// </summary>
        [DataMember]
        public TimeLimitationDto EndTimeLimitation { get; set; }

        /// <summary>
        /// Gets or sets the work time preferences.
        /// </summary>
        [DataMember]
        public TimeLimitationDto WorkTimeLimitation { get; set; }

        /// <summary>
        /// Gets or sets the text representation of the start time preferences.
        /// </summary>
        [DataMember]
        public string LimitationStartTimeString { get; set; }

        /// <summary>
        /// Gets or sets the text representation of the end time preferences.
        /// </summary>
        [DataMember]
        public string LimitationEndTimeString { get; set; }

        /// <summary>
        /// Gets or sets absence preference
        /// </summary>
        [DataMember(IsRequired = false, Order = 1)]
        public AbsenceDto Absence { get; set; }
    }
}
