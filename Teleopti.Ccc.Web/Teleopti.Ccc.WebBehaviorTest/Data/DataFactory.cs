using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class DataFactory
	{
		private readonly IList<IDataSetup> _dataSetups = new List<IDataSetup>();

		public void Setup(IDataSetup dataSetup)
		{
			_dataSetups.Add(dataSetup);
		}

		public void Persist()
		{
			TestDataSetup.UnitOfWorkAction(uow => _dataSetups.ForEach(s => s.Apply(uow)));
		}

		private IEnumerable<T> QueryData<T>() { return from s in _dataSetups where typeof(T).IsAssignableFrom(s.GetType()) select (T)s; }

		public IEnumerable<IDataSetup> Setups { get { return _dataSetups; } }
		public bool HasSetup<T>() { return QueryData<T>().Any(); }
		public T Data<T>() { return QueryData<T>().SingleOrDefault(); }

	}
}