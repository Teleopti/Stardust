using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// SystemRole - ApplicationRole mapper entity provider.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/27/2007
    /// </remarks>
    public class StaticAvailableDataProvider : IAuthorizationEntityProvider<IAvailableDataEntry>
    {
        private readonly IAvailableDataRepository _availableDataRep;
        private IList<IApplicationRole> _inputApplicationRoles;
        private IList<IAvailableData> _staticAvailableData;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticAvailableDataProvider"/> class.
        /// </summary>
        /// <param name="availableDataRepository">The available data repository.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public StaticAvailableDataProvider(IAvailableDataRepository availableDataRepository)
        {
            _availableDataRep = availableDataRepository;
        }

        #region Interface

        #region IAuthorizationEntityProvider<AvailableDataEntry> Members

        /// <summary>
        /// Gets the result entity list.
        /// </summary>
        /// <value>The result entity list.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IAvailableDataEntry> ResultEntityList
        {
            get
            {
                IList<IAvailableDataEntry> returnList = new List<IAvailableDataEntry>();
                IEnumerable<IAvailableData> filteredAvailableData = InputApplicationRoles.Select(a => a.AvailableData).Where(a => a != null);
                foreach (IAvailableData availableData in filteredAvailableData)
                {
                    
                    _availableDataRep.LoadAllCollectionsInAvailableData(availableData);
                    foreach (IAvailableDataEntry availableDataEntry in availableData.ConvertToPermittedDataEntryCollection())
                    {
                        returnList.Add(availableDataEntry);
                    }
                }
                _staticAvailableData = new List<IAvailableData>(filteredAvailableData);
                return returnList;
            }
        }

        /// <summary>
        /// Sets the parent entity list.
        /// </summary>
        /// <value>The parent entity list.</value>
        /// <remarks>
        /// Used for setting the parent result when needed for the operation.
        /// </remarks>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IAuthorizationEntity> InputEntityList
        {
            set
            {
                _inputApplicationRoles = AuthorizationEntityExtender.ConvertToSpecificList<IApplicationRole>(value);
            }
        }

        /// <summary>
        /// Gets the input application roles.
        /// </summary>
        /// <value>The input application roles.</value>
        protected virtual IList<IApplicationRole> InputApplicationRoles
        {
            get { return _inputApplicationRoles; }
        }

        #endregion

        /// <summary>
        /// Gets the static available data.
        /// </summary>
        /// <value>Static available data.</value>
        public IList<IAvailableData> StaticAvailableData
        {
            get { return _staticAvailableData; }
        }

        #endregion
    }
}
