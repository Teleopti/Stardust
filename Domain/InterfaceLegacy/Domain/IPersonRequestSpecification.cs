namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface IPersonRequestSpecification<T>
    {
        IValidatedRequest IsSatisfied(T obj);
    }
}
