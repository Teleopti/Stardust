﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultDataCreator
	{
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

		public void CreateDefaultScenario()
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				new DefaultScenario().Apply(new ThisUnitOfWork(uow));
				uow.PersistAll();
			}
		}
	}
}