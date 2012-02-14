using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll
{
    public class GridViewBaseModel<T> : ViewModel<T>, IGridViewBaseModel where T : IMultiplicatorDefinition
    {
        #region Fields - Instance Members

        #region Fields - Instance Members - Private Fields

        private T _containedEntity;

        #endregion

        #endregion

        #region Properties -Instance Members

        public IList<IMultiplicator> MultiplicatorCollection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the multiplicator.
        /// </summary>
        /// <value>The multiplicator.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-04-15
        /// </remarks>
        public IMultiplicator Multiplicator
        {
            get
            {
                return _containedEntity.Multiplicator;
            }
            set
            {
                if (value.Id.HasValue)
                {
                    _containedEntity.Multiplicator = value;
                }
            }
        }
        #endregion

        #region Methods - Instance Members

        #region Methods - Instance Members - Constructors

        public GridViewBaseModel(T definition)
            : base(definition)
        {
            _containedEntity = definition;
        }
        
        #endregion

        #endregion
    }
}
