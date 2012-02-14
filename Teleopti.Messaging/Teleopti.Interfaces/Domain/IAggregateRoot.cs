namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Interface for aggregate roots.
    /// These types are the ONLY entity types allowed 
    /// to be referenced from outside its own aggregate.
    ///</summary>
    public interface IAggregateRoot : IEntity
    {

    }
}