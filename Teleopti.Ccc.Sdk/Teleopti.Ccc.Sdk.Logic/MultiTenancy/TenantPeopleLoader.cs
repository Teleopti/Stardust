using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.MultiTenancy
{
	public interface ITenantPeopleLoader
	{
		void FillDtosWithLogonInfo(IList<PersonDto> personDtos);
	}

	public class TenantPeopleLoader : ITenantPeopleLoader
	{
		private readonly ITenantLogonDataManagerClient _tenantLogonDataManager;

		public TenantPeopleLoader(ITenantLogonDataManagerClient tenantLogonDataManager)
		{
			_tenantLogonDataManager = tenantLogonDataManager;
		}

		public void FillDtosWithLogonInfo(IList<PersonDto> personDtos)
		{
			var guids = personDtos.Select(personDto => personDto.Id.GetValueOrDefault()).ToList();
			var infos =_tenantLogonDataManager.GetLogonInfoModelsForGuids(guids);
			foreach (var logonInfoModel in infos)
			{
				var dto = personDtos.FirstOrDefault(personDto => personDto.Id.Equals(logonInfoModel.PersonId));
				if (dto != null)
				{
					dto.ApplicationLogOnName = logonInfoModel.LogonName;
					dto.ApplicationLogOnPassword = ""; //this is encrypted anyway and we don't return it here
					if(!string.IsNullOrEmpty(logonInfoModel.Identity))
					{
#pragma warning disable 618
						if (logonInfoModel.Identity.Contains(@"\"))
						{
							var identities = IdentityHelper.Split(logonInfoModel.Identity);
							dto.WindowsDomain = identities.Item1;
							dto.WindowsLogOnName = identities.Item2;
						}
						else
						{
							dto.WindowsDomain = "";
							dto.WindowsLogOnName = "";
						}
#pragma warning restore 618
						dto.Identity = logonInfoModel.Identity;
					}
				}
			}
		}
	}
}