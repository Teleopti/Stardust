﻿using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Payroll
{
    public class GridViewBaseModel<T> : ViewModel<T>, IGridViewBaseModel where T : IMultiplicatorDefinition
    {
        private readonly T _containedEntity;

        protected GridViewBaseModel(T definition) : base(definition)
        {
            _containedEntity = definition;
        }

        public IList<IMultiplicator> MultiplicatorCollection { get; set; }

        /// <summary>
        /// Gets the multiplicator.
        /// </summary>
        /// <value>The multiplicator.</value>
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
    }
}
