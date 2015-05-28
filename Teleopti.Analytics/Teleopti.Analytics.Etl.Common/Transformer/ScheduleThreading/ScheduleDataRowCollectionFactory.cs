using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	public class ScheduleDataRowCollectionFactory : IScheduleDataRowCollectionFactory
	{
		public ICollection<DataRow> CreateScheduleDataRowCollection(DataTable dataTable
												  , IScheduleProjection scheduleProjection
												  , IntervalBase interval
												  , DateTimePeriod intervalPeriod
												  , DateTime insertDateTime
												  , int intervalsPerDay
									 , IScheduleDataRowFactory scheduleDataRowFactory)
		{

			var list = new List<DataRow>();

			// Get list of layers inside the given interval period
			IVisualLayerCollection intervalLayerCollection = scheduleProjection.SchedulePartProjection.FilterLayers(intervalPeriod);

			foreach (VisualLayer layer in intervalLayerCollection)
			{
				var layerCollection = intervalLayerCollection.Count() == 1
												? intervalLayerCollection
												: scheduleProjection.SchedulePartProjection.FilterLayers(layer.Period);

				DataRow dataRow = scheduleDataRowFactory.CreateScheduleDataRow(dataTable, layer, scheduleProjection,
																									interval, insertDateTime, intervalsPerDay,
																									layerCollection);
				list.Add(dataRow);
			}
			return list;
		}




	}


}
