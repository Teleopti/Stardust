using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for MainShift domain object
    /// </summary>
    public static class MainShiftFactory
    {
        /// <summary>
        /// Creates a main shift.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="period">The period.</param>
        /// <param name="category">The category.</param>
        /// <returns></returns>
        public static MainShift CreateMainShift(IActivity activity, DateTimePeriod period, IShiftCategory category)
        {
            MainShift shift = new MainShift(category);
            MainShiftActivityLayer layer = new MainShiftActivityLayer(activity, period);
            shift.LayerCollection.Add(layer);
            return shift;
        }

        /// <summary>
        /// Creates a shift with default category
        /// </summary>
        /// <returns></returns>
        public static MainShift CreateMainShiftWithDefaultCategory()
        {
            ShiftCategory category = new ShiftCategory("Morning");
            MainShift shift = new MainShift(category);
            return shift;
        }

        /// <summary>
        /// Creates the main shift.
        /// </summary>
        /// <param name="cat">The cat.</param>
        /// <returns></returns>
        public static MainShift CreateMainShift(IShiftCategory cat)
        {
            MainShift shift = new MainShift(cat);
            return shift;
        }


        /// <summary>
        /// Creates mainshift with three activity layers.
        /// </summary>
        /// <returns></returns>
        public static MainShift CreateMainShiftWithThreeActivityLayers()
        {
            Activity telephone = ActivityFactory.CreateActivity("Tel");
            DateTimePeriod period1 =
                new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc));

            Activity longDuty = ActivityFactory.CreateActivity("Long duty");
            DateTimePeriod period2 =
                new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 2, 1, 0, 0, DateTimeKind.Utc));

            DateTimePeriod period3 =
                new DateTimePeriod(new DateTime(2007, 1, 2, 1, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 2, 2, 0, 0, DateTimeKind.Utc));


            MainShiftActivityLayer layer1 = new MainShiftActivityLayer(telephone, period1);
            MainShiftActivityLayer layer2 = new MainShiftActivityLayer(longDuty, period2);
            MainShiftActivityLayer layer3 = new MainShiftActivityLayer(longDuty, period3);

            MainShift resultShift = CreateMainShift(ShiftCategoryFactory.CreateShiftCategory("TEL"));
            resultShift.LayerCollection.Add(layer1);
            resultShift.LayerCollection.Add(layer2);
            resultShift.LayerCollection.Add(layer3);

            return resultShift;
        }
    }
}