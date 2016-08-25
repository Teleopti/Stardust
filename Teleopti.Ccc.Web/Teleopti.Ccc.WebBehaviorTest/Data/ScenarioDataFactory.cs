using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class ScenarioDataFactory : TestDataFactory
	{
		private readonly AnalyticsDataFactory _analyticsDataFactory = new AnalyticsDataFactory();
		private readonly IList<IDelayedSetup> _delayedSetups = new List<IDelayedSetup>();
		private static IUnitOfWork _unitOfWork;

		public void TryDisposeUnitOfWork()
		{
			if (_unitOfWork == null) return;

			_unitOfWork.Dispose();
			_unitOfWork = null;
		}

		private static void withScenarioUnitOfWork(Action<ICurrentUnitOfWork> action)
		{
			if (_unitOfWork == null)
			{
				_unitOfWork = SystemSetup.UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
				_unitOfWork.DisableFilter(QueryFilter.BusinessUnit);
			}
			action.Invoke(new ThisUnitOfWork(_unitOfWork));
			_unitOfWork.PersistAll();
		}

		private static void withTenantUnitOfWork(Action<ICurrentTenantSession> action)
		{
			using (SystemSetup.TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				action.Invoke(SystemSetup.CurrentTenantSession);
			}
		}

		public void WithTenantUnitOfWork(Action<ICurrentTenantSession> action)
		{
			withTenantUnitOfWork(action);
		}

		public ScenarioDataFactory() : base(withScenarioUnitOfWork, withTenantUnitOfWork)
		{
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

		public void ApplyLater(IDelayedSetup delayedSetup)
		{
			_delayedSetups.Add(delayedSetup);
		}

		public string ApplyDelayed()
		{
			_analyticsDataFactory.Persist(Me().Culture);

			withScenarioUnitOfWork(uow => _delayedSetups.ForEach(s => s.Apply(Me().Person, uow)));

			return Me().LogOnName;
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