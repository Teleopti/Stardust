using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Configuration
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