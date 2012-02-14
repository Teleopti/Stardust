using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Budgeting.Events
{
	public class ShrinkageRowsDeleted : CompositePresentationEvent<IEnumerable<ICustomShrinkage>>
	{
	}
}