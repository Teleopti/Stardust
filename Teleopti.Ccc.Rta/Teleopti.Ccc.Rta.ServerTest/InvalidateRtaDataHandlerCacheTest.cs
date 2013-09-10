using System;
using System.Collections.Generic;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public static class InvalidateRtaDataHandlerCacheTest
	{
		 [Test]
		 public static void ShouldInvalidate()
		 {
			 var mbCacheFactory = MockRepository.GenerateMock<IMbCacheFactory>();
			 var dataHandler = MockRepository.GenerateMock<IDatabaseHandler>();
			 var alarmMapper = MockRepository.GenerateMock<IAlarmMapper>();
			 var personId = Guid.NewGuid();
			 var timeStamp = DateTime.Now;
			 var target = new actualAgentAssemblerForTest(dataHandler, mbCacheFactory, alarmMapper);

			 target.InvalidateReadModelCache(personId);

			 mbCacheFactory.AssertWasCalled(
				 x =>
				 x.Invalidate(target.ExposeDatabaseHandler,
				              y => y.CurrentLayerAndNext(timeStamp, new List<ScheduleLayer>()), true),
				 o => o.IgnoreArguments());
		 }

		private class actualAgentAssemblerForTest : ActualAgentAssembler
		{
			public actualAgentAssemblerForTest(IDatabaseHandler databaseHandler, IMbCacheFactory mbCacheFactory, IAlarmMapper alarmMapper) : base(databaseHandler, mbCacheFactory, alarmMapper)
			{
			}

			public IDatabaseHandler ExposeDatabaseHandler
			{
				get { return DatabaseHandler; }
			}
		}
	}
}