using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
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