using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class FindLogonInfoFake : IFindLogonInfo
	{
		private readonly IDictionary<Guid, LogonInfo> data = new Dictionary<Guid, LogonInfo>();

		public IEnumerable<LogonInfo> GetForIds(IEnumerable<Guid> ids)
		{
			return (from id in ids where data.ContainsKey(id) select data[id]).ToList();
		}

		public LogonInfo GetForLogonName(string logonName)
		{
			return data.Values.Single(x => x.LogonName == logonName);
		}

		public LogonInfo GetForIdentity(string identity)
		{
			return data.Values.Single(x => x.Identity == identity);
		}

		public void Add(LogonInfo logonInfo)
		{
			data[logonInfo.PersonId] = logonInfo;
		}
	}
}