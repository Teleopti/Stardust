#region Imports

using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Represents the integer column comparer of the person account view.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-08
    /// </remarks>
    public class PersonAccountIntegerTimeSpanColumnComparer : IComparer<IPersonAccountModel>
    {
        #region Fields - Instance Member

        /// <summary>
        /// Holds the binding proeprty of the relevant column.
        /// </summary>
        private string _bindingProperty;

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - PersonalAccountGridView Members

        #region Methods - Instance Member - PersonalAccountGridView Members - (Constructors)

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAccountIntegerColumnComparer"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        public PersonAccountIntegerTimeSpanColumnComparer(string bindingProperty)
        {
            _bindingProperty = bindingProperty;
        }

        #endregion

        #region Methods - Instance Member - PersonalAccountGridView Members - (Methods)

        /// <summary>
        /// Compares the given binding property values of given objecets as double.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        private int CompareAsDouble(IPersonAccountModel x, IPersonAccountModel y)
        {
            int result = 0;
            double valueX = GetValue(x);
            double valueY = GetValue(y);

            if (valueX < valueY)
            {
                result = -1;
            }
            if (valueX > valueY)
            {
                result = 1;
            }

            return result;
        }

        /// <summary>
        /// Gets the bindign property value of the given obejct.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        private double GetValue(IPersonAccountModel item)
        {
            double value = 0;
            // Instantiates the property reflector
            PropertyReflector reflector = new PropertyReflector();

            Object objectValue = reflector.GetValue(item, _bindingProperty);

            if (objectValue is TimeSpan)
            {
                TimeSpan dataItem = (TimeSpan)objectValue;
                value = dataItem.TotalDays;
            }
            else
            {
                value = (double)Convert.ChangeType(objectValue, typeof(double), CultureInfo.InvariantCulture);
            }

            return value;
        }

        #endregion

        #endregion

        #region Methods - Instance Member - IComparer<IPersonAccountModel> Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, 
        /// equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        /// Condition
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        public int Compare(IPersonAccountModel x, IPersonAccountModel y)
        {
            int result = 0;

            if (x.CurrentAccount == null && y.CurrentAccount == null)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.CurrentAccount == null)
            {
                result = -1;
            }
            else if (y.CurrentAccount == null)
            {
                result = 1;
            }
            else
            {
                // Compares the values as strings.
                result = CompareAsDouble(x, y);
            }

            return result;
        }

        #endregion

        #endregion
    }
}
