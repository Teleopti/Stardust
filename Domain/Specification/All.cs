
namespace Teleopti.Ccc.Domain.Specification
{
    public class All<T> : Specification<T>
    {
        public override bool IsSatisfiedBy(T obj)
        {
            return true;
        }
    }
}