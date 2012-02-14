using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData

{
    public static class ActivityFactory
    {
        public static IList<IActivity> CreateActivityCollection()
        {
            IList<IActivity> retList = new List<IActivity>();

            Activity activity = Ccc.TestCommon.FakeData.ActivityFactory.CreateActivity("Phone", Color.Green);
            ((IEntity) activity).SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetCreatedOn(activity, DateTime.Now);
            activity.InReadyTime = true;
            retList.Add(activity);

            activity = Ccc.TestCommon.FakeData.ActivityFactory.CreateActivity("Lunch Break", Color.Yellow);
            ((IEntity) activity).SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetCreatedOn(activity, DateTime.Now);
            retList.Add(activity);

            activity = Ccc.TestCommon.FakeData.ActivityFactory.CreateActivity("Short Break", Color.Red);
            ((IEntity) activity).SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetCreatedOn(activity, DateTime.Now);
            retList.Add(activity);

            activity = Ccc.TestCommon.FakeData.ActivityFactory.CreateActivity("Deleted activity", Color.Red);
            ((IEntity)activity).SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetCreatedOn(activity, DateTime.Now);
            activity.SetDeleted();
            retList.Add(activity);

            return retList;
        }
    }
}