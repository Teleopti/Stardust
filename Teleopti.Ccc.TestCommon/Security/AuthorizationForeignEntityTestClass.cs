using System;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Security
{

    /// <summary>
    /// Simple authorization foreign entity implementation for test purposes. It has nothing
    /// to do with AuthorizationEntity this is ok, needed for the test.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/23/2007
    /// </remarks>
    public class AuthorizationForeignEntityTestClass : IAuthorizationEntity
    {
        private string _keyField;
        private string _nameField;
        private string _descriptionField;
        private string _valueField;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationEntity"/> class.
        /// </summary>
        /// <param name="keyField">The key field.</param>
        /// <param name="nameField">The name value.</param>
        /// <param name="descriptionField">The description value.</param>
        /// <param name="valueField">The value field.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        public AuthorizationForeignEntityTestClass(string keyField, string nameField, string descriptionField, string valueField)
        {
            _keyField = keyField;
            _nameField = nameField;
            _descriptionField = descriptionField;
            _valueField = valueField;
        }

        #region IAuthorizationEntity Members

        /// <summary>
        /// Gets the Name value. Usually this is the key.
        /// </summary>
        /// <value>The name field.</value>
        public string AuthorizationKey
        {
            get
            {
                return _keyField;
            }
            set
            {
                _keyField = value;
            }
        }

        /// <summary>
        /// Gets the Name value. Usually this is the key.
        /// </summary>
        /// <value>The name field.</value>
        public string AuthorizationName
        {
            get
            {
                return _nameField;
            }
            set
            {
                _nameField = value;
            }
        }

        /// <summary>
        /// Gets the description or additional info value. Usually that is
        /// a longer description about the authorization entity.
        /// </summary>
        /// <value>The description field.</value>
        /// <remarks>
        /// Usually this value goes to the tooltip to the control.
        /// </remarks>
        public string AuthorizationDescription
        {
            get
            {
                return _descriptionField;
            }
            set
            {
                _descriptionField = value;
            }
        }

        /// <summary>
        /// Gets any additional value connected to the authorization
        /// </summary>
        /// <value>The value field.</value>
        /// <remarks>
        /// Usually this value holds some numeric data.
        /// </remarks>
        public string AuthorizationValue
        {
            get
            {
                return _valueField;
            }
            set
            {
                _valueField = value;
            }
        }

        #endregion

        #region IEntity Members

        public Guid? Id
        {
            get { return new Guid(); }
        }

        public void SetId(Guid? newId)
        {
            throw new NotImplementedException();
        }

    	public void ClearId()
    	{
    		throw new NotImplementedException();
    	}

    	#endregion

        #region IEquatable<IEntity> Members

        public bool Equals(IEntity other)
        {
            return true;
        }

        #endregion
    }
}
