using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.Global.Models
{
	public class SearchGroupIdsData
	{
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