using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class DataFactory
	{
		private readonly Action<Action<IUnitOfWork>> _unitOfWorkAction;
		private readonly IList<IDataSetup> _dataSetups = new List<IDataSetup>();

		public DataFactory(Action<Action<IUnitOfWork>> unitOfWorkAction) {
			_unitOfWorkAction = unitOfWorkAction;
		}

		public void Setup(IDataSetup setup)
		{
			_dataSetups.Add(setup);
		}

		public IEnumerable<IDataSetup> Setups { get { return _dataSetups; } }

		public void Persist()
		{
			_dataSetups.ForEach(s => _unitOfWorkAction.Invoke(s.Apply));
		}

		private IEnumerable<T> QueryData<T>() { return from s in _dataSetups where typeof(T).IsAssignableFrom(s.GetType()) select (T)s; }

		public T Data<T>() { return QueryData<T>().SingleOrDefault(); }

	}
}