using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class OptionalColumn : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IOptionalColumn
	{
		protected OptionalColumn()
		{ }

		public OptionalColumn(string name)
			 : this()
		{
			_name = name;
		}

		private string _name;
		private bool _availableAsGroupPage;
		private string _tableName;

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual bool AvailableAsGroupPage
		{
			get { return _availableAsGroupPage; }
			set { _availableAsGroupPage = value; }
		}

		public virtual string TableName
		{
			get { return _tableName; }
			set { _tableName = value; }
		}
	}
}