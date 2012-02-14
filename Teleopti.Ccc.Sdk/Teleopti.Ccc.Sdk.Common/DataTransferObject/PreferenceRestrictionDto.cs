using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Preference details for one day
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PreferenceRestrictionDto : RestrictionBaseDto, IPreferenceContainerDto
    {
        private ICollection<ActivityRestrictionDto> _activityRestrictionCollection = new List<ActivityRestrictionDto>();
        private ICollection<ActivityDto> _activityCollection = new List<ActivityDto>();

        /// <summary>
        /// Gets or sets a shift category preference
        /// </summary>
        /// <remarks>A shift category cannot be preferred in conjunction with a day off.</remarks>
        [DataMember]
        public ShiftCategoryDto ShiftCategory { get; set; }

        /// <summary>
        /// Gets or sets a day off preference
        /// </summary>
        /// <remarks>A day off cannot be preferred in conjunction with a shift category.</remarks>
        [DataMember]
        public DayOffInfoDto DayOff { get; set; }

        /// <summary>
        /// Gets or sets the activity restriction collection.
        /// A collection with restrictions of a certain activity
        /// </summary>
        /// <value>The activity restriction collection.</value>
        [DataMember(IsRequired = false, Order = 1)]
        public ICollection<ActivityRestrictionDto> ActivityRestrictionCollection
        {
            get { return _activityRestrictionCollection; }
            private set
            {
                if (value!=null)
                {
                    _activityRestrictionCollection = new List<ActivityRestrictionDto>(value);
                }
            }
        }

        /// <summary>
        /// Deprecated. Use ActivityRestrictionCollection instead
        /// Gets or sets the activity collection.
        /// </summary>
        /// <value>The activity collection.</value>
        [DataMember]
        [Obsolete("Deprecated. Use ActivityRestrictionCollection instead")]
        public ICollection<ActivityDto> ActivityCollection
        {
            get { return _activityCollection; }
            private set
            {
                if (value!=null)
                {
                    _activityCollection = new List<ActivityDto>(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the date for this preference
        /// </summary>
        [DataMember]
        public DateOnlyDto RestrictionDate { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        [DataMember]
        public PersonDto Person { get; set; }

        /// <summary>
        /// Gets or sets the must have. (High priority preference)
        /// </summary>
        [DataMember]
        public bool MustHave { get; set; }

        /// <summary>
        /// Gets or sets the template name this preference was created from.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1)]
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets absence preference
        /// </summary>
        [DataMember(IsRequired = false, Order = 2)]
        public AbsenceDto Absence { get; set; }
    }
}