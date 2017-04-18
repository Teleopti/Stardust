using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// The adapter to convert the <see cref="ShiftCategory"/> to the format that neeeds to be displayed
    /// </summary>
    public class ShiftCategoryModel
    {
        private IShiftCategory _containedEntity;
        private string _shortName;
        private string _name;
        private Color _displayColor;
        private LocalizedUpdateInfo _localizer;

        public ShiftCategoryModel(IShiftCategory shiftCategory)
        {
            _containedEntity = shiftCategory;
            _localizer = new LocalizedUpdateInfo();
        }

        public IShiftCategory ContainedEntity
        {
            get
            {
                return _containedEntity;
            }
        }

        /// <summary>
        /// The short description name
        /// </summary>
        public string ShortName
        {
            get
            {
                return _containedEntity.Description.ShortName;
            }
            set
            {
                _shortName = value;
                Description desc = new Description(_containedEntity.Description.Name, _shortName);
                _containedEntity.Description = desc;
            }
        }

        /// <summary>
        /// The full description name
        /// </summary>
        public string Name
        {
            get
            {
                return _containedEntity.Description.Name;
            }
            set
            {
                _name = value;
                Description desc = new Description(_name, _containedEntity.Description.ShortName);
                _containedEntity.Description = desc;
            }
        }

        /// <summary>
        /// The display color
        /// </summary>
        public Color DisplayColor
        {
            get
            {
                return _containedEntity.DisplayColor;
            }
            set
            {
                _displayColor = value;
                _containedEntity.DisplayColor = _displayColor;
            }
        }

        public IPerson CreatedBy
        {
					get { 
						//return "will be removed"; 
						return null;
					}
        }

        public DateTime? CreatedOn
        {
            get
            {
	             //"will be removed"
							//return ContainedEntity.CreatedOn;
							return new DateTime(1900,1,1);
            }
        }

        public IPerson UpdatedBy
        {
            get { return ContainedEntity.UpdatedBy; }
        }

        public DateTime? UpdatedOn
        {
            get { return ContainedEntity.UpdatedOn; }
        }

        public string UpdatedTimeInUserPerspective { get { return _localizer.UpdatedTimeInUserPerspective(ContainedEntity); } }
    }
}
