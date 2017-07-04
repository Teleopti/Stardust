using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeQueuedOvertimeRequestRepository : IQueuedOvertimeRequestRepository
	{
		private readonly IList<IQueuedOvertimeRequest> storage = new List<IQueuedOvertimeRequest>();

		public void Add(IQueuedOvertimeRequest root)
		{
			storage.Add(root);
		}

		public IQueuedOvertimeRequest Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IQueuedOvertimeRequest Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IQueuedOvertimeRequest> LoadAll()
		{
			throw new NotImplementedException();
		}

		public void Remove(IQueuedOvertimeRequest root)
		{
			throw new NotImplementedException();
		}
	}
}