using System;
using System.Collections.Generic;
using System.IO;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.MultiTenancy
{
	public interface ITenantPeopleSaver
	{
		void SaveTenantData(PersonDto personDto, Guid id);
	}
	public class TenantPeopleSaver : ITenantPeopleSaver
	{
		private readonly ITenantDataManagerClient _tenantDataManager;

		public TenantPeopleSaver(ITenantDataManagerClient tenantDataManager)
		{
			_tenantDataManager = tenantDataManager;
		}

		public void SaveTenantData(PersonDto personDto, Guid id)
		{
			if (personDto.IsDeleted)
			{
				_tenantDataManager.DeleteTenantPersons(new List<Guid>{id});
				return;
			}
			
			var data = new TenantAuthenticationData
			{
				ApplicationLogonName = personDto.ApplicationLogOnName,
				Password = personDto.ApplicationLogOnPassword,
#pragma warning disable 618
				Identity = !string.IsNullOrEmpty(personDto.WindowsDomain) && !string.IsNullOrEmpty(personDto.WindowsLogOnName) ? personDto.WindowsDomain + "\\" + personDto.WindowsLogOnName : null,
				PersonId = id
			};
			if (string.IsNullOrEmpty(data.Identity) && !string.IsNullOrEmpty(personDto.WindowsLogOnName))
				data.Identity = personDto.WindowsLogOnName;
#pragma warning restore 618
			if (!string.IsNullOrEmpty(personDto.Identity))
				data.Identity = personDto.Identity;
			
			var result = _tenantDataManager.SaveTenantData(data);
			if(!result.Success)
				throw new InvalidDataException(result.FailReason);
		}
	}

}