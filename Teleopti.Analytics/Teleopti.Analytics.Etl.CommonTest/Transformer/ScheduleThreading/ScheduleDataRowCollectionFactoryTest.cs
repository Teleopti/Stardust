using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.ScheduleThreading
{
	[TestFixture]
	public class ScheduleDataRowCollectionFactoryTest
	{
		private ScheduleDataRowCollectionFactory _target;
		private IScheduleProjection _scheduleProjection;
		private int _intervalsPerDay;
		private IPerson _person;
		private IScheduleDataRowFactory _scheduleDataRowFactory;
		private IVisualLayerCollection _projectedLayers;
		private IVisualLayer _layer1;
		private IVisualLayer _layer2;
		private DateTimePeriod _layer1Period;
		private DateTimePeriod _layer2Period;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_intervalsPerDay = 48;
			_mocks = new MockRepository();

			_scheduleDataRowFactory = _mocks.StrictMock<IScheduleDataRowFactory>();
			_scheduleProjection = _mocks.StrictMock<IScheduleProjection>();
			_projectedLayers = _mocks.StrictMock<IVisualLayerCollection>();
			
			_person = PersonFactory.CreatePerson("John", "Smith");

			var activity1 = ActivityFactory.CreateActivity("Phone");
			var activity2 = ActivityFactory.CreateActivity("Email");

			var theDate = new DateTime(2012, 9, 10, 0, 0, 0, DateTimeKind.Utc);
			_layer1Period = new DateTimePeriod(theDate.AddHours(12), theDate.Add(new TimeSpan(12, 15, 0)));
			_layer2Period = new DateTimePeriod(theDate.Add(new TimeSpan(12, 15, 0)), theDate.Add(new TimeSpan(12, 30, 0)));
			
			var factory = new VisualLayerFactory();
			_layer1 = factory.CreateShiftSetupLayer(activity1, _layer1Period, _person);
			_layer2 = factory.CreateShiftSetupLayer(activity2, _layer2Period, _person);

			_target = new ScheduleDataRowCollectionFactory();
		}

		[Test]
		public void ShouldCallFilterLayersThreeTimesWhenTwoLayers()
		{
			
			using (var dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				ScheduleInfrastructure.AddColumnsToDataTable(dataTable);

				var interval = new IntervalBase(_layer1Period.StartDateTime, _intervalsPerDay);
				var intervalPeriod = new DateTimePeriod(_layer1Period.StartDateTime, _layer2Period.EndDateTime);

				var layerCollection = new FilteredVisualLayerCollection(_person, new List<IVisualLayer> {_layer1, _layer2}, new ProjectionPayloadMerger(), null);
				var layerCollectionOnlyLayer1 = new FilteredVisualLayerCollection(_person, new List<IVisualLayer> { _layer1 }, new ProjectionPayloadMerger(), null);
				var layerCollectionOnlyLayer2 = new FilteredVisualLayerCollection(_person, new List<IVisualLayer> { _layer2 }, new ProjectionPayloadMerger(), null);

				using (_mocks.Record())
				{
					Expect.Call(_scheduleProjection.SchedulePartProjection).Return(_projectedLayers).Repeat.Times(3);
					Expect.Call(_projectedLayers.FilterLayers(intervalPeriod)).Return(layerCollection);
					Expect.Call(_projectedLayers.FilterLayers(_layer1Period)).Return(layerCollectionOnlyLayer1);
					Expect.Call(_projectedLayers.FilterLayers(_layer2Period)).Return(layerCollectionOnlyLayer2);
					Expect.Call(_scheduleDataRowFactory.CreateScheduleDataRow(null, null, null, null, new DateTime(), 0, null)).
						IgnoreArguments().Return(dataTable.NewRow()).Repeat.Twice();
				}
				using (_mocks.Playback())
				{
					var dataRowCollection = _target.CreateScheduleDataRowCollection(dataTable, _scheduleProjection, interval,
					                                                                intervalPeriod, DateTime.Now, _intervalsPerDay,
					                                                                _scheduleDataRowFactory);

					dataRowCollection.Count.Should().Be.EqualTo(2);
				}
			}
		}

		[Test]
		public void ShouldCallFilterLayersOnceIfOneOnlyOneLayer()
		{

			using (var dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				ScheduleInfrastructure.AddColumnsToDataTable(dataTable);

				var interval = new IntervalBase(_layer1Period.StartDateTime, _intervalsPerDay);
				var intervalPeriod = new DateTimePeriod(_layer1Period.StartDateTime, _layer2Period.EndDateTime);

				var layerCollectionOnlyLayer1 = new FilteredVisualLayerCollection(_person, new List<IVisualLayer> { _layer1 }, new ProjectionPayloadMerger(), null);

				using (_mocks.Record())
				{
					Expect.Call(_scheduleProjection.SchedulePartProjection).Return(_projectedLayers).Repeat.Times(1);
					Expect.Call(_projectedLayers.FilterLayers(intervalPeriod)).Return(layerCollectionOnlyLayer1);
					Expect.Call(_scheduleDataRowFactory.CreateScheduleDataRow(null, null, null, null, new DateTime(), 0, null)).
						IgnoreArguments().Return(dataTable.NewRow());
				}
				using (_mocks.Playback())
				{
					var dataRowCollection = _target.CreateScheduleDataRowCollection(dataTable, _scheduleProjection, interval,
					                                                                intervalPeriod, DateTime.Now, _intervalsPerDay,
					                                                                _scheduleDataRowFactory);

					dataRowCollection.Count.Should().Be.EqualTo(1);
				}
			}
		}
	}
}
