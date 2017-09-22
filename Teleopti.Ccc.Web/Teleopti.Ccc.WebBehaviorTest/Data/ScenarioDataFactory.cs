using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class ScenarioDataFactory : TestDataFactory, IDisposable
	{
		private readonly AnalyticsDataFactory _analyticsDataFactory = new AnalyticsDataFactory();
		private readonly IList<IDelayedSetup> _delayedSetups = new List<IDelayedSetup>();

		public void Dispose()
		{
			if (LocalSystem.UnitOfWork.HasCurrent())
				LocalSystem.UnitOfWork.Current().Dispose();
		}
		
		public ScenarioDataFactory() : base(
			LocalSystem.UnitOfWork, 
			LocalSystem.CurrentTenantSession, 
			LocalSystem.TenantUnitOfWork)
		{
			LocalSystem.UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork(QueryFilter.NoFilter);

			AddPerson("I").Apply(new PersonUserConfigurable
			{
				UserName = "1",
				Password = DefaultPassword.ThePassword
			});
		}

		public CultureInfo MyCulture { get { return Me().Culture; } }
		public IPerson MePerson { get { return Me().Person; } }

		public AnalyticsDataFactory Analytics()
		{
			return _analyticsDataFactory;
		}

		public void Apply(IUserSetup setup)
		{
			Me().Apply(setup);
		}

		public void Apply(IUserDataSetup setup)
		{
			Me().Apply(setup);
		}

		public void ApplyAfterSetup(IDelayedSetup delayedSetup)
		{
			_delayedSetups.Add(delayedSetup);
		}

		public void EndSetupPhase()
		{
			_analyticsDataFactory.Persist(Me().Culture);

			_delayedSetups.ForEach(s => s.Apply(Me().Person, LocalSystem.UnitOfWork));
			LocalSystem.UnitOfWork.Current().PersistAll();

			// to create/update any data that is periodically kept up to date
			// like the rule mappings
			LocalSystem.EventPublisher.Publish(new TenantMinuteTickEvent(), new TenantHourTickEvent());
		}

		private IEnumerable<object> AllSetups
		{
			get
			{
				return Me().Applied
						   .Union(_analyticsDataFactory.Setups)
						   .Union(_delayedSetups)
						   .Union(DataFactory.Applied)
					;
			}
		}

		public IEnumerable<T> UserDatasOfType<T>()
		{
			return from s in AllSetups where typeof(T).IsAssignableFrom(s.GetType()) select (T)s;
		}

		public bool HasSetup<T>()
		{
			return UserDatasOfType<T>().Any();
		}

		public T UserData<T>()
		{
			return UserDatasOfType<T>().Last();
		}

	}
}