using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class DataFactory
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IResolver _resolver;
		private readonly IList<object> _applied = new List<object>();

		public DataFactory(ICurrentUnitOfWork unitOfWork, IResolver resolver)
		{
			_unitOfWork = unitOfWork;
			_resolver = resolver;
		}
		
		public void Apply<T>(T specOrSetup)
		{
			switch (specOrSetup)
			{
				case IDataSetup setup:
					apply(setup);
					break;
				default:
					apply(specOrSetup);
					break;
			}
		}

		private void apply(IDataSetup setup)
		{
			setup.Apply(_unitOfWork);
			_unitOfWork.Current().PersistAll();
			_applied.Add(setup);
		}
		
		private void apply<T>(T spec)
		{
			var dataSetup = _resolver.ResolveDataSetupFor<T>();
			if (dataSetup != null)
			{
				dataSetup.Apply(spec);
				_unitOfWork.Current().PersistAll();
				_applied.Add(spec);
				return;
			}
			throw new NotImplementedException($"Cant resolve setup for {spec.GetType().Name}");
		}
		
		public IEnumerable<object> Applied => _applied;
	}
}