using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeTenantPersonLogonQuerier : ITenantPersonLogonQuerier
	{
		private readonly IList<PersonInfoModel> _personInfoModels;

		public FakeTenantPersonLogonQuerier(IList<PersonInfoModel> personInfoModels)
		{
			_personInfoModels = personInfoModels;
		}

		public void Add(PersonInfoModel personInfoModel)
		{
			_personInfoModels.Add(personInfoModel);
		}

		public IEnumerable<IPersonInfoModel> FindApplicationLogonUsers(IEnumerable<string> logonNames)
		{
			return logonNames.Select(logonName => _personInfoModels.FirstOrDefault(x => x.ApplicationLogonName == logonName)).ToList();
		}
	}
}
