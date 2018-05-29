using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using NHibernate.Transform;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	public class ExtensiveLogRepositoryTest : IIsolateSystem
	{
		public IExtensiveLogRepository Target;
		public WithUnitOfWork WithUnitOfWork;
		public FakeCurrentBusinessUnit CurrentBusinessUnit;
		public FakeLoggedOnUser LoggedOnUser;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			var bu = BusinessUnitFactory.CreateWithId("BU");
			var fb = new FakeCurrentBusinessUnit();
			fb.FakeBusinessUnit(bu);
			isolate.UseTestDouble(fb).For<ICurrentBusinessUnit>();

			var person = PersonFactory.CreatePerson("system").WithId();
			isolate.UseTestDouble(new FakeLoggedOnUser(person)).For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldSaveLog()
		{
			Now.Is("2018-05-25 09:54".Utc());
			logSetting("true");

			WithUnitOfWork.Do(() =>
			{
				Target.Add("This is a string", Guid.NewGuid(), "StringType");
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs =  Target.LoadAll();
				extensiveLogs.Count.Should().Be.EqualTo(1);
			});
		}

		[Test]
		public void ShouldSaveLogWithValues()
		{
			Now.Is("2018-05-25 09:54".Utc());
			logSetting("true");

			var obj = "This is a string";
			var objId = Guid.NewGuid();
			var entityType = "StringType";

			WithUnitOfWork.Do(() =>
			{
				Target.Add(obj, objId, entityType);
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs = Target.LoadAll();
				extensiveLogs.Count.Should().Be.EqualTo(1);
				extensiveLogs.First().BusinessUnit.Should().Be.EqualTo(CurrentBusinessUnit.CurrentId());
				extensiveLogs.First().EntityType.Should().Be.EqualTo(entityType);
				extensiveLogs.First().ObjectId.Should().Be.EqualTo(objId);
				extensiveLogs.First().Person.Should().Be.EqualTo(LoggedOnUser.CurrentUser().Id);
				extensiveLogs.First().UpdatedOn.Should().Be.EqualTo(Now.UtcDateTime());
			});
		}

		[Test]
		public void ShouldSaveLogWithRawData()
		{
			Now.Is("2018-05-25 09:54".Utc());
			logSetting("true");

			var obj = "This is a string";
			var objId = Guid.NewGuid();
			var entityType = "StringType";
			var stringJson =  JsonConvert.SerializeObject(obj, Formatting.Indented,
				new JsonSerializerSettings()
				{
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore
				}
			);
			WithUnitOfWork.Do(() =>
			{
				Target.Add(obj, objId, entityType);
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs = Target.LoadAll();
				extensiveLogs.First().RawData.Should().Be.EqualTo(stringJson);
			});
		}

		[Test]
		public void ShouldNotLogIfsettingIsDisabled()
		{
			Now.Is("2018-05-25 09:54".Utc());
			logSetting("false");
			var obj = "This is a string";
			var objId = Guid.NewGuid();
			var entityType = "StringType";
			
			WithUnitOfWork.Do(() =>
			{
				Target.Add(obj, objId, entityType);
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs = Target.LoadAll();
				extensiveLogs.Count.Should().Be.EqualTo(0);
			});
		}

		[Test]
		public void ShouldLogHostAndIpAddress()
		{
			Now.Is("2018-05-25 09:54".Utc());
			logSetting("true");

			var person = PersonFactory.CreatePersonWithId();

			string hostName = Dns.GetHostName(); // Retrive the Name of HOST  

			string myIP = Dns.GetHostAddresses(hostName).First(x => x.AddressFamily.ToString() == ProtocolFamily.InterNetwork.ToString()).ToString();

			WithUnitOfWork.Do(() =>
			{
				Target.Add(person, person.Id.GetValueOrDefault(), "Person");
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs = Target.LoadAll();
				extensiveLogs.First().HostName.Should().Be.EqualTo(hostName);
				extensiveLogs.First().IpAddress.Should().Be.EqualTo(myIP);
			});
		}

		[Test]
		public void ShouldDisableLoggingIfTimeout()
		{
			Now.Is("2018-05-25 09:50".Utc());
			logSetting("true");
			
			WithUnitOfWork.Do(() =>
			{
				var person = PersonFactory.CreatePersonWithId();
				Target.Add(person, person.Id.GetValueOrDefault(), "Person");
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs = Target.LoadAll();
				extensiveLogs.Count.Should().Be.EqualTo(1);
			});
			//
			Now.Is("2018-05-25 09:52".Utc());
			

			WithUnitOfWork.Do(() =>
			{
				var person = PersonFactory.CreatePersonWithId();
				Target.Add(person, person.Id.GetValueOrDefault(), "Person");
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs = Target.LoadAll();
				extensiveLogs.Count.Should().Be.EqualTo(2);
			});
			//
			Now.Is("2018-05-25 10:20".Utc());

			WithUnitOfWork.Do(() =>
			{
				var person = PersonFactory.CreatePersonWithId();
				Target.Add(person, person.Id.GetValueOrDefault(), "Person");
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs = Target.LoadAll();
				extensiveLogs.Count.Should().Be.EqualTo(2);
			});

			WithUnitOfWork.Do(uow =>
			{
				var session = uow.Current().FetchSession();
				var command = $@"select value,StartedLoggingAt from ExtensiveLogsSettings WHERE Setting = 'EnableRequestLogging' ";
				var result = session.CreateSQLQuery(command).SetResultTransformer(Transformers.AliasToEntityMap)
					.List<Hashtable>().First(); ;
				result["value"].Should().Be.EqualTo("false");
				result["StartedLoggingAt"].Should().Be.Null();
				
			});
		}


		[Test]
		public void ShouldSaveLogWithHugeRawData()
		{
			Now.Is("2018-05-25 09:54".Utc());
			logSetting("true");

			var personList = new List<IPerson>();
			for (int i = 0; i < 1000; i++)
			{
				personList.Add(PersonFactory.CreatePersonWithId());
			}
			var objId = Guid.NewGuid();
			var entityType = "StringType";
			
			WithUnitOfWork.Do(() =>
			{
				Target.Add(personList, objId, entityType);
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs = Target.LoadAll();
				extensiveLogs.Count.Should().Be.EqualTo(1);
			});
		}

		[Test]
		public void ShouldSaveLogWithHugeRawDataWithContent()
		{
			Now.Is("2018-05-25 09:54".Utc());
			logSetting("true");

			var personList = new List<IPerson>();
			for (int i = 0; i < 100000; i++)
			{
				personList.Add(PersonFactory.CreatePersonWithId());
			}
			var stringJson = JsonConvert.SerializeObject(personList, Formatting.Indented,
				new JsonSerializerSettings()
				{
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore
				}
			);
			var objId = Guid.NewGuid();
			var entityType = "StringType";

			WithUnitOfWork.Do(() =>
			{
				Target.Add(personList, objId, entityType);
			});

			WithUnitOfWork.Do(() =>
			{
				var extensiveLogs = Target.LoadAll();
				extensiveLogs.First().RawData.Should().Be.EqualTo(stringJson);
			});
		}


		private void logSetting(string value)
		{
			WithUnitOfWork.Do(uow =>
			{
				var session = uow.Current().FetchSession();
				var command = $@"UPDATE ExtensiveLogsSettings SET Value = '{value}' WHERE Setting = 'EnableRequestLogging' ";
				session.CreateSQLQuery(command).ExecuteUpdate();
			});
		}
	}
}
