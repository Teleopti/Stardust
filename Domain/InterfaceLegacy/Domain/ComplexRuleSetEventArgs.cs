﻿using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Used when sending an event that a rule set is too complex to be used
	/// </summary>
	public class ComplexRuleSetEventArgs : EventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ruleSetName"></param>
		public ComplexRuleSetEventArgs(string ruleSetName)
		{
			RuleSetName = ruleSetName;
		}

		/// <summary>
		/// 
		/// </summary>
		public string RuleSetName { get; set; }
	}
}