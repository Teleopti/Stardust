using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultDataCreator
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IEventPublisherScope _eventPublisher;

		public DefaultDataCreator(ICurrentUnitOfWorkFactory unitOfWorkFactory, ICurrentUnitOfWork unitOfWork, IEventPublisherScope eventPublisher)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_unitOfWork = unitOfWork;
			_eventPublisher = eventPublisher;
		}

		private static readonly IEnumerable<IHashableDataSetup> setups = new IHashableDataSetup[]
		{
			new DefaultPersonThatCreatesData(),
			new DefaultLicense(),
			new DefaultBusinessUnit(),
			new DefaultRaptorApplicationFunctions(),
			new DefaultMatrixApplicationFunctions()
		};

		private static int? _hashValue;

		public static int HashValue
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
			var dataFactory = new DataFactory(_unitOfWork);
			setups.ForEach(s =>
			{
				if (s is DefaultPersonThatCreatesData)
					using (_eventPublisher.OnThisThreadPublishTo(new NoEventPublisher()))
					using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
						dataFactory.Apply(s);
				else
					using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
						dataFactory.Apply(s);
			});
		}

		public void CreateDefaultScenario()
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				DefaultScenario.Scenario.SetId(null);
				new DefaultScenario().Apply(new ThisUnitOfWork(uow));
				uow.PersistAll();
			}
		}
	}

	public class NoEventPublisher : IEventPublisher
	{
		public void Publish(params IEvent[] events)
		{
		}
	}
}