using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class RegisterAreas : IRegisterAreas
	{
		private readonly IRegisterArea _registerArea;
		private readonly IFindAreaRegistrations _findAreaRegistrations;

		public RegisterAreas(IRegisterArea registerArea, IFindAreaRegistrations findAreaRegistrations)
		{
			_registerArea = registerArea;
			_findAreaRegistrations = findAreaRegistrations;
		}

		public void Execute()
		{
			_findAreaRegistrations.AreaRegistrations()
				.ForEach(x => _registerArea.Register(x));
		}
	}
}