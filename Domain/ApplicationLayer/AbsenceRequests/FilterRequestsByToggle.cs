using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public interface IFilterRequests
	{
		List<IPersonRequest> Filter(List<IPersonRequest> personRequests);
	}


	public class FilterOutRequestsHandledByReadmodel : IFilterRequests
	{
		private readonly IAbsenceRequestSetting _absenceRequestSetting;
		private readonly INow _now;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;

		public FilterOutRequestsHandledByReadmodel(IAbsenceRequestSetting absenceRequestSetting, INow now, IAbsenceRequestValidatorProvider absenceRequestValidatorProvider)
		{
			_absenceRequestSetting = absenceRequestSetting;
			_now = now;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
		}

		public List<IPersonRequest> Filter(List<IPersonRequest> personRequests)
		{
			var immediatePeriodInHours = _absenceRequestSetting.ImmediatePeriodInHours;
			var requestEndTime = _now.UtcDateTime().AddHours(immediatePeriodInHours);

			var requestsUsingBudget = new List<IPersonRequest>();
			foreach (var pr in personRequests)
			{
				var mergedPeriod = pr.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod(pr.Request as IAbsenceRequest);
				var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);
				if(validators.Any(x => (x.GetType() == typeof(BudgetGroupAllowanceValidator) || x.GetType() == typeof(BudgetGroupHeadCountValidator))) || 
					pr.Request.Period.EndDateTime >= requestEndTime)
				{
					requestsUsingBudget.Add(pr);
				}
			}

			return requestsUsingBudget;
		}
	}

	public class NoFilterCheckRequests : IFilterRequests
	{
		public List<IPersonRequest> Filter(List<IPersonRequest> personRequests)
		{
			return personRequests;
		}
	}


}
