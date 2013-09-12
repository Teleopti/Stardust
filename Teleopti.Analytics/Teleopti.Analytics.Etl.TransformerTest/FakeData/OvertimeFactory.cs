﻿using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
	public static class OvertimeFactory
	{
		public static IList<IMultiplicatorDefinitionSet> CreateMultiplicatorDefinitionSetList()
		{
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet1 = new MultiplicatorDefinitionSet("MDS 1",
			                                                                                        MultiplicatorType.Overtime);
			multiplicatorDefinitionSet1.SetId(Guid.NewGuid());
			RaptorTransformerHelper.SetUpdatedOn(multiplicatorDefinitionSet1,DateTime.Now);
			
			IMultiplicatorDefinitionSet multiplicatorDefinitionSet2 = new MultiplicatorDefinitionSet("MDS 2",
																									MultiplicatorType.Overtime);
			multiplicatorDefinitionSet2.SetId(Guid.NewGuid());
			RaptorTransformerHelper.SetUpdatedOn(multiplicatorDefinitionSet2, DateTime.Now);
			((IDeleteTag) multiplicatorDefinitionSet2).SetDeleted();

			IMultiplicatorDefinitionSet multiplicatorDefinitionSet3 = new MultiplicatorDefinitionSet("MDS 3",
																									MultiplicatorType.OBTime);
			multiplicatorDefinitionSet3.SetId(Guid.NewGuid());
			RaptorTransformerHelper.SetUpdatedOn(multiplicatorDefinitionSet3, DateTime.Now);

			return new List<IMultiplicatorDefinitionSet> {multiplicatorDefinitionSet1, multiplicatorDefinitionSet2, multiplicatorDefinitionSet3};
		}
	}
}
