namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// OvertimeShiftActivityLayer
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-02-05
    /// </remarks>
	public interface IOvertimeShiftActivityLayer : ILayer<IActivity>, IAggregateEntity  //, IPersistedLayer<IActivity>
    {
		IMultiplicatorDefinitionSet DefinitionSet { get; }
    }
}
