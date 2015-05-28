using System;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	public interface IScheduleDataRowFactory
	{
		DataRow CreateScheduleDataRow(DataTable dataTable, IVisualLayer layer, IScheduleProjection scheduleProjection, IntervalBase interval, DateTime insertDateTime, int intervalsPerDay, IVisualLayerCollection layerCollection);

		DateTimePeriod GetShiftPeriod(IVisualLayerCollection layerCollection, ILayer<IPayload> layer);
	}
}