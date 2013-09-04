namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IPersonalSettings<T>
	{
		void MapTo(T target);
		void MapFrom(T source);
	}
}

