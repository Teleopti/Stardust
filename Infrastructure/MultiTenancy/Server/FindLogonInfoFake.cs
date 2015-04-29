using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindLogonInfoFake : IFindLogonInfo
	{
		private readonly IDictionary<Guid, LogonInfo> data = new Dictionary<Guid, LogonInfo>();

		public IEnumerable<LogonInfo> GetForIds(IEnumerable<Guid> ids)
		{
			return (from id in ids where data.ContainsKey(id) select data[id]).ToList();
		}

		public void Add(LogonInfo logonInfo)
		{
			data[logonInfo.PersonId] = logonInfo;
		}
	}
}