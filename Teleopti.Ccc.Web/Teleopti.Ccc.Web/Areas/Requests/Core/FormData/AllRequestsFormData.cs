using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Search.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class AllRequestsFormData
	{
		public DateOnly StartDate;
		public DateOnly EndDate;
		public IList<RequestsSortingOrder> SortingOrders = new List<RequestsSortingOrder>();
		public IDictionary<PersonFinderField, string> AgentSearchTerm;
		public IDictionary<RequestFilterField, string> Filters;
		public Paging Paging = new Paging();
	}

	public class AllRequestsFormDataConverter : IModelBinder
	{
		public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if (bindingContext.ModelType != typeof (AllRequestsFormData))
			{
				return false;
			}

			var startDate = bindingContext.ValueProvider.GetValue("StartDate");
			var endDate = bindingContext.ValueProvider.GetValue("EndDate");
			var sortingOrders = bindingContext.ValueProvider.GetValue("SortingOrders");
			var agentSearchTerm = bindingContext.ValueProvider.GetValue("AgentSearchTerm");
			var filters = bindingContext.ValueProvider.GetValue("Filters");
			var take = bindingContext.ValueProvider.GetValue("Take");
			var skip = bindingContext.ValueProvider.GetValue("Skip");

			if (startDate == null || endDate == null)
			{
				return false;
			}

			DateTime startDateTime, endDateTime;

			if (startDate.RawValue == null || !DateTime.TryParse(startDate.AttemptedValue, out startDateTime)
				|| endDate.RawValue == null || !DateTime.TryParse(endDate.AttemptedValue, out endDateTime))
			{
				bindingContext.ModelState.AddModelError(
					bindingContext.ModelName, "Cannot convert value to all requests form data");
				return false;
			}


			bindingContext.Model = new AllRequestsFormData
			{
				StartDate = new DateOnly(startDateTime),
				EndDate = new DateOnly(endDateTime),
				SortingOrders = parseRequestsSortingOrders(sortingOrders),
				AgentSearchTerm = parseAgentSearchTerm(agentSearchTerm),
				Filters = parseRequestFilters(filters),
				Paging = parsePaging(take, skip)
			};
			return true;
		}

		private IList<RequestsSortingOrder> parseRequestsSortingOrders(ValueProviderResult result)
		{
			var sortingOrders = new List<RequestsSortingOrder>();
			if (result == null) return sortingOrders;

			var input = result.AttemptedValue;

			foreach (var orderString in input.Split(','))
			{
				RequestsSortingOrder sortingOrder;
				if (Enum.TryParse(orderString.Trim(), out sortingOrder))
				{
					sortingOrders.Add(sortingOrder);
				}
			}

			return sortingOrders;
		}

		private Paging parsePaging(ValueProviderResult take, ValueProviderResult skip)
		{
			if (take == null || skip == null)
			{
				return Paging.Nothing;
			}

			return new Paging
			{
				Skip = (int) skip.ConvertTo(typeof (int)),
				Take = (int) take.ConvertTo(typeof (int))
			};
		}

		private IDictionary<PersonFinderField, string> parseAgentSearchTerm(ValueProviderResult result)
		{
			return string.IsNullOrWhiteSpace(result?.AttemptedValue)
				? null
				: SearchTermParser.Parse(result.AttemptedValue);
		}


		private static IDictionary<RequestFilterField, string> parseRequestFilters(ValueProviderResult filters)
		{
			var filterString = filters?.AttemptedValue;
			if (string.IsNullOrWhiteSpace(filterString)) return null;

			var keyValuePairs = filterString.IndexOf(',') > -1
				? filterString.Split(',')
				: new[] {filterString};

			var result = new Dictionary<RequestFilterField, string>();
			foreach (var keyValuePair in keyValuePairs)
			{
				var trimmedKeyValuePair = keyValuePair.Trim('{', '}', ' ');
				var colonIndex = trimmedKeyValuePair.IndexOf(':');
				if (colonIndex < 0) continue;

				var key = trimmedKeyValuePair.Substring(0, colonIndex).Trim('\"', ' ');
				var value = trimmedKeyValuePair.Substring(colonIndex + 1).Trim('\"', ' ');
				RequestFilterField filterField;
				if (!Enum.TryParse(key, out filterField)) continue;

				result.Add(filterField, value);
			}

			return result;
		}
	}
}