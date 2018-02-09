using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IRepository<T>
	{
		void Add(T root);
		void Remove(T root);
		T Get(Guid id);
		T Load(Guid id);
		IEnumerable<T> LoadAll();
	}
}