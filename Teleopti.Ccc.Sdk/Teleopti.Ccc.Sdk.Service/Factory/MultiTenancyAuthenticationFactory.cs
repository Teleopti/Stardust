using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfService.LogOn;
using Teleopti.Interfaces.Domain;

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
		private readonly IAssembler<AuthenticationResult, AuthenticationResultDto> _authenticationResultAssembler;
		private readonly IMultiTenancyApplicationLogon _multiTenancyApplicationLogon;
		private readonly IMultiTenancyWindowsLogon _multiTenancyWindowsLogon;
		private readonly PersonCache _personCache = new PersonCache();
		public const string UserAgent = "SDK";

		public MultiTenancyAuthenticationFactory(IAssembler<AuthenticationResult, AuthenticationResultDto> authenticationResultAssembler,
			IMultiTenancyApplicationLogon multiTenancyApplicationLogon, IMultiTenancyWindowsLogon multiTenancyWindowsLogon)
		{
			_authenticationResultAssembler = authenticationResultAssembler;
			_multiTenancyApplicationLogon = multiTenancyApplicationLogon;
			_multiTenancyWindowsLogon = multiTenancyWindowsLogon;
		}

		public void SetBusinessUnit(BusinessUnitDto businessUnit)
		{
			//not supported anymore
		}

		public AuthenticationResultDto LogOnWindows(DataSourceDto dataSource)
		{
			var model = new LogonModel();
			var result = _multiTenancyWindowsLogon.Logon(model, StateHolderReader.Instance.StateReader.ApplicationScopeData,
				UserAgent);

			var resultDto = _authenticationResultAssembler.DomainEntityToDto(result);

			if (!resultDto.Successful) return resultDto;
			resultDto.Tenant = model.SelectedDataSourceContainer.DataSourceName;
			var buList = model.SelectedDataSourceContainer.AvailableBusinessUnitProvider.AvailableBusinessUnits();
			buList.ForEach(unit => resultDto.BusinessUnitCollection.Add(new BusinessUnitDto { Id = unit.Id, Name = unit.Name }));
			addToCache(result, model);
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
			var model = new LogonModel { UserName = userName, Password = password };
			var result = _multiTenancyApplicationLogon.Logon(model, UserAgent);

			var resultDto = _authenticationResultAssembler.DomainEntityToDto(result);

			if (!resultDto.Successful) return resultDto;
			resultDto.Tenant = model.SelectedDataSourceContainer.DataSourceName;
			var buList = model.SelectedDataSourceContainer.AvailableBusinessUnitProvider.AvailableBusinessUnits();
			buList.ForEach(unit => resultDto.BusinessUnitCollection.Add(new BusinessUnitDto { Id = unit.Id, Name = unit.Name }));
			addToCache(result, model);
			return resultDto;
		}

		private void addToCache(AuthenticationResult result, LogonModel model)
		{
			var personContainer = new PersonContainer(result.Person)
			{
				DataSource = model.SelectedDataSourceContainer.DataSourceName,
				Password = model.Password,
				UserName = model.UserName
			};
			_personCache.Add(personContainer);
		}
	}
}
