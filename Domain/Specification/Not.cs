namespace Teleopti.Ccc.Domain.Specification
{
	public class Not<T> : Specification<T>
	{
		public override bool IsSatisfiedBy(T obj)
		{
			return false;
		}
	}
}