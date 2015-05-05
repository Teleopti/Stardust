using System;
using System.Collections.Generic;
using System.IO;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	public interface ITenantPeopleSaver
	{
		void SaveTenantData(PersonDto personDto, Guid id, string tenant);
	}
	public class TenantPeopleSaver : ITenantPeopleSaver
	{
		private readonly ITenantDataManager _tenantDataManager;

		public TenantPeopleSaver(ITenantDataManager tenantDataManager)
		{
			_tenantDataManager = tenantDataManager;
		}

		public void SaveTenantData(PersonDto personDto, Guid id, string tenant)
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
#pragma warning restore 618
				PersonId = id,
				Tenant = tenant
			};
			if (!string.IsNullOrEmpty(personDto.Identity))
				data.Identity = personDto.Identity;

			if (personDto.TerminationDate != null)
				data.TerminalDate = personDto.TerminationDate.DateTime;
			
			var result = _tenantDataManager.SaveTenantData(data);
			if(!result.Success)
				throw new InvalidDataException(result.FailReason);
		}
	}

}