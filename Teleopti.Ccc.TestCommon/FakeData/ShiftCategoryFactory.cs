using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for ShiftCategory domain object
    /// </summary>
    public static class ShiftCategoryFactory
    {
        /// <summary>
        /// Creates a ShiftCategory
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ShiftCategory CreateShiftCategory(string name)
        {
            return CreateShiftCategory(name,Color.DeepSkyBlue.ToString());
        }

		public static ShiftCategory CreateShiftCategory()
		{
			return CreateShiftCategory("shiftCategory");
		}

        public static ShiftCategory CreateShiftCategory(string name, string displayColor)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            ShiftCategory myShiftCategory = new ShiftCategory(name);
            myShiftCategory.Description = new Description(name, name.Substring(0, 2));
            myShiftCategory.DisplayColor = Color.FromName(displayColor);
            return myShiftCategory;
        }
    }
}