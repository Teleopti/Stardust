using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Handles converting from 6x to new format of Absence
    /// </summary>
    public class AbsenceMapper : Mapper<IAbsence, global::Domain.Absence>
    {
        private readonly IList<DataRow> _confidentialRows;

        public AbsenceMapper(MappedObjectPair mappedObjectPair, IList<DataRow> confidentialRows)
                            : base(mappedObjectPair, null)
        {
            _confidentialRows = confidentialRows;

        }

        public override IAbsence Map(global::Domain.Absence oldEntity)
        {
            Absence newAbsence = null;
            if (!oldEntity.UseCountRules)
            {
                string oldName = oldEntity.Name;
                if (String.IsNullOrEmpty(oldName))
                    oldName = MissingData.Name;

                while (newAbsence == null)
                {
                    newAbsence = new Absence();
                    try
                    {
                        newAbsence.Description = new Description(oldName, oldEntity.ShortName);
                    }
                    catch (ArgumentException)
                    {
                        oldName = oldName.Remove(oldName.Length - 1);
                        newAbsence = null;
                    }
                }
                newAbsence.DisplayColor = oldEntity.LayoutColor;
                newAbsence.InContractTime = oldEntity.InWorkTime;
                newAbsence.Requestable = oldEntity.Vacation;
                newAbsence.InWorkTime = oldEntity.InWorkTime;
                newAbsence.InPaidTime = oldEntity.PaidTime;
                newAbsence.Confidential = isConfidential(oldEntity);
                if (oldEntity.Deleted)
                    ((IDeleteTag)newAbsence).SetDeleted();
                if(oldEntity.AbsenceActivity != null)
                {
                   MappedObjectPair.AbsenceActivity.Add(oldEntity, oldEntity.AbsenceActivity);
                }
            }
            
            return newAbsence;
        }

        private bool isConfidential(global::Domain.Absence oldAbsence)
        {
            foreach (var row in _confidentialRows)
            {
                if ((int)row["abs_id"] == oldAbsence.Id)
                {
                    var confDesc = row["private_desc"].ToString();
                    return !string.IsNullOrEmpty(confDesc);
                }
            }
            return false;
        }
    }
}