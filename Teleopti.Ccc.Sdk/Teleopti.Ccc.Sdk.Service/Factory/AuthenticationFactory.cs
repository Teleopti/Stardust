using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    /// <summary>
    /// Later on we can probably load this using AutoFac instead and get rid of some of the code. /Robin
    /// </summary>
    public class AuthenticationFactory
    {
        private readonly IEnumerable<IDataSourceProvider> _dataSourceProviders;
        private readonly IAssembler<AuthenticationResult, AuthenticationResultDto> _authenticationResultAssembler;
        private readonly LicenseFactory _licenseFactory;
        private readonly ILicenseCache _licenseCache;

        public AuthenticationFactory(IEnumerable<IDataSourceProvider> dataSourceProviders, IAssembler<AuthenticationResult,AuthenticationResultDto> authenticationResultAssembler, LicenseFactory licenseFactory, ILicenseCache licenseCache)
        {
            _dataSourceProviders = dataSourceProviders;
            _authenticationResultAssembler = authenticationResultAssembler;
            _licenseFactory = licenseFactory;
            _licenseCache = licenseCache;
        }

        public void SetBusinessUnit(BusinessUnitDto businessUnit)
        {
            //Must do a check for license here the first time the service is started
            if (_licenseCache.Get()==null)
            {
                _licenseFactory.VerifyLicense();
            }
        }

        public AuthenticationResultDto LogOnWindows(DataSourceDto dataSource)
        {
            AuthenticationResultDto authenticationResultDto = new AuthenticationResultDto {Successful = false};
            try
            {
                var dataSourceContainers = DataSourceContainers();
                var dataSourceContainer =
                    dataSourceContainers.FirstOrDefault(d => d.DataSource.Application.Name == dataSource.Name &&
                                                             d.AuthenticationTypeOption == AuthenticationTypeOption.Windows);

                if (dataSourceContainer != null)
                {
                    IEnumerable<IBusinessUnit> buList =
                        dataSourceContainer.AvailableBusinessUnitProvider.AvailableBusinessUnits();
                    authenticationResultDto.Successful = true;
                    buList.ForEach(unit => authenticationResultDto.BusinessUnitCollection.Add(new BusinessUnitDto(unit)));
                }
            }
            catch
            {
                authenticationResultDto.HasMessage = true;
                authenticationResultDto.Message = UserTexts.Resources.LogOnFailedInvalidUserNameOrPassword;
            }
            return authenticationResultDto;
        }

        public ICollection<DataSourceDto> GetDataSources()
        {
            var dataSourceList = DataSourceContainers();

            IList<DataSourceDto> dataSources = new List<DataSourceDto>();
            foreach (DataSourceContainer dataSource in dataSourceList)
            {
                dataSources.Add(new DataSourceDto(dataSource.DataSource,
                                                  dataSource.AuthenticationTypeOption ==
                                                  AuthenticationTypeOption.Windows
                                                      ? AuthenticationTypeOptionDto.Windows
                                                      : AuthenticationTypeOptionDto.Application));
            }
            return dataSources;
        }

        public IEnumerable<DataSourceContainer> DataSourceContainers()
        {
            List<DataSourceContainer> dataSourceContainers = new List<DataSourceContainer>();
            foreach (IDataSourceProvider provider in _dataSourceProviders)
            {
                dataSourceContainers.AddRange(provider.DataSourceList());
            }
            return dataSourceContainers;
        }

        public void TransferSession(SessionDataDto sessionDataDto)
        {
            var authenticationType = sessionDataDto.AuthenticationType == AuthenticationTypeOptionDto.Windows
                                         ? AuthenticationTypeOption.Windows
                                         : AuthenticationTypeOption.Application;
            var dataSourceContainers = DataSourceContainers();
            var dataSourceContainer =
                dataSourceContainers.FirstOrDefault(d => d.DataSource.Application.Name == sessionDataDto.DataSource.Name &&
                                                         d.AuthenticationTypeOption == authenticationType);
            if (authenticationType==AuthenticationTypeOption.Application)
            {
                var result = dataSourceContainer.LogOn(sessionDataDto.LoggedOnPerson.ApplicationLogOnName,
                                                       sessionDataDto.LoggedOnPassword);
                var resultDto = _authenticationResultAssembler.DomainEntityToDto(result);
                if (!resultDto.Successful)
                {
                    throw new FaultException("The supplied log on name could not be used.");
                }
            }

            if (dataSourceContainer == null)
            {
                throw new FaultException("The requested data source is not available");
            }

            IEnumerable<IBusinessUnit> buList = dataSourceContainer.AvailableBusinessUnitProvider.AvailableBusinessUnits();
            IBusinessUnit businessUnit = buList.FirstOrDefault(b => b.Id.Value == sessionDataDto.BusinessUnit.Id.Value);
            if (businessUnit == null)
            {
                throw new FaultException("The requested business unit is not available");
            }

            SetBusinessUnit(new BusinessUnitDto(businessUnit));
        }

        public AuthenticationResultDto LogOnApplication(string userName, string password, DataSourceDto dataSource)
        {
            var dataSourceContainers = DataSourceContainers();
            var dataSourceContainer =
                dataSourceContainers.FirstOrDefault(d => d.DataSource.Application.Name == dataSource.Name &&
                                                         d.AuthenticationTypeOption == AuthenticationTypeOption.Application);

            AuthenticationResultDto resultDto;
            if (dataSourceContainer != null)
            {
                var result = dataSourceContainer.LogOn(userName, password);
                resultDto = _authenticationResultAssembler.DomainEntityToDto(result);
                if (resultDto.Successful)
                {
                    IEnumerable<IBusinessUnit> buList =
                        dataSourceContainer.AvailableBusinessUnitProvider.AvailableBusinessUnits();
                    buList.ForEach(unit => resultDto.BusinessUnitCollection.Add(new BusinessUnitDto(unit)));
                }
            }
            else
            {
                resultDto = new AuthenticationResultDto
                                {
                                    Successful = false,
                                    HasMessage = true,
                                    Message = UserTexts.Resources.NoAvailableDataSourcesHasBeenFound
                                };
            }

            return resultDto;
        }
    }
}
