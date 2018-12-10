using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    public class PersonAvailabilityModelBase : IRotationModel<IPersonAvailability, IAvailabilityRotation>
    {
        private DateOnly? _startDate;
        private IAvailabilityRotation _currentAvailability;
        private readonly CommonNameDescriptionSetting _commonNameDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAvailabilityModelBase"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="personAvailability">The person availability.</param>
        /// <param name="commonNameDescription">The common name description.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public PersonAvailabilityModelBase(IPerson person, IPersonAvailability personAvailability, CommonNameDescriptionSetting commonNameDescription)
        {
            Person = person;
            PersonRotation = personAvailability;
            _commonNameDescription = commonNameDescription;
        }

        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        /// <value>The full name of the person.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public virtual string PersonFullName
        {
            get
            {
                if (_commonNameDescription == null)
                    return Person.Name.ToString();
                return _commonNameDescription.BuildFor(Person);
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public IPerson Person
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets from date.
        /// </summary>
        /// <value>From date.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public DateOnly? FromDate
        {
            get
            {
                if (PersonRotation != null)
                {
                    _startDate = PersonRotation.StartDate;
                    return _startDate;
                }
                return null;
            }
            set
            {
                _startDate = value;
                if (PersonRotation != null && _startDate.HasValue)
                    PersonRotation.StartDate = value.Value;
            }
        }

        public bool CanBold
        {
            get ; 
            set ; 
        }

        public int StartWeek
        {
            get
            {
                if (PersonRotation != null)
                {
                    return PersonRotation.StartDay / 7;
                }
                return -1;
            }
            set
            {
                if (PersonRotation != null)
                {
                    if (((long)value * 7) > int.MaxValue)
                        PersonRotation.StartDay = int.MaxValue;
                    else
                        PersonRotation.StartDay = (int)((long)value * 7);
                }
            }
        }

        /// <summary>
        /// Gets or sets the person rotation.
        /// </summary>
        /// <value>The person rotation.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public IPersonAvailability PersonRotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the grid control.
        /// </summary>
        /// <value>The grid control.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public GridControl GridControl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [expand state].
        /// </summary>
        /// <value><c>true</c> if [expand state]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public bool ExpandState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance can gray.
        /// </summary>
        /// <value><c>true</c> if this instance can gray; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public bool CanGray
        {
            get
            {
                return PersonRotation == null;
            }
        }

        /// <summary>
        /// Gets or sets the current rotation.
        /// </summary>
        /// <value>The current rotation.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public IAvailabilityRotation CurrentRotation
        {
            get { return _currentAvailability; }
            set
            {
                _currentAvailability = value;
                if (PersonRotation != null)
                    PersonRotation.Availability = value;
                else
                    return;
            }
        }

    }
}
