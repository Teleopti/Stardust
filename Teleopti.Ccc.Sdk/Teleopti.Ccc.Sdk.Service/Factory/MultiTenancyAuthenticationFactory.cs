using System.Collections.Generic;
using System.IdentityModel.Configuration;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfService.LogOn;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	public interface IAuthenticationFactory
	{
		void SetBusinessUnit(BusinessUnitDto businessUnit);
		AuthenticationResultDto LogOnWindows(DataSourceDto dataSource);
		ICollection<DataSourceDto> GetDataSources();
		IEnumerable<DataSourceContainer> DataSourceContainers();
		void TransferSession(SessionDataDto sessionDataDto);
		AuthenticationResultDto LogOnApplication(string userName, string password, DataSourceDto dataSource);
	}

	public class MultiTenancyAuthenticationFactory : IAuthenticationFactory
	{
		private readonly IAssembler<AuthenticationQuerierResult, AuthenticationResultDto> _authenticationResultAssembler;
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly IWindowsUserProvider _windowsUserProvider;
		private readonly PersonCache _personCache = new PersonCache();
		public const string UserAgent = "SDK";

		public MultiTenancyAuthenticationFactory(IAssembler<AuthenticationQuerierResult, AuthenticationResultDto> authenticationResultAssembler,
			IAuthenticationQuerier authenticationQuerier, IWindowsUserProvider windowsUserProvider)
		{
			_authenticationResultAssembler = authenticationResultAssembler;
			_authenticationQuerier = authenticationQuerier;
			_windowsUserProvider = windowsUserProvider;
		}

		public void SetBusinessUnit(BusinessUnitDto businessUnit)
		{
			//not supported anymore
		}

		public AuthenticationResultDto LogOnWindows(DataSourceDto dataSource)
		{
			var result = _authenticationQuerier.TryLogon(new IdentityLogonClientModel { Identity = _windowsUserProvider.Identity() }, UserAgent);
			var resultDto = _authenticationResultAssembler.DomainEntityToDto(result);
			if (!resultDto.Successful) return resultDto;

			resultDto.Tenant = result.DataSource.DataSourceName;
			var buList = new AvailableBusinessUnitsProvider(result.Person, result.DataSource).AvailableBusinessUnits(new RepositoryFactory());
			buList.ForEach(unit => resultDto.BusinessUnitCollection.Add(new BusinessUnitDto { Id = unit.Id, Name = unit.Name }));
			addToCache(result, null, null);
			return resultDto;
		}

		public ICollection<DataSourceDto> GetDataSources()
		{
			//return some fake ones for backward comp.
			return new List<DataSourceDto>
			{
				new DataSourceDto {Name = "Teleopti WFM", AuthenticationTypeOptionDto = AuthenticationTypeOptionDto.Application},
				new DataSourceDto {Name = "Teleopti WFM", AuthenticationTypeOptionDto = AuthenticationTypeOptionDto.Windows}
			};
		}

		public IEnumerable<DataSourceContainer> DataSourceContainers()
		{
			//just return an empty list
			return new List<DataSourceContainer>();
		}

		public void TransferSession(SessionDataDto sessionDataDto)
		{
			//not supported anymore
		}

		public AuthenticationResultDto LogOnApplication(string userName, string password, DataSourceDto dataSource)
		{
			var result = _authenticationQuerier.TryLogon(new ApplicationLogonClientModel {UserName = userName, Password = password}, UserAgent);
			var resultDto = _authenticationResultAssembler.DomainEntityToDto(result);

			if (!resultDto.Successful) return resultDto;
			resultDto.Tenant = result.DataSource.DataSourceName;
			var buList = new AvailableBusinessUnitsProvider(result.Person, result.DataSource).AvailableBusinessUnits(new RepositoryFactory());
			buList.ForEach(unit => resultDto.BusinessUnitCollection.Add(new BusinessUnitDto { Id = unit.Id, Name = unit.Name }));
			addToCache(result, userName, password);
			return resultDto;
		}

		private void addToCache(AuthenticationQuerierResult result, string userName, string password)
		{
			var personContainer = new PersonContainer(result.Person, userName, password,
				result.DataSource.DataSourceName, result.TenantPassword);
			_personCache.Add(personContainer);
		}
	}
}
