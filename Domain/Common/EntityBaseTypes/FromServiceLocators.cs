using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
	public class FromServiceLocators : IPopEventsContext
	{
		public INow Now => ServiceLocator_DONTUSE.Now;
	}
}