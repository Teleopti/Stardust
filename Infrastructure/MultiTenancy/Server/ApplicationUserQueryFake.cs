﻿using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class ApplicationUserQueryFake : IApplicationUserQuery
	{
		private readonly IDictionary<string, PersonInfo> data = new Dictionary<string, PersonInfo>();

		public PersonInfo Find(string username)
		{
			PersonInfo ret;
			return data.TryGetValue(username, out ret) ? ret : null;
		}

		public void Has(PersonInfo personInfo)
		{
			data[personInfo.ApplicationLogonInfo.LogonName] = personInfo;
		}
	}
}