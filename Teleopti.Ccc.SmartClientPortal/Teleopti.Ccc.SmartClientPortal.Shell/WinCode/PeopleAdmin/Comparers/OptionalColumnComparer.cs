using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Compares the optional column data.
    /// </summary>
    /// <typeparam name="T">Type of the objects to be comapred</typeparam>
    /// <remarks>
    /// Created by: Aruna Priyankara Wickrama
    /// Created date: 8/12/2008
    /// </remarks>
	public class OptionalColumnComparer<T> : IComparer<T> where T : PersonGeneralModel, IEntity
    {
        private string _bindingProperty;

        /// <summary>
        /// Gets the binding property.
        /// </summary>
        /// <value>The binding property.</value>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 8/12/2008
        /// </remarks>
        public string BindingProperty
        {
            get { return _bindingProperty; }   
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalColumnComparer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="bindingProperty">The binding property.</param>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 8/12/2008
        /// </remarks>
        public OptionalColumnComparer(string bindingProperty)
        {
            _bindingProperty = bindingProperty;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value Condition Less than zero<paramref name="x"/> 
        /// is less than <paramref name="y"/>.Zero<paramref name="x"/> 
        /// equals <paramref name="y"/>.Greater than zero<paramref name="x"/> 
        /// is greater than <paramref name="y"/>.
        /// </returns>
        /// <remarks>
        /// Created by: Aruna Priyankara Wickrama
        /// Created date: 8/12/2008
        /// </remarks>
        public int Compare(T x, T y)
        {
            int result = 0;

            // Gets the relevant optional column from the column list
            IOptionalColumn selectedOptionalColumn = x.OptionalColumns.FirstOrDefault(p => p.Name == _bindingProperty);

            if (selectedOptionalColumn!=null)
            {
                // Gets the optional column value of object X and Y
            	IOptionalColumnValue valueX =
            		x.ContainedEntity.OptionalColumnValueCollection.FirstOrDefault(p => ((OptionalColumn)p.Parent).Name == _bindingProperty);
				IOptionalColumnValue valueY = y.ContainedEntity.OptionalColumnValueCollection.FirstOrDefault(p => ((OptionalColumn)p.Parent).Name == _bindingProperty);

                if ((valueX == null) && (valueY == null))
                {
                    result = 0;
                }
                else if (valueX == null)
                {
                    result = -1;
                }
                else if (valueY == null)
                {
                    result = 1;
                }
                else
                {
                    // compares the roles of the y with the roels of y
                    result = string.Compare(valueX.Description, valueY.Description,
                                            StringComparison.CurrentCulture);
                }
            }

            return result;
        }
    }
}
