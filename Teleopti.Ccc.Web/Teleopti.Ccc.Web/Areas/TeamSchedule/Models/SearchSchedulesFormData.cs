using System;
using System.Linq;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class SearchSchedulesFormData
	{
		public string Keyword { get; set; }
		public DateOnly Date { get; set; }
		public int PageSize { get; set; }
		public int CurrentPageIndex { get; set; }
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
		//public T ToModel<T>() where T :  SearchSchedulesInput
		//{
		//	var model = Activator.CreateInstance<T>();
		//	model.key
		//	var isIds = SelectedGroupIds.All(id =>
		//	{
		//		Guid value;
		//		return Guid.TryParse(id, out value);
		//	});
		//	if (isIds)
		//	{

		//	}
		//}
	}

}