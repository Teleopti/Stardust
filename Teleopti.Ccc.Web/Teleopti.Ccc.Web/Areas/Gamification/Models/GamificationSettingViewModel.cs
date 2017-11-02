using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Gamification.Models
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage)]
	
	public class GamificationSettingViewModel: GamificationSettingView
	{
		public GamificationSettingViewModel(IGamificationSetting setting) : base(setting)
		{
		}

		public Guid Id { get; set; }
	}
	
	public class GamificationDescriptionViewMode
	{
		public Guid GamificationSettingId { get; set; }
		public Description Value { get; set; }
	}

	public class GamificationAnsweredCallsEnabledViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public bool Value { get; set; }
	}
}