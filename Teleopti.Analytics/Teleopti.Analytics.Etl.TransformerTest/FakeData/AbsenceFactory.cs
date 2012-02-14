using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
    public static class AbsenceFactory
    {
        public static IList<IAbsence> CreateAbsenceCollection()
        {
            IList<IAbsence> retList = new List<IAbsence>();

            IAbsence absence = new Absence();
            absence.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetCreatedOn(absence, DateTime.Now);
            absence.Description = new Description("Sick Leave");
            absence.DisplayColor = Color.ForestGreen;
            retList.Add(absence);

            absence = new Absence();
            absence.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetCreatedOn(absence, DateTime.Now);
            absence.Description = new Description("parentalLeave");
            absence.DisplayColor = Color.DeepPink;
            retList.Add(absence);

            absence = new Absence();
            absence.SetId(Guid.NewGuid());
            RaptorTransformerHelper.SetCreatedOn(absence, DateTime.Now);
            absence.Description = new Description("Deleted absence");
            absence.DisplayColor = Color.LemonChiffon;
            ((Absence)absence).SetDeleted();
            retList.Add(absence);

            return retList;
        }
    }
}