using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MbCache.Core;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PhoneStateStepDefinitions
	{

		private readonly Lazy<IRtaDataHandler> rtaFactory = new Lazy<IRtaDataHandler>(() =>
			{
				var databaseConnectionFactory = new DatabaseConnectionFactory();
				var databaseConnectionStringHandler = new DatabaseConnectionStringHandlerFake();
				var databaseWriter = new DatabaseWriter(databaseConnectionFactory, databaseConnectionStringHandler);
				var databaseReader = new DatabaseReader(databaseConnectionFactory, databaseConnectionStringHandler,
				                                        new ActualAgentStateCache(databaseWriter));
				var mbCacheFactory = new MbCacheFactoryFake();
				return new RtaDataHandler(new AsyncSignalSender(TestSiteConfigurationSetup.Url.ToString()),
				                          new DataSourceResolverFake(),
				                          new PersonResolverFake(n => DataMaker.Person(n).Person),
				                          new ActualAgentAssembler(databaseReader, new CurrentAndNextLayerExtractor(),
				                                                   mbCacheFactory,
				                                                   new AlarmMapper(databaseReader, databaseWriter,
				                                                                   mbCacheFactory)),
				                          new ActualAgentStateCache(databaseWriter));
			});

		[When(@"'(.*)' sets (?:his|her) phone state to '(.*)'")]
		public void WhenSetsHisPhoneStateTo(string personName, string state)
		{
			rtaFactory.Value.ProcessRtaData(personName, state, TimeSpan.Zero, CurrentTime.Value(), Guid.Empty, "0",
																			DateHelper.MinSmallDateTime, false);
		}
	}


	public class DatabaseConnectionStringHandlerFake : IDatabaseConnectionStringHandler
	{
		public string AppConnectionString()
		{
			return TestData.DataSource.Application.ConnectionString;
		}

		public string DataStoreConnectionString()
		{
			return TestData.DataSource.Statistic.ConnectionString;
		}
	}

	public class MbCacheFactoryFake : IMbCacheFactory
	{
		public T Create<T>(params object[] parameters) where T : class
		{
			throw new NotImplementedException();
		}

		public T ToCachedComponent<T>(T uncachedComponent) where T : class
		{
			throw new NotImplementedException();
		}

		public void Invalidate<T>()
		{
			throw new NotImplementedException();
		}

		public void Invalidate(object component)
		{
			throw new NotImplementedException();
		}

		public void Invalidate<T>(T component, Expression<Func<T, object>> method, bool matchParameterValues)
		{
			throw new NotImplementedException();
		}

		public bool IsKnownInstance(object component)
		{
			throw new NotImplementedException();
		}

		public Type ImplementationTypeFor(Type componentType)
		{
			throw new NotImplementedException();
		}

		public void DisableCache<T>(bool evictCacheEntries = true)
		{
			throw new NotImplementedException();
		}

		public void EnableCache<T>()
		{
			throw new NotImplementedException();
		}
	}

	public class PersonResolverFake : IPersonResolver
	{
		private readonly Func<string, IPerson> _func;

		public PersonResolverFake(Func<string, IPerson> func)
		{
			_func = func;
		}

		public bool TryResolveId(int dataSourceId, string logOn, out IEnumerable<PersonWithBusinessUnit> personId)
		{
			var person = _func.Invoke(logOn);
			personId = new List<PersonWithBusinessUnit>
				{
					new PersonWithBusinessUnit
						{
							BusinessUnitId = person.PersonPeriodCollection.First().Team.BusinessUnitExplicit.Id.GetValueOrDefault(),
							PersonId = person.Id.GetValueOrDefault()
						}
				};

			return true;
		}
	}

	public class DataSourceResolverFake : IDataSourceResolver
	{
		public bool TryResolveId(string sourceId, out int dataSourceId)
		{
			dataSourceId = 0;
			return true;
		}
	}
}