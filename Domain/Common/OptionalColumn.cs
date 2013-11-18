using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class OptionalColumn : VersionedAggregateRootWithBusinessUnit, IOptionalColumn
    {
        protected OptionalColumn()
        {}

        public OptionalColumn(string name)
            : this()
        {
            _name = name;
        }

        private string _name;
        private string _tableName;
        
        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }
    }
}