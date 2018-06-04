using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExtensiveLogRepository : IExtensiveLogRepository
	{
		private readonly IList<ExtensiveLog> _extensiveLogs =new List<ExtensiveLog>();
		
		public void Add(object obj, Guid objId, string entityType)
		{
			_extensiveLogs.Add(new ExtensiveLog()
			{
				Id = objId
			});
		}

		public IList<ExtensiveLog> LoadAll()
		{
			return _extensiveLogs;
		}
		
	}
}