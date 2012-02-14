using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Mapps old availability to new Availability
    /// </summary>
    /// <remarks>
    /// Created by: ZoeT
    /// Created date: 2008-12-05
    /// </remarks>
    public class AvailabilityMapper : Mapper<IAvailabilityRotation, DataRow>
    {
        private readonly IList<DataRow> _availabilityDays;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailabilityMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="availabilityDays">The availability days.</param>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public AvailabilityMapper(MappedObjectPair mappedObjectPair, IList<DataRow> availabilityDays)
            : base(mappedObjectPair, null)
        {
            _availabilityDays = availabilityDays;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public override IAvailabilityRotation Map(DataRow oldEntity)
        {

            ////TODO:What should be in description? Person name and start date?

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            var id = (int)oldEntity["core_id"];
            var date = (DateTime)oldEntity["date_from"];
            var empId = (int)oldEntity["emp_id"];
            string description;
            if (FindPerson(empId) == null)
                description = string.Concat(date.ToString("d", cultureInfo));
            else
                description = string.Concat(FindPerson(empId).Name, " ", date.ToString("d", cultureInfo));
            IAvailabilityRotation availability = new AvailabilityRotation(description, ((int)oldEntity["periods"] * 7));

            IEnumerable<DataRow> theDays = DaysOnAvailability(id);
            foreach (DataRow day in theDays)
            {
                int dayIdx = (int)day["core_day"] - 1;

                int startMinute = 0;
                if (day["time_from"] != DBNull.Value)
                {
                    startMinute = (int)day["time_from"];
                }

                int endMinute = 0;
                if (day["time_to"] != DBNull.Value)
                {
                    endMinute = (int)day["time_to"];
                    // next day
                    if (endMinute < startMinute && endMinute > 0)
                        endMinute = endMinute + 1440;

                    if (endMinute == 1440)
                        endMinute = 0;
                }

                var newRestriction = new AvailabilityRestriction();
                if (startMinute > 0)
                    newRestriction.StartTimeLimitation =
                        new StartTimeLimitation(new TimeSpan(0, startMinute, 0), null);
                else
                {
                    newRestriction.StartTimeLimitation = new StartTimeLimitation();
                }
                if (endMinute > 0)
                    newRestriction.EndTimeLimitation =
                        new EndTimeLimitation(null, new TimeSpan(0, endMinute, 0));
                else
                {
                    newRestriction.EndTimeLimitation = new EndTimeLimitation();
                }
                newRestriction.NotAvailable = !(bool)day["available"];

                availability.AvailabilityDays[dayIdx].Restriction = newRestriction;
            }
            return availability;
        }
        private IEnumerable<DataRow> DaysOnAvailability(int coreId)
        {
            IList<DataRow> ret = new List<DataRow>();
            foreach (DataRow row in _availabilityDays)
            {
                if ((int)row["core_id"] == coreId)
                    ret.Add(row);
            }
            return ret;
        }
        private IPerson FindPerson(int oldIdOnPerson)
        {
            IPerson ret = null;
            foreach (ObjectPair<Agent, IPerson> pair in MappedObjectPair.Agent)
            {
                if (pair.Obj1.Id == oldIdOnPerson)
                    ret = pair.Obj2;
            }
            return ret;
        }
    }
}
