using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultDataCreator
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DefaultDataCreator));

		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public DefaultDataCreator(ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		private readonly IEnumerable<IHashableDataSetup> setups = new IHashableDataSetup[]
		{
			new DefaultPersonThatCreatesData(),
			new DefaultLicense(),
			new DefaultBusinessUnit(),
			new DefaultScenario(),
			new DefaultRaptorApplicationFunctions(),
			new DefaultMatrixApplicationFunctions()
		};

		private int? _hashValue;

		public int HashValue
		{
			get
			{
				if (!_hashValue.HasValue)
					_hashValue = setups.Aggregate(37, (current, setup) => current ^ setup.HashValue());
				log.Debug("_hashValue.Value " + _hashValue.Value);
				return _hashValue.Value;
			}
		}

		public void Create()
		{
			var dataFactory = new DataFactory(action =>
			{
				using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					action.Invoke(new ThisUnitOfWork(uow));
					uow.PersistAll();
				}
			});
			setups.ForEach(dataFactory.Apply);
		}
	}
}