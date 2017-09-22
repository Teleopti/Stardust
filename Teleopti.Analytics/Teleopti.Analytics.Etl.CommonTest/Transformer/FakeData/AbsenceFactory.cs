using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
    public static class AbsenceFactory
    {
        public static IList<IAbsence> CreateAbsenceCollection()
        {
            IList<IAbsence> retList = new List<IAbsence>();

            IAbsence absence = new Absence();
            absence.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(absence, DateTime.Now);
            absence.Description = new Description("Sick Leave");
            absence.DisplayColor = Color.ForestGreen;
            retList.Add(absence);

            absence = new Absence();
            absence.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(absence, DateTime.Now);
            absence.Description = new Description("parentalLeave");
            absence.DisplayColor = Color.DeepPink;
            retList.Add(absence);

            absence = new Absence();
            absence.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetUpdatedOn(absence, DateTime.Now);
            absence.Description = new Description("Deleted absence");
            absence.DisplayColor = Color.LemonChiffon;
            ((Absence)absence).SetDeleted();
            retList.Add(absence);

            return retList;
        }
    }
}