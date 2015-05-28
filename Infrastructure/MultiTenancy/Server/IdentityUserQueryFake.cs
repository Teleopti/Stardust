using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class IdentityUserQueryFake : IIdentityUserQuery
	{
		private readonly IDictionary<string, PersonInfo> data = new Dictionary<string, PersonInfo>();

		public void Has(PersonInfo personInfo)
		{
			data[personInfo.Identity] = personInfo;
		}

		public PersonInfo FindUserData(string identity)
		{
			PersonInfo ret;
			return data.TryGetValue(identity, out ret) ? ret : null;
		}
	}
}