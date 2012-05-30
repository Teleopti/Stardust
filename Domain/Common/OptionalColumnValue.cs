using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Represents a optional column value
    /// </summary>
    /// <remarks>
    /// Created by: Viraj Siriwardana
    /// Created date: 2008-07-24
    /// </remarks>
    public class OptionalColumnValue : AggregateEntity, IOptionalColumnValue
    {
		//private Guid? _referenceId;
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

		//public virtual Guid? ReferenceId
		//{
		//    get { return _referenceId; }
		//    set { _referenceId = value; }
		//}

		public virtual IEntity ReferenceObject
		{
			get { return _referenceObject; }
			set { _referenceObject = value; }
		}
    }
}
