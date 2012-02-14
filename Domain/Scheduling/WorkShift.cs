using System;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Work shift class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-10-24
    /// </remarks>
    public class WorkShift : Shift, IWorkShift
    {

        private readonly IShiftCategory _shiftCategory;
        private IVisualLayerCollection _visualLayerCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkShift"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-10
        /// </remarks>
        public WorkShift(IShiftCategory category)
        {
            InParameter.NotNull("category", category);
            _shiftCategory = category;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkShift"/> class. For NHib.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        protected WorkShift() 
        {
        }

        /// <summary>
        /// Gets the base date.
        /// </summary>
        /// <value>The base date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-29
        /// </remarks>
        public static DateTime BaseDate
        {
            get { return DateTime.SpecifyKind(new DateTime(1800, 1, 1), DateTimeKind.Utc); }

        }

        /// <summary>
        /// Gets the shift category.
        /// </summary>
        /// <value>The shift category.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-10
        /// </remarks>
        public IShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
        }

       public TimePeriod? ToTimePeriod()
       {
           DateTimePeriod? period = Projection.Period();

           if(!period.HasValue)
               return null;

           TimeSpan startTime = period.Value.StartDateTime.Subtract(BaseDate);
           TimeSpan endTime = period.Value.EndDateTime.Subtract(BaseDate);
           return new TimePeriod(startTime, endTime);
       }


        /// <summary>
        /// Called before layer is added to collection.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <remarks>
        /// Check here on shift because we want activity layers to be persisted in different tables
        /// (eg adding an activity layer to a MainShift shouldn't be possible even though it makes
        /// perfect sence regarding objects)
        /// Created by: rogerkr
        /// Created date: 2008-01-25
        /// </remarks>
        public override void OnAdd(ILayer<IActivity> layer)
        {
            if (!(layer is WorkShiftActivityLayer))
                throw new ArgumentException("Only WorkShiftActivityLayers can be added to a WorkShift");
            _visualLayerCollection = null;
        }


        /// <summary>
        /// Gets the projection.
        /// </summary>
        /// <value>The projection.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-05-09
        /// </remarks>
        public IVisualLayerCollection Projection
        {
            get
            {
                if (_visualLayerCollection == null)
                {
                    _visualLayerCollection = ProjectionService().CreateProjection();
                }
                return _visualLayerCollection;
            }
        }

        public IMainShift ToMainShift(DateTime localMainShiftBaseDate, ICccTimeZoneInfo localTimeZoneInfo)
        {
            
            DateTime utcDateBaseDate = TimeZoneHelper.ConvertToUtc(localMainShiftBaseDate, localTimeZoneInfo);
            DateTime nextUtcDate = TimeZoneHelper.ConvertToUtc(localMainShiftBaseDate.AddDays(1), localTimeZoneInfo);
            if (nextUtcDate.AddMinutes(-1).Subtract(utcDateBaseDate) != TimeSpan.FromHours(24).Add(TimeSpan.FromMinutes(-1)))
                return ToMainShiftOnDaylightSavingChange(localMainShiftBaseDate, localTimeZoneInfo);

            MainShift ret = new MainShift(ShiftCategory);
            foreach (ILayer<IActivity> layer in LayerCollection)
            {
                DateTime start = utcDateBaseDate.Add(layer.Period.StartDateTime - BaseDate);
                DateTime end = start.Add(layer.Period.ElapsedTime());
                DateTimePeriod outPeriod = new DateTimePeriod(start, end);

                MainShiftActivityLayer mainShiftLayer = new MainShiftActivityLayer(layer.Payload, outPeriod);
                ret.LayerCollection.Add(mainShiftLayer);
            }

            return ret;
        }

        private IMainShift ToMainShiftOnDaylightSavingChange(DateTime localMainShiftBaseDate, ICccTimeZoneInfo localTimeZoneInfo)
        {
            MainShift ret = new MainShift(ShiftCategory);
            int hoursToChange = 0;
            foreach (ILayer<IActivity> layer in LayerCollection)
            {
                DateTime localStart = localMainShiftBaseDate.Add(layer.Period.StartDateTime - BaseDate);
                DateTime localEnd = localMainShiftBaseDate.Add(layer.Period.EndDateTime - BaseDate);
                localStart = DateTime.SpecifyKind(localStart, DateTimeKind.Unspecified);
                localEnd = DateTime.SpecifyKind(localEnd, DateTimeKind.Unspecified);

                if (localTimeZoneInfo.IsInvalidTime(localStart))
                    hoursToChange = 1;

                localStart = localStart.AddHours(hoursToChange);

                if (localTimeZoneInfo.IsInvalidTime(localEnd))
                    hoursToChange = 1;


                localEnd = localEnd.AddHours(hoursToChange);

                DateTime start = localTimeZoneInfo.ConvertTimeToUtc(localStart, localTimeZoneInfo);
                DateTime end = localTimeZoneInfo.ConvertTimeToUtc(localEnd, localTimeZoneInfo);
                DateTimePeriod outPeriod = new DateTimePeriod(start, end);

                MainShiftActivityLayer mainShiftLayer = new MainShiftActivityLayer(layer.Payload, outPeriod);
                ret.LayerCollection.Add(mainShiftLayer);
            }

            return ret;
        }
    }
}