﻿using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting.Events
{
	public class ShrinkageRowsDeleted : CompositePresentationEvent<IEnumerable<ICustomShrinkage>>
	{
	}
}