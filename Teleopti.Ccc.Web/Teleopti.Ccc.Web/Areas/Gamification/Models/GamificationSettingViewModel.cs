using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Gamification.Models
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage)]
	
	public class GamificationSettingViewModel: GamificationSettingView
	{
		public GamificationSettingViewModel(IGamificationSetting setting) : base(setting)
		{
		}
	}
	
	public class GamificationDescriptionViewMode
	{
		public Guid GamificationSettingId { get; set; }
		public Description Value { get; set; }
	}

	public class GamificationThresholdEnabledViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public bool Value { get; set; }
	}

	public class GamificationAnsweredCallsThresholdViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public int Value { get; set; }
	}

	public class GamificationAHTThresholdViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public TimeSpan Value { get; set; }
	}

	public class GamificationAdherenceThresholdViewModel
	{
		public Guid GamificationSettingId { get; set; }
		public Percent Value { get; set; }
	}
}