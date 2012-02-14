#region Imports

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.Win.Permissions
{
    /// <summary>
    /// PermissionsDataHolder to hold the cached data for each role visited 
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 11/17/2008
    /// </remarks>
    public class PermissionsDataHolder
    {
        #region Fields - Instance Member

        private readonly ICollection<IPerson> _personCollection = new List<IPerson>();
        private IAvailableData _availableData;
        private bool _isDirtyRole;

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - ThisClass Members

        /// <summary>
        /// Gets the person collection.
        /// </summary>
        /// <value>The person collection.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public ICollection<IPerson> PersonCollection
        {
            get
            {
                return _personCollection;
            }
        }

        /// <summary>
        /// Gets or sets the available data.
        /// </summary>
        /// <value>The available data.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public IAvailableData AvailableData
        {
            get
            {
                return _availableData;
            }
            set
            {
                _availableData = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is dirty role.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is dirty role; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public bool IsDirtyRole
        {
            get
            {
                return _isDirtyRole;
            }
            set
            {
                _isDirtyRole = value;
            }
        }

        #endregion

        #endregion

        #region Methods - Instance Member
        
        #region Methods - Instance Member - ThisClass Members

        #region Methods - Instance Member - ThisClass Members - (constructors)

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsDataHolder"/> class.
        /// </summary>
        /// <param name="isDirtyRole">if set to <c>true</c> [is un saved role].</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public PermissionsDataHolder(bool isDirtyRole)
        {
            _isDirtyRole = isDirtyRole;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsDataHolder"/> class.
        /// </summary>
        /// <param name="personCollection">The person collection.</param>
        /// <param name="availableData">The available data.</param>
        /// <param name="isDirtyRole">if set to <c>true</c> [is un saved role].</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public PermissionsDataHolder(ICollection<IPerson> personCollection, IAvailableData availableData, bool isDirtyRole)
        {
            _personCollection = personCollection;
            _availableData = availableData;
            _isDirtyRole = isDirtyRole;
        }

        #endregion

        #region Methods - Instance Member - ThisClass Members - (helpers)

        /// <summary>
        /// Adds the person to collection.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void AddPersonToCollection(IPerson person)
        {
            if (!_personCollection.Contains(person))
            {
                _personCollection.Add(person);
            }
        }

        /// <summary>
        /// Removes the person from collection.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void RemovePersonFromCollection(IPerson person)
        {
            if (_personCollection.Contains(person))
            {
                _personCollection.Remove(person);
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
