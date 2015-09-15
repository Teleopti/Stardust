using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class IdUserQueryFake : IIdUserQuery
	{
		private readonly IDictionary<Guid, PersonInfo> data = new Dictionary<Guid, PersonInfo>();

		public void Has(PersonInfo personInfo)
		{
			data[personInfo.Id] = personInfo;
		}

		public PersonInfo FindUserData(Guid id)
		{
			PersonInfo ret;
			return data.TryGetValue(id, out ret) ? ret : null;
		}
	}
}