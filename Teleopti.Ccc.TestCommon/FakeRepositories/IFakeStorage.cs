using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public interface IFakeStorage
	{
		void Add(IAggregateRoot entity);
		void Remove(IAggregateRoot entity);
		T Get<T>(Guid id) where T : IAggregateRoot;
		IEnumerable<T> LoadAll<T>();
		T Merge<T>(T root) where T : class, IAggregateRoot;
		IEnumerable<IRootChangeInfo> Commit();
	}
}