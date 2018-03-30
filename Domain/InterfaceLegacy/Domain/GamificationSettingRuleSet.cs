namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
	/// Types of AgentBadgeSettingRuleSet
    /// </summary>
   public enum GamificationSettingRuleSet
	{
		/// <summary>
		/// Rule with ratio convertor
		/// </summary>
		RuleWithRatioConvertor = 0,
		/// <summary>
		/// Rule with different threshold
		/// </summary>
		RuleWithDifferentThreshold = 1,
	}

	public enum GamificationRollingPeriodSet
	{
		OnGoing = 0,
		Weekly,
		Monthly
	}
}