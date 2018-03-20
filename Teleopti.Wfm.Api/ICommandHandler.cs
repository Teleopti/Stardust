namespace Teleopti.Wfm.Api
{
	public interface ICommandHandler<T> where T : ICommandDto
	{
		ResultDto Handle(T command);
	}
}