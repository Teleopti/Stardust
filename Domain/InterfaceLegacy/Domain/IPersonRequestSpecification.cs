namespace Teleopti.Interfaces.Domain
{
    public interface IPersonRequestSpecification<T>
    {
        IValidatedRequest IsSatisfied(T obj);
    }
}
