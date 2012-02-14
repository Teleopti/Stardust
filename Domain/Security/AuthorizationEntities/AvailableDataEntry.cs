using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{

    /// <summary>
    /// Holds one entry for the available data.
    /// </summary>
    public class AvailableDataEntry : Entity, IAvailableDataEntry
    {

        #region Variables

        private string _availableDataHolderKey;
        private string _availableDataHolderName;
        private string _availableDataHolderDescription;
        private string _availableDataHolderValue;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableDataEntry"/> class.
        /// </summary>
        public AvailableDataEntry()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableDataEntry"/> class.
        /// </summary>
        /// <param name="authorizationEntity">The authorization entity.</param>
        public AvailableDataEntry(IAuthorizationEntity authorizationEntity)
        {
            _availableDataHolderKey = authorizationEntity.AuthorizationKey;
            _availableDataHolderName = authorizationEntity.AuthorizationName;
            _availableDataHolderDescription = authorizationEntity.AuthorizationDescription;
            _availableDataHolderValue = authorizationEntity.AuthorizationValue;
        }

        #endregion

        #region Interface

        #region IAuthorizationEntity Members

        /// <summary>
        /// Gets the authorization unique key. This must be unique and that is used for any
        /// comparisons within the authorization related stuff.
        /// </summary>
        /// <value>The authorization key.</value>
        public string AuthorizationKey
        {
            get { return AvailableDataHolderKey; }
        }

        /// <summary>
        /// Gets the Name value. Usually this is the key.
        /// </summary>
        /// <value>The name field.</value>
        public string AuthorizationName
        {
            get { return AvailableDataHolderName; }
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
            get { return AvailableDataHolderDescription; }
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
            get { return AvailableDataHolderValue; }
        }

        #endregion

        /// <summary>
        /// Gets or sets the key of the available data holder.
        /// </summary>
        /// <value>The name of the available data holder.</value>
        public string AvailableDataHolderKey
        {
            get { return _availableDataHolderKey; }
            set { _availableDataHolderKey = value; }
        }

        /// <summary>
        /// Gets or sets the name of the available data holder.
        /// </summary>
        /// <value>The name of the available data holder.</value>
        public string AvailableDataHolderName
        {
            get { return _availableDataHolderName; }
            set { _availableDataHolderName = value; }
        }

        /// <summary>
        /// Gets or sets the available data holder description.
        /// </summary>
        /// <value>The available data holder description.</value>
        public string AvailableDataHolderDescription
        {
            get { return _availableDataHolderDescription; }
            set { _availableDataHolderDescription = value; }
        }

        /// <summary>
        /// Gets or sets the available data holder value.
        /// </summary>
        /// <value>The available data holder value.</value>
        public string AvailableDataHolderValue
        {
            get { return _availableDataHolderValue; }
            set { _availableDataHolderValue = value; }
        }

        #endregion
    }
}
