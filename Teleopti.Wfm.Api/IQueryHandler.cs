namespace Teleopti.Wfm.Api
{
	public interface IQueryHandler<T, TResult> where T : IQueryDto
	{
		QueryResultDto<TResult> Handle(T command);
	}
}