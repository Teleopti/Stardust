using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfHost.Service.LogOn;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
	public interface IAuthenticationFactory
	{
		AuthenticationResultDto LogOnWindows(DataSourceDto dataSource);
		ICollection<DataSourceDto> GetDataSources();
		AuthenticationResultDto LogOnApplication(string userName, string password, DataSourceDto dataSource);
	}

	public class MultiTenancyAuthenticationFactory : IAuthenticationFactory
	{
		private readonly IAssembler<AuthenticationQuerierResult, AuthenticationResultDto> _authenticationResultAssembler;
		private readonly IAuthenticationTenantClient _authenticationQuerier;
		private readonly IWindowsUserProvider _windowsUserProvider;
		private readonly PersonCache _personCache = new PersonCache();
		public const string UserAgent = "SDK";

		public MultiTenancyAuthenticationFactory(IAssembler<AuthenticationQuerierResult, AuthenticationResultDto> authenticationResultAssembler,
			IAuthenticationTenantClient authenticationQuerier, IWindowsUserProvider windowsUserProvider)
		{
			_authenticationResultAssembler = authenticationResultAssembler;
			_authenticationQuerier = authenticationQuerier;
			_windowsUserProvider = windowsUserProvider;
		}
		
		public AuthenticationResultDto LogOnWindows(DataSourceDto dataSource)
		{
			var identity = _windowsUserProvider.Identity();
			var result = _authenticationQuerier.TryLogon(new IdentityLogonClientModel { Identity = identity }, UserAgent);
			var resultDto = _authenticationResultAssembler.DomainEntityToDto(result);
			if (!resultDto.Successful) return resultDto;

			resultDto.Tenant = result.DataSource.DataSourceName;
			var buList = new AvailableBusinessUnitsProvider(new RepositoryFactory()).AvailableBusinessUnits(result.Person, result.DataSource);
			buList.ForEach(unit => resultDto.BusinessUnitCollection.Add(new BusinessUnitDto { Id = unit.Id, Name = unit.Name }));
			addToCache(result, identity, null);
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
		
		public AuthenticationResultDto LogOnApplication(string userName, string password, DataSourceDto dataSource)
		{
			var result = _authenticationQuerier.TryLogon(new ApplicationLogonClientModel {UserName = userName, Password = password}, UserAgent);
			var resultDto = _authenticationResultAssembler.DomainEntityToDto(result);

			if (!resultDto.Successful) return resultDto;
			resultDto.Tenant = result.DataSource.DataSourceName;
			var buList = new AvailableBusinessUnitsProvider(new RepositoryFactory()).AvailableBusinessUnits(result.Person, result.DataSource);
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
