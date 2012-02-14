using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for converting shiftCategories from old version to new version
    /// </summary>
    public class ShiftCategoryMapper : Mapper<IShiftCategory, global::Domain.ShiftCategory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftCategoryMapper"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public ShiftCategoryMapper(MappedObjectPair mappedObjectPair) 
                            : base(mappedObjectPair, null)
        {
        }


        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public override IShiftCategory Map(global::Domain.ShiftCategory oldEntity)
        {
            IShiftCategory newShiftCat;
            string oldName = oldEntity.Name;

            int oldNameLength = (oldName.Length > 50) ? 50 : oldName.Length;
            if(oldNameLength==0)
                newShiftCat = new ShiftCategory(MissingData.Name);
            else
                newShiftCat = new ShiftCategory(oldName.Substring(0, oldNameLength));
            newShiftCat.DisplayColor = oldEntity.ColorLayout;

            int oldShortNameLength = (oldEntity.ShortName.Length > 50) ? 50 : oldEntity.ShortName.Length;
            newShiftCat.Description =
                new Description(newShiftCat.Description.Name, oldEntity.ShortName.Substring(0, oldShortNameLength));
            if (!oldEntity.InUse)
                ((IDeleteTag)newShiftCat).SetDeleted();

            mapJusticeValues(newShiftCat, oldEntity.FairnessValue);

            return newShiftCat;
        }

        private static void mapJusticeValues(IShiftCategory newShiftCat, int justiceValue)
        {
            for (int i = 0; i < 7; i++)
            {
                newShiftCat.DayOfWeekJusticeValues[(DayOfWeek)i] = justiceValue;
            }
        }
    }
}