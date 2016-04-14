using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IErrorAttachedCommand
	{
		IList<string> ErrorMessages { get; set; } 
	}
}