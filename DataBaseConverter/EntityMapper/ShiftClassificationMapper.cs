using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for converting shiftClasses from old version to new version
    /// </summary>
    public class ShiftClassificationMapper : Mapper<ShiftClassification, global::Domain.ShiftClass>
    {
        private ICollection<global::Domain.WorkShift> _workShifts;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftClassificationMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="workShifts">The work shifts.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public ShiftClassificationMapper(MappedObjectPair mappedObjectPair, TimeZoneInfo timeZone, ICollection<global::Domain.WorkShift> workShifts)
            : base(mappedObjectPair, timeZone)
        {
            _workShifts = workShifts;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public override ShiftClassification Map(global::Domain.ShiftClass oldEntity)
        {
            ShiftClassification newShiftClassification;
            string oldName = oldEntity.Name;

            ShiftCategory category = MappedObjectPair.ShiftCategory.GetPaired(oldEntity.Category);
            Site site = MappedObjectPair.Site.GetPaired(oldEntity.Unit);

            IList<Site> sites = new List<Site>() { site };

            //TODO: Creation of contracts should maybe be based on EmploymentType in old domain
            IList<Contract> contracts = MappedObjectPair.Contract.Obj2Collection().Distinct().ToList();
            //foreach (ObjectPair<global::Domain.WorktimeType, Contract> contractInfo in MappedObjectPair.Contract.Obj2Collection())
                //if (!contracts.Contains(contractInfo.Obj2)) contracts.Add(contractInfo.Obj2);

            int oldNameLength = (oldName.Length > 50) ? 50 : oldName.Length;
            newShiftClassification = new ShiftClassification(oldName.Substring(0, oldNameLength),category,sites,contracts);
            newShiftClassification.UseSiteTimeZone = true; //Default behavior, time is related to the site's time zone. Not to UTC.

            if (oldEntity.StartAndLengthDefinition!=null)
            {
                newShiftClassification.StartScope =
                new TimePeriod(oldEntity.StartAndLengthDefinition.EarliestStartTime,
                               oldEntity.StartAndLengthDefinition.LatestStartTime);

                newShiftClassification.StartSegment = oldEntity.StartAndLengthDefinition.StartTimeSegment;

                newShiftClassification.LengthScope =
                new TimePeriod(oldEntity.StartAndLengthDefinition.MinLength,
                               oldEntity.StartAndLengthDefinition.MaxLength);

                //Do we have the wrong name?
                newShiftClassification.LengthSegment = oldEntity.StartAndLengthDefinition.EndTimeSegment;
            }

            newShiftClassification.EndScope = new TimePeriod(oldEntity.EarliestEnd, oldEntity.LatestEnd);
            newShiftClassification.EndSegment = oldEntity.EndSegment;

            if (_workShifts != null)
            {
                WorkShiftMapper workShiftMapper = new WorkShiftMapper(MappedObjectPair, TimeZone);
                IList<global::Domain.WorkShift> oldWorkShiftsForClassification = _workShifts.Where(w => w.Bag == oldEntity).ToList();

                Console.WriteLine("Number of WorkShifts for ShiftClass {0}: {1}", oldName,oldWorkShiftsForClassification.Count);
                foreach (global::Domain.WorkShift oldWorkShift in oldWorkShiftsForClassification)
                {
                    WorkShift newWorkShift = workShiftMapper.Map(oldWorkShift);
                    if (newWorkShift != null) newShiftClassification.AddWorkShift(newWorkShift);
                }
            }

            return newShiftClassification;
        }
    }
}