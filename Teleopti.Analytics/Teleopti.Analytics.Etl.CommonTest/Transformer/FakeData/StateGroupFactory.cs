using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
    public static class StateGroupFactory
    {
		public static IList<IRtaStateGroup> CreateStateGroupList()
        {
			IRtaStateGroup stateGroup1 = new RtaStateGroup("test1", false, true);
			stateGroup1.SetId(Guid.NewGuid());

			IRtaStateGroup stateGroup2 = new RtaStateGroup("IsDefaultStateGroupe", true, true);
			stateGroup2.SetId(Guid.NewGuid());

			IRtaStateGroup stateGroup3 = new RtaStateGroup("DeletedStateGroup", false, true);
			stateGroup3.SetId(Guid.NewGuid());

			RaptorTransformerHelper.SetUpdatedOn(stateGroup1, DateTime.Now);
			RaptorTransformerHelper.SetUpdatedOn(stateGroup2, DateTime.Now);
			RaptorTransformerHelper.SetUpdatedOn(stateGroup3, DateTime.Now);

			return new List<IRtaStateGroup> { stateGroup1, stateGroup2, stateGroup3 };
        }
    }
}