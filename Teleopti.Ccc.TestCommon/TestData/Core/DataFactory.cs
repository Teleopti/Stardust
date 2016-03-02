using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class DataFactory
	{
		private readonly Action<Action<ICurrentUnitOfWork>> _unitOfWorkAction;
		private readonly IList<IDataSetup> _applied = new List<IDataSetup>();

		public DataFactory(Action<Action<ICurrentUnitOfWork>> unitOfWorkAction)
		{
			_unitOfWorkAction = unitOfWorkAction;
		}

		public void Apply(IDataSetup setup)
		{
			_unitOfWorkAction(setup.Apply);
			_applied.Add(setup);
		}

		public IEnumerable<IDataSetup> Applied { get { return _applied; } }

	}
}