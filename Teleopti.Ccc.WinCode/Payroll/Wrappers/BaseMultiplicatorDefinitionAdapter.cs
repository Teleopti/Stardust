using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll.Wrappers
{
    public abstract class BaseMultiplicatorDefinitionAdapter
    {
        #region Fields - Instance Members - Private Fields

        /// <summary>
        /// Holds the contained entity
        /// </summary>
        private IMultiplicatorDefinition _containedEntitity;

        #endregion
        
        #region Properties - Instance Members

        /// <summary>
        /// Protected constructor to prevent direct inititializatoin. 
        /// </summary>
        /// <param name="multiplicatorDefinition">The multiplicator definition.</param>
        protected BaseMultiplicatorDefinitionAdapter(IMultiplicatorDefinition multiplicatorDefinition)
        {
            _containedEntitity = multiplicatorDefinition;
        }

        /// <summary>
        /// Gets or sets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        public MultiplicatorType MultiplicatorType
        {
            get
            {
                return _containedEntitity.Multiplicator.MultiplicatorType;
            }
            set
            {
                _containedEntitity.Multiplicator.MultiplicatorType = value;
            }

        }

        /// <summary>
        /// Gets or sets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        public int OrderIndex
        {
            get
            {
                return _containedEntitity.OrderIndex;
            }

        }

        #endregion
    }
}
