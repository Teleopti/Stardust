namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A layer of absence.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-07
    /// </remarks>
	public interface IAbsenceLayer : ILayer<IAbsence>, IAggregateEntity, ICloneableEntity<ILayer<IAbsence>>
    {
    }
}
