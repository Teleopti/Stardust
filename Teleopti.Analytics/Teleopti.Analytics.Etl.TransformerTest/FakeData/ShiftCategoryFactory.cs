using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
    public sealed class ShiftCategoryFactory
    {
        #region Constructors (1)

        private ShiftCategoryFactory()
        {
        }

        #endregion Constructors

        #region Methods (2)

        // Public Methods (1)

        public static IList<IShiftCategory> CreateShiftCategoryCollection()
        {
            IList<IShiftCategory> retList = new List<IShiftCategory>();

            IShiftCategory shiftCategory = CreateShiftCategory("Early Start", "ES", Color.DeepPink);
            retList.Add(shiftCategory);
            shiftCategory = CreateShiftCategory("Mid Start", "MS", Color.DarkGreen);
            retList.Add(shiftCategory);
            shiftCategory = CreateShiftCategory("Late Start", "LS", Color.Yellow);
            retList.Add(shiftCategory);
            shiftCategory = CreateShiftCategory("Deleted SC", "DS", Color.Yellow);
            ((ShiftCategory)shiftCategory).SetDeleted();
            retList.Add(shiftCategory);

            return retList;
        }

        // Private Methods (1)

        private static IShiftCategory CreateShiftCategory(string name, string shortName, Color displayColor)
        {
            ShiftCategory sc = new ShiftCategory(name);
            sc.Description = new Description(name, shortName);
            sc.DisplayColor = displayColor;
            ((IEntity) sc).SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetCreatedOn(sc, DateTime.Now);

            return sc;
        }

        #endregion Methods
    }
}
