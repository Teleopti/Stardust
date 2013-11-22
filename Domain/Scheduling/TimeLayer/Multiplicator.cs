using System.Drawing;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.TimeLayer
{
    public class Multiplicator : VersionedAggregateRootWithBusinessUnit, IMultiplicator, IDeleteTag
    {
        private Description _description;
        private Color _displayColor = Color.Cornsilk;
        private MultiplicatorType _multiplicatorType;
        private double _multiplicatorValue;
        private string _exportCode = string.Empty;
        private bool _isDeleted;

        protected Multiplicator() { }

        public Multiplicator(MultiplicatorType multiplicatorType)
            : this()
        {
            _multiplicatorType = multiplicatorType;
            _description = new Description();
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets the display color.
        /// </summary>
        /// <value>The display color.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        public virtual Color DisplayColor
        {
            get { return _displayColor; }
            set { _displayColor = value; }
        }

        /// <summary>
        /// Gets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        public virtual MultiplicatorType MultiplicatorType
        {
            get { return _multiplicatorType; }
            set { _multiplicatorType = value; }
        }

        /// <summary>
        /// Gets or sets the multiplicator value.
        /// </summary>
        /// <value>The multiplicator value.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        public virtual double MultiplicatorValue
        {
            get { return _multiplicatorValue; }
            set { _multiplicatorValue = value; }
        }

        /// <summary>
        /// Gets or sets the export code. Typicaly used by pay roll reporting
        /// </summary>
        /// <value>The export code.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-08
        /// </remarks>
        public virtual string ExportCode
        {
            get { return _exportCode; }
            set { _exportCode = value; }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual IMultiplicator NoneEntityClone()
        {
            Multiplicator retObj = (Multiplicator)MemberwiseClone();
            retObj.SetId(null);

            return retObj;
        }

        public virtual IMultiplicator EntityClone()
        {
            Multiplicator retObj = (Multiplicator)MemberwiseClone();
            
            return retObj;
        }

        public virtual object Clone()
        {
            return EntityClone();
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
