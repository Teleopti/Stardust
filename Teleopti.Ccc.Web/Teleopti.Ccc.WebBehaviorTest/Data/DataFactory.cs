using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class DataFactory
	{
		private readonly Action<Action<IUnitOfWork>> _unitOfWorkActionAction;
		private readonly IList<IDataSetup> _dataSetups = new List<IDataSetup>();

		public DataFactory(Action<Action<IUnitOfWork>> unitOfWorkActionAction) {
			_unitOfWorkActionAction = unitOfWorkActionAction;
		}

		public void Setup(IDataSetup dataSetup)
		{
			_dataSetups.Add(dataSetup);
		}

		public void Apply()
		{
			_dataSetups.ForEach(s => _unitOfWorkActionAction.Invoke(s.Apply));
		}

		public void Clear()
		{
			_dataSetups.Clear();
		}

		private IEnumerable<T> QueryData<T>() { return from s in _dataSetups where typeof(T).IsAssignableFrom(s.GetType()) select (T)s; }

		public IEnumerable<IDataSetup> Setups { get { return _dataSetups; } }
		public bool HasSetup<T>() { return QueryData<T>().Any(); }
		public T Data<T>() { return QueryData<T>().SingleOrDefault(); }

	}
}