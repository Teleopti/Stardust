using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WorkShift : IWorkShift
    {

        private readonly IShiftCategory _shiftCategory;
        private IVisualLayerCollection _visualLayerCollection;
		private IList<ILayer<IActivity>> _layerCollection = new List<ILayer<IActivity>>();

        public WorkShift(IShiftCategory category)
        {
            InParameter.NotNull("category", category);
            _shiftCategory = category;
        }

        protected WorkShift() 
        {
        }

		public virtual ILayerCollection<IActivity> LayerCollection
		{
			get { return (new LayerCollection<IActivity>(this, _layerCollection)); }

		}

		public virtual IProjectionService ProjectionService()
		{
			var proj = new VisualLayerProjectionService(null);
			proj.Add(LayerCollection, new VisualLayerFactory());
			return proj;
		}

		public virtual object Clone()
		{
			return EntityClone();
		}

		public virtual IWorkShift NoneEntityClone()
		{
			var retObj = (WorkShift)MemberwiseClone();
			retObj._layerCollection = new List<ILayer<IActivity>>();
			foreach (var newLayer in _layerCollection.Select(layer => ((WorkShiftActivityLayer)layer).NoneEntityClone()))
			{
				retObj._layerCollection.Add(newLayer);
			}
			return retObj;
		}

		public virtual IWorkShift EntityClone()
		{
			var retObj = (WorkShift)MemberwiseClone();
			retObj._layerCollection = new List<ILayer<IActivity>>();
			foreach (var newLayer in _layerCollection.Select(layer => ((WorkShiftActivityLayer)layer).EntityClone()))
			{
				retObj._layerCollection.Add(newLayer);
			}
			return retObj;
		}

        public static DateTime BaseDate
        {
            get { return DateTime.SpecifyKind(new DateTime(1800, 1, 1), DateTimeKind.Utc); }

        }

        public IShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
        }

       public TimePeriod? ToTimePeriod()
       {
           var period = Projection.Period();

           if(!period.HasValue)
               return null;

           var startTime = period.Value.StartDateTime.Subtract(BaseDate);
           var endTime = period.Value.EndDateTime.Subtract(BaseDate);
           return new TimePeriod(startTime, endTime);
       }

        public void OnAdd(ILayer<IActivity> layer)
        {
            if (!(layer is WorkShiftActivityLayer))
                throw new ArgumentException("Only WorkShiftActivityLayers can be added to a WorkShift");
            _visualLayerCollection = null;
        }

        public IVisualLayerCollection Projection
        {
            get { return _visualLayerCollection ?? (_visualLayerCollection = ProjectionService().CreateProjection()); }
        }

        public IEditableShift ToEditorShift(DateTime localMainShiftBaseDate, TimeZoneInfo localTimeZoneInfo)
        {
            var utcDateBaseDate = TimeZoneHelper.ConvertToUtc(localMainShiftBaseDate, localTimeZoneInfo);
            var nextUtcDate = TimeZoneHelper.ConvertToUtc(localMainShiftBaseDate.AddDays(1), localTimeZoneInfo);
            if (nextUtcDate.AddMinutes(-1).Subtract(utcDateBaseDate) != TimeSpan.FromHours(24).Add(TimeSpan.FromMinutes(-1)))
                return toMainShiftOnDaylightSavingChange(localMainShiftBaseDate, localTimeZoneInfo);

            var ret = new EditableShift(ShiftCategory);
            foreach (var layer in LayerCollection)
            {
                var start = utcDateBaseDate.Add(layer.Period.StartDateTime - BaseDate);
                var end = start.Add(layer.Period.ElapsedTime());
                var outPeriod = new DateTimePeriod(start, end);

                var mainShiftLayer = new EditableShiftLayer(layer.Payload, outPeriod);
                ret.LayerCollection.Add(mainShiftLayer);
            }

            return ret;
        }

		private IEditableShift toMainShiftOnDaylightSavingChange(DateTime localMainShiftBaseDate, TimeZoneInfo localTimeZoneInfo)
        {
            var ret = new EditableShift(ShiftCategory);
            var hoursToChange = 0;
            foreach (var layer in LayerCollection)
            {
                var localStart = localMainShiftBaseDate.Add(layer.Period.StartDateTime - BaseDate);
                var localEnd = localMainShiftBaseDate.Add(layer.Period.EndDateTime - BaseDate);
                localStart = DateTime.SpecifyKind(localStart, DateTimeKind.Unspecified);
                localEnd = DateTime.SpecifyKind(localEnd, DateTimeKind.Unspecified);

                if (localTimeZoneInfo.IsInvalidTime(localStart))
                    hoursToChange = 1;

                localStart = localStart.AddHours(hoursToChange);

                if (localTimeZoneInfo.IsInvalidTime(localEnd))
                    hoursToChange = 1;


                localEnd = localEnd.AddHours(hoursToChange);

                var start = localTimeZoneInfo.SafeConvertTimeToUtc(localStart);
                var end = localTimeZoneInfo.SafeConvertTimeToUtc(localEnd);
                var outPeriod = new DateTimePeriod(start, end);

				var mainShiftLayer = new EditableShiftLayer(layer.Payload, outPeriod);
                ret.LayerCollection.Add(mainShiftLayer);
            }

            return ret;
        }
    }
}