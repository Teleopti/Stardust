using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Budgeting.Events
{
	public class DeleteCustomShrinkages : CompositePresentationEvent<IEnumerable<ICustomShrinkage>>
	{
	}
}