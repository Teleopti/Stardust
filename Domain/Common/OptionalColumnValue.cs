using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Represents a optional column value
    /// </summary>
    public class OptionalColumnValue : AggregateEntity, IOptionalColumnValue
    {
		private string _description;
		private IEntity _referenceObject;

        protected OptionalColumnValue()
        {
        }

        public OptionalColumnValue(string description)
            : this()
        {
            _description = description;
        }

        public virtual string Description
        {
            get { return _description; }
            set { _description = value; }
        }

		public virtual IEntity ReferenceObject
		{
			get { return _referenceObject; }
			set { _referenceObject = value; }
		}
    }
}
