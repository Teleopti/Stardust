using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeBusinessRuleProvider:IBusinessRuleProvider
	{
		private INewBusinessRuleCollection _businessRuleCollection;

		public void SetBusinessRules(INewBusinessRuleCollection businessRuleCollection)
		{
			_businessRuleCollection = businessRuleCollection;
		}
		public INewBusinessRuleCollection GetAllBusinessRules(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return _businessRuleCollection;
		}
	}
}
