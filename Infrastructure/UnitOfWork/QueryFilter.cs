using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class QueryFilter : IQueryFilter
	{
		public static readonly IQueryFilter BusinessUnit = new QueryFilter("businessUnitFilter"); 
		public static readonly IQueryFilter Deleted = new QueryFilter("deletedFlagFilter");

		private QueryFilter(string name)
		{
			InParameter.NotNull("name", name);
			Name = name;
		}

		public string Name { get; private set; }

		public override bool Equals(object obj)
		{
			var castedObj = obj as QueryFilter;
			return castedObj != null && Name.Equals(castedObj.Name);
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}
}
