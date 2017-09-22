using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
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

            IShiftCategory shiftCategory = createShiftCategory("Early Start", "ES", Color.DeepPink);
            retList.Add(shiftCategory);
            shiftCategory = createShiftCategory("Mid Start", "MS", Color.DarkGreen);
            retList.Add(shiftCategory);
            shiftCategory = createShiftCategory("Late Start", "LS", Color.Yellow);
            retList.Add(shiftCategory);
            shiftCategory = createShiftCategory("Deleted SC", "DS", Color.Yellow);
            ((ShiftCategory)shiftCategory).SetDeleted();
            retList.Add(shiftCategory);

            return retList;
        }

        // Private Methods (1)

        private static IShiftCategory createShiftCategory(string name, string shortName, Color displayColor)
        {
            var sc = new ShiftCategory(name);
            sc.Description = new Description(name, shortName);
            sc.DisplayColor = displayColor;
            ((IEntity) sc).SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(sc, DateTime.Now);

            return sc;
        }

        #endregion Methods
    }
}
