using System;
using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
	[TestFixture]
	public static class InvalidateRtaDataHandlerCacheTest
	{
		 [Test]
		 public static void ShouldInvalidate()
		 {
			 var mbCacheFactory = MockRepository.GenerateMock<IMbCacheFactory>();
			 var dataHandler = MockRepository.GenerateMock<IDatabaseReader>();
			 var alarmMapper = MockRepository.GenerateMock<IAlarmMapper>();
             var currentAndNextLayerExtractor = MockRepository.GenerateMock<ICurrentAndNextLayerExtractor>();
			 var personId = Guid.NewGuid();
			 var timeStamp = DateTime.Now;
			 var target = new actualAgentAssemblerForTest(dataHandler, mbCacheFactory, alarmMapper, currentAndNextLayerExtractor);

			 target.InvalidateReadModelCache(personId);

			 mbCacheFactory.AssertWasCalled(
				 x =>
				 x.Invalidate(target.ExposeDatabaseReader,
				              y => y.ActivityAlarms(), true),
				 o => o.IgnoreArguments());
		 }

		private class actualAgentAssemblerForTest : ActualAgentAssembler
		{
			public actualAgentAssemblerForTest(IDatabaseReader databaseReader, IMbCacheFactory mbCacheFactory, IAlarmMapper alarmMapper, ICurrentAndNextLayerExtractor currentAndNextLayerExtractor) : base(databaseReader, currentAndNextLayerExtractor, mbCacheFactory, alarmMapper)
			{
			}

			public IDatabaseReader ExposeDatabaseReader
			{
				get { return DatabaseReader; }
			}
		}
	}
}