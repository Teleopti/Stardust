using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    /// <summary>
    /// Logic for checking if a part shifttrade is ok
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-05-27
    /// </remarks>
    public interface IShiftTradeSpecification :ISpecification<IList<IShiftTradeSwapDetail>>
    {
      
        /// <summary>
        /// Verifies the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-05-27
        /// </remarks>
        ShiftTradeRequestValidationResult Validate(IList<IShiftTradeSwapDetail> obj);

        /// <summary>
        /// Gets the DenyReason (the explanation why/if the shifttrade wasnt alowed).
        /// </summary>
        /// <value>The deny reason.</value>
        /// <remarks>
        /// This returns the key, not the translated string
        /// Created by: henrika
        /// Created date: 2010-05-27
        /// </remarks>
        string DenyReason { get; }
    }


    //Henrik 2010-05-26 Just keeping these separate for now, will refact to list....
    /// <summary>
    /// Checks that the WorkflowControlSet isnt null
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-05-26'
    /// Henrik 2010-05-26 Just keeping these separate for now, will refact to list....
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IIsWorkflowControlSetNullSpecification : IShiftTradeSpecification
    {

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IOpenShiftTradePeriodSpecification : IShiftTradeSpecification
    {

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IShiftTradeSkillSpecification : IShiftTradeSpecification
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IShiftTradeTargetTimeSpecification : IShiftTradeSpecification
    {

    }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IShiftTradeAbsenceSpecification : IShiftTradeSpecification
	{
		
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IShiftTradePersonalActivitySpecification : IShiftTradeSpecification
	{
		
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IShiftTradeMeetingSpecification : IShiftTradeSpecification
	{
		
	}
}