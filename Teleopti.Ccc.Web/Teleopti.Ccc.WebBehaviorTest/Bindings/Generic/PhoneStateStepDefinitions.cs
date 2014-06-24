using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using MbCache.Core;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
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
				var now = new ThisIsNow(CurrentTime.Value());
				var databaseReader = new DatabaseReader(databaseConnectionFactory, databaseConnectionStringHandler, now);
				var mbCacheFactory = new MbCacheFactoryFake();
				var messageSender = new SignalSender(TestSiteConfigurationSetup.Url.ToString(), new IConnectionKeepAliveStrategy[]{}, new Time(now));
				messageSender.StartBrokerService();
				var personOrganizationProvider = new PersonOrganizationProvider(new PersonOrganizationReader(now, ConnectionStringHelper.ConnectionStringUsedInTests));
				return new RtaDataHandler(messageSender,
				                          new DataSourceResolverFake(),
				                          new PersonResolverFake(n => DataMaker.Person(n).Person),
				                          new ActualAgentAssembler(databaseReader, new CurrentAndNextLayerExtractor(),
				                                                   mbCacheFactory,
				                                                   new AlarmMapper(databaseReader, databaseWriter,
				                                                                   mbCacheFactory)),
																	databaseWriter,
				                          
						                          new AdherenceAggregator(messageSender,
						                                                  new OrganizationForPerson(personOrganizationProvider))
					                      );
			});

		[Given(@"there is an external logon named '(.*)' with datasource (.*)")]
		public void GivenThereIsAnExternalLogonNamedWithDatasource(string acdLogOnName, int datasourceId)
		{
			DataMaker.Data().Apply(new ExternalLogonConfigurable()
			{
				AcdLogOnName = acdLogOnName,
				DataSourceId = datasourceId,
				AcdLogOnOriginalId = acdLogOnName
			});
		}

		[When(@"'(.*)' sets (?:his|her) phone state to '(.*)' on datasource (.*)")]
		[Given(@"'(.*)' sets (?:his|her) phone state to '(.*)' on datasource (.*)")]
		public void WhenSetsHisPhoneStateToOnDatasource(string personName, string stateCode, int datasource)
		{
			var rtaUri = new Uri(TestSiteConfigurationSetup.Url, "Rta/Service/SaveExternalUserState");
			var request = (HttpWebRequest) WebRequest.Create(rtaUri);
			request.Method = "POST";
			request.ContentType = "application/json";

			var externalState = JsonConvert.SerializeObject(new AjaxUserState
			{
				AuthenticationKey = "!#¤atAbgT%",
				UserCode = personName,
				StateCode = stateCode,
				IsLoggedOn = true,
				Timestamp = CurrentTime.Value().ToString("yyyy-MM-dd HH:mm:ss"),
				PlatformTypeId = Guid.Empty.ToString(),
				SourceId = datasource.ToString(CultureInfo.InvariantCulture),
				IsSnapshot = false
			});

			using (var writer = new StreamWriter(request.GetRequestStream()))
			{
				writer.Write(externalState);
			}
			request.GetResponse();
		}


		//[Given(@"'(.*)' sets (?:his|her) phone state to '(.*)'")]
		//[When(@"'(.*)' sets (?:his|her) phone state to '(.*)'")]
		//public void WhenSetsHisPhoneStateTo(string personName, string state)
		//{
		//	rtaFactory.Value.ProcessRtaData(personName, state, TimeSpan.Zero, CurrentTime.Value(), Guid.Empty, "0",
		//																	DateHelper.MinSmallDateTime, false);
		//}

		[Given(@"there is a datasouce with id (.*)")]
		public void GivenThereIsADatasouceWithId(int datasourceId)
		{
			var datasource = new Datasources(datasourceId, " ", -1, " ", -1, " ", " ", 1, false, "6", false);
			DataMaker.Analytics().Apply(datasource);
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