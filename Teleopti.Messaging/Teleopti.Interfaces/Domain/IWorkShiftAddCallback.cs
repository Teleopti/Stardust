using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// 
	/// </summary>
	public interface IWorkShiftAddCallback
	{
		/// <summary>
		/// Happen on every change of the number of shift generated
		/// </summary>
		event EventHandler<EventArgs> CountChanged;
		/// <summary>
		/// Happens when shift generations are ready
		/// </summary>
		event EventHandler<EventArgs> RuleSetReady;
		/// <summary>
		/// Happens when shift generations takes longer than 3 seconds
		/// </summary>
		event EventHandler<EventArgs> RuleSetWarning;
		/// <summary>
		/// Happens when shift generations takes longer than 60 seconds.
		/// The ruleset will not be used in this case.
		/// </summary>
		event EventHandler<ComplexRuleSetEventArgs> RuleSetToComplex;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		void BeforeAdd(IWorkShift item);
		/// <summary>
		/// 
		/// </summary>
		void BeforeRemove();
		/// <summary>
		/// 
		/// </summary>
		bool IsCanceled { get; }
		/// <summary>
		/// 
		/// </summary>
		void Cancel();
		/// <summary>
		/// 
		/// </summary>
		int CurrentCount { get; }

		/// <summary>
		/// 
		/// </summary>
		string CurrentRuleSetName { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ruleSet"></param>
		void StartNewRuleSet(IWorkShiftRuleSet ruleSet);

		/// <summary>
		/// 
		/// </summary>
		void EndRuleSet();
	}
}