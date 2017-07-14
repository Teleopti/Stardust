using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class DataFactory
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IList<IDataSetup> _applied = new List<IDataSetup>();

		public DataFactory(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Apply(IDataSetup setup)
		{
			setup.Apply(_unitOfWork);
			_unitOfWork.Current().PersistAll();
			_applied.Add(setup);
		}

		public IEnumerable<IDataSetup> Applied => _applied;
	}
}