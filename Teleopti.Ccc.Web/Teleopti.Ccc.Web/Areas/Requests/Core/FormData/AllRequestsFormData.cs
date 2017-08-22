using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class AllRequestsFormData
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public IList<RequestsSortingOrder> SortingOrders { get; set; }
		public IDictionary<PersonFinderField, string> AgentSearchTerm { get; set; }
		public IDictionary<RequestFilterField, string> Filters { get; set; }
		public Paging Paging { get; set; }
		public string[] SelectedGroupIds { get; set; }
		public string[] DynamicOptionalValues
		{
			get
			{
				if (!isGuids())
					return SelectedGroupIds;
				return new string[0];
			}
		}

		public bool IsDynamic => DynamicOptionalValues.Any();

		public Guid[] GroupIds
		{
			get
			{
				if (isGuids())
					return SelectedGroupIds.Select(id => Guid.Parse(id)).ToArray();
				return new Guid[0];
			}
		}

		private bool isGuids()
		{
			return SelectedGroupIds.All(id =>
			{
				Guid value;
				return Guid.TryParse(id, out value);
			});
		}
	}
}