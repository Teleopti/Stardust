using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Dynamic available data provider. That is the available data part that is loaded
    /// by dynamically according to the available data range property of the application roles
    /// that the person has, and the current person's current membership in the hierarchy 
    /// (what is the current team, site, business unit the person belongs to at the time
    /// of the check).
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 04/24/2008
    /// </remarks>
    public class DynamicAvailableDataProvider : IAuthorizationEntityProvider<IAvailableDataEntry>
    {
        private readonly IAvailableDataRepository _availableDataRep;
        private readonly IBusinessUnitRepository _businessUnitRepository;
        private IList<IApplicationRole> _inputApplicationRoles;
        private readonly IPerson _person;
        private readonly IBusinessUnit _currentBusinessUnit;
        private IList<IAvailableData> _dynamicAvailableDataList;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticAvailableDataProvider"/> class.
        /// </summary>
        /// <param name="availableDataRepository">The available data repository.</param>
        /// <param name="businessUnitRepository">The business unit repository.</param>
        /// <param name="person">The person.</param>
        /// <param name="currentBusinessUnit">The business unit.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public DynamicAvailableDataProvider(IAvailableDataRepository availableDataRepository, IBusinessUnitRepository businessUnitRepository, IPerson person, IBusinessUnit currentBusinessUnit)
        {
            _availableDataRep = availableDataRepository;
            _businessUnitRepository = businessUnitRepository;
            _person = person;
            _currentBusinessUnit = currentBusinessUnit;
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
                IList<IAvailableData> dynamicAvailableDataList = new List<IAvailableData>();
                foreach (IAvailableData availableData in filteredAvailableData)
                {
                    _availableDataRep.LoadAllCollectionsInAvailableData(availableData);
                    IAvailableData dynamicAvailableData = CreateDynamicAvailableData(availableData.AvailableDataRange, availableData.ApplicationRole, _person, _currentBusinessUnit, _businessUnitRepository);
                    dynamicAvailableDataList.Add(dynamicAvailableData);
                    foreach (AvailableDataEntry availableDataEntry in dynamicAvailableData.ConvertToPermittedDataEntryCollection())
                    {
                        if (!returnList.Contains(availableDataEntry))
                            returnList.Add(availableDataEntry);
                    }
                }
                _dynamicAvailableDataList = dynamicAvailableDataList;
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
        /// Gets all available data.
        /// </summary>
        /// <value>All available data.</value>
        public IList<IAvailableData> DynamicAvailableData
        {
            get { return _dynamicAvailableDataList; }
        }

        #endregion

        #region Local

        /// <summary>
        /// Adds the dynamic available data.
        /// </summary>
        /// <param name="availableDataRange">The available data range.</param>
        /// <param name="applicationRole">The application role.</param>
        /// <param name="person">The person.</param>
        /// <param name="businessUnit">The business unit.</param>
        /// <param name="businessUnitRepository">The business unit repository.</param>
        /// <returns></returns>
        protected virtual IAvailableData CreateDynamicAvailableData(AvailableDataRangeOption availableDataRange, IApplicationRole applicationRole, IPerson person, IBusinessUnit businessUnit, IBusinessUnitRepository businessUnitRepository)
        {
            IAvailableData returnData = new AvailableData();
            switch (availableDataRange)
            {
                case AvailableDataRangeOption.MyOwn:
                    returnData = AddMyOwn(returnData, person);
                    break;
                case AvailableDataRangeOption.MyTeam:
                    returnData = AddMyTeam(returnData, person);
                    break;
                case AvailableDataRangeOption.MySite:
                    returnData = AddMySite(returnData, person);
                    break;
                case AvailableDataRangeOption.MyBusinessUnit:
                    returnData = AddMyBusinessUnit(returnData, businessUnit);
                    break;
                case AvailableDataRangeOption.Everyone:
                    returnData = AddEveryBusinessUnits(returnData, businessUnitRepository);
                    break;
            }
            returnData.ApplicationRole = applicationRole;
            return returnData;
        }

        /// <summary>
        /// Adds my own data.
        /// </summary>
        /// <param name="dynamicAvailableData">The dynamic available data.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        protected virtual IAvailableData AddMyOwn(IAvailableData dynamicAvailableData, IPerson person)
        {
            dynamicAvailableData.AddAvailablePerson(person);
            return dynamicAvailableData;
        }

        /// <summary>
        /// Adds my team as available data.
        /// </summary>
        /// <param name="dynamicAvailableData">The dynamic available data.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        protected virtual IAvailableData AddMyTeam(IAvailableData dynamicAvailableData, IPerson person)
        {
            ITeam team = person.MyTeam(DateOnly.Today);
            if(team != null)
            {
                dynamicAvailableData.AddAvailableTeam(team);
            }
            return dynamicAvailableData;
        }

        /// <summary>
        /// Adds my site as available data.
        /// </summary>
        /// <param name="dynamicAvailableData">The dynamic available data.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        protected virtual IAvailableData AddMySite(IAvailableData dynamicAvailableData, IPerson person)
        {
            ITeam team = person.MyTeam(DateOnly.Today);
            if (team != null && team.Site != null)
            {
                dynamicAvailableData.AddAvailableSite(team.Site);
            }
            return dynamicAvailableData;
        }

        /// <summary>
        /// Adds my business unit as available data.
        /// </summary>
        /// <param name="dynamicAvailableData">The dynamic available data.</param>
        /// <param name="businessUnit">The business unit.</param>
        /// <returns></returns>
        protected virtual IAvailableData AddMyBusinessUnit(IAvailableData dynamicAvailableData, IBusinessUnit businessUnit)
        {
            dynamicAvailableData.AddAvailableBusinessUnit(businessUnit);
            return dynamicAvailableData;
        }

        /// <summary>
        /// Adds my business unit as available data.
        /// </summary>
        /// <param name="dynamicAvailableData">The dynamic available data.</param>
        /// <param name="businessUnitRepository">The business unit repository.</param>
        /// <returns></returns>
        protected virtual IAvailableData AddEveryBusinessUnits(IAvailableData dynamicAvailableData, IBusinessUnitRepository businessUnitRepository)
        {
            IList<IBusinessUnit> loadedBusinessUnits = _businessUnitRepository.LoadAllBusinessUnitSortedByName();
            foreach (IBusinessUnit businessUnit in loadedBusinessUnits)
            {
                dynamicAvailableData.AddAvailableBusinessUnit(businessUnit);
            }
            return dynamicAvailableData;
        }

        #endregion

    }
}
