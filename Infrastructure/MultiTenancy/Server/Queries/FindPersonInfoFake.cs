using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class FindPersonInfoFake : IFindPersonInfo
	{
		private readonly IDictionary<Guid, PersonInfo> data = new Dictionary<Guid, PersonInfo>();

		public PersonInfo GetById(Guid id)
		{
			return data.TryGetValue(id, out var ret) ? ret : null;
		}

		public void Add(PersonInfo personInfo)
		{
			data[personInfo.Id] = personInfo;
		}
	}
}