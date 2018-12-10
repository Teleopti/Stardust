using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// This class was added with the purpose of putting all the common properties of GridView adapters at a single place
    /// </summary>
    public class PersonRotationModelBase :  IRotationModel<IPersonRotation, IRotation>
    {
        /// <summary>
        /// The person of concern.
        /// </summary>
        private IPerson _person;
        private readonly CommonNameDescriptionSetting _commonNameDescription;

        /// <summary>
        /// The person rotation of concern.
        /// </summary>
        private IPersonRotation _personRotation;

        /// <summary>
        /// /The start date of the Rotation for the respective person
        /// </summary>
        private DateOnly? _startDate;
        private IRotation _currentRotation;

        /// <summary>
        /// The overloaded constructor to set the properties.
        /// </summary>
        /// <param name="person">The person</param>
        /// <param name="commonNameDescription">The common name description.</param>
        public PersonRotationModelBase(IPerson person, CommonNameDescriptionSetting commonNameDescription)
        {
            _person = person;
            _commonNameDescription = commonNameDescription;
        }

        /// <summary>
        /// Returns the full name of the person
        /// </summary>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-07-23
        /// </remarks>
        public virtual string PersonFullName
        {
            get
            {
                if (_commonNameDescription == null)
                    return _person.Name.ToString();
                
                return _commonNameDescription.BuildFor(_person);
            }
            set { }
        }

        /// <summary>
        /// The start date of the rotation
        /// </summary>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-07-23
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
            get;
            set;
        }

        /// <summary>
        /// The rotation start week for the given person
        /// </summary>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-07-23
        /// </remarks>
        public int StartWeek
        {
            get
            {
                if (_personRotation != null)
                {
                    return (_personRotation.StartDay / 7) + 1;
                }
                return -1;
            }
            set
            {
                if (_personRotation != null)
                {
                    value--;
                    int startDay = int.MaxValue;
                    if (((long)value * 7) < int.MaxValue)
                        startDay = value * 7;

                    _personRotation.StartDay = startDay;
                }
            }
        }

        /// <summary>
        /// The Person of the rotation. We need to hold this apart from the person rotation since 
        /// the adapter can hold persons even without a person-rotation
        /// </summary>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-07-23
        /// </remarks>
        public IPerson Person
        {
            get { return _person; }
            set { _person = value; }
        }

        /// <summary>
        /// The Person Rotation
        /// </summary>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-07-23
        /// </remarks>
        public IPersonRotation PersonRotation
        {
            get { return _personRotation; }
            set { _personRotation = value; }
        }

        public GridControl GridControl
        {
            get;
            set;
        }

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
        /// Created by:     Shiran Ginige
        /// Created date:   2008-07-07
        /// </remarks>
        public bool CanGray
        {
            get
            {
                return _personRotation == null;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <remarks>
        /// Created by:     Shiran Ginige
        /// Created date:   2008-06-15
        /// </remarks>
        public IRotation CurrentRotation
        {
            get { return _currentRotation; }
            set
            {
				if (value == null) return;
                _currentRotation = value;
                if (_personRotation != null)
                    _personRotation.Rotation = value;
                else
                    return;
            }
        }

        /// <summary>
        /// Gets the rotation week count.
        /// </summary>
        /// <value>The rotation week count.</value>
        public IList<int> RotationWeekCount
        {
            get
            {
                IList<int> weekList = new List<int>();

                if (_personRotation != null && _personRotation.Rotation.RotationDays.Count >= 7)
                {
                    int weekCount = _personRotation.Rotation.RotationDays.Count / 7;

                    for (int i = 0; i < weekCount; i++)
                    {
                        weekList.Add(i + 1);
                    }
                }
                return weekList;

            }
        }
    }
}
