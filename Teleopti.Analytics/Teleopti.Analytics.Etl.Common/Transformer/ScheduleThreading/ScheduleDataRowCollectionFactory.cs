using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	public class ScheduleDataRowCollectionFactory
	{
		public ICollection<DataRow> CreateScheduleDataRowCollection(DataTable dataTable
			, IScheduleProjection scheduleProjection
			, IntervalBase interval
			, DateTimePeriod intervalPeriod
			, DateTime insertDateTime
			, int intervalsPerDay
			, ScheduleDataRowFactory scheduleDataRowFactory
			, DateTime shiftStart
			, DateTime shiftEnd)
		{ 

			var list = new List<DataRow>();

			// Get list of layers inside the given interval period
			IVisualLayerCollection intervalLayerCollection = scheduleProjection.SchedulePartProjection.FilterLayers(intervalPeriod);

			foreach (VisualLayer layer in intervalLayerCollection)
			{
				var layerCollection = intervalLayerCollection.Count() == 1
												? intervalLayerCollection
												: scheduleProjection.SchedulePartProjection.FilterLayers(layer.Period);

				DataRow dataRow = scheduleDataRowFactory.CreateScheduleDataRow(dataTable, 
					layer, 
					scheduleProjection,
					interval, 
					insertDateTime, 
					intervalsPerDay,
					layerCollection,
					shiftStart,
					shiftEnd);
				list.Add(dataRow);
			}
			return list;
		}




	}


}
