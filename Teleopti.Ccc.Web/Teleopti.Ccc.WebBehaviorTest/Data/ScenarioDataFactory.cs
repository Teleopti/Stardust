using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class ScenarioDataFactory : TestDataFactory, IDisposable
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly AnalyticsDataFactory _analyticsDataFactory = new AnalyticsDataFactory();
		private readonly IList<IDelayedSetup> _delayedSetups = new List<IDelayedSetup>();

		public ScenarioDataFactory(
			ICurrentUnitOfWork unitOfWork,
			ICurrentUnitOfWorkFactory unitOfWorks,
			IResolver resolver,
			IEventPublisher eventPublisher) : base(
			unitOfWork,
			resolver)
		{
			_eventPublisher = eventPublisher;
			var uow = unitOfWorks.Current().CreateAndOpenUnitOfWork();
			uow.DisableFilter(QueryFilter.BusinessUnit);

			Person("I").Apply(new PersonUserConfigurable
			{
				UserName = "1",
				Password = DefaultPassword.ThePassword
			});
		}

		public void Dispose()
		{
			if (_unitOfWork.HasCurrent())
				_unitOfWork.Current().Dispose();
		}

		public CultureInfo MyCulture => Me().Culture;
		public IPerson MePerson => Me().Person;
		public AnalyticsDataFactory Analytics() => _analyticsDataFactory;
		public void Apply(IUserSetup setup) => Me().Apply(setup);
		public void Apply(IUserDataSetup setup) => Me().Apply(setup);
		public void ApplyAfterSetup(IDelayedSetup delayedSetup) => _delayedSetups.Add(delayedSetup);

		public void EndSetupPhase()
		{
			_analyticsDataFactory.Persist(Me().Culture);

			_delayedSetups.ForEach(s => s.Apply(Me().Person, _unitOfWork));
			_unitOfWork.Current().PersistAll();

			// to create/update any data that is periodically kept up to date
			// like the rule mappings
			_eventPublisher.Publish(new TenantMinuteTickEvent(), new TenantHourTickEvent());
		}

		private IEnumerable<object> applied =>
			Applied
				.Union(Me().Applied)
				.Union(_analyticsDataFactory.Setups)
				.Union(_delayedSetups);

		public IEnumerable<T> UserDatasOfType<T>() =>
			from s in applied where typeof(T).IsAssignableFrom(s.GetType()) select (T) s;

		public bool HasSetup<T>() => UserDatasOfType<T>().Any();
		public T UserData<T>() => UserDatasOfType<T>().Last();
	}
}