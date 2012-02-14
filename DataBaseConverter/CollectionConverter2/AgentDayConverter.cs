using System;
using Domain;
using Infrastructure;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using AgentAssignment=Teleopti.Ccc.Domain.Scheduling.AgentAssignment;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter2
{
    /// <summary>
    /// Converts the specified AgentDays
    /// </summary>
    public class AgentDayConverter
    {
        private readonly DateTime _fromDate;
        private readonly DateTime _toDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentDayConverter"/> class.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        public AgentDayConverter(DateTime fromDate, DateTime toDate)
        {
            _fromDate = fromDate;
            _toDate = toDate;
        }


        /// <summary>
        /// Converts the specified AgentDays, only agent days with workshift will be saved..
        /// </summary>
        /// <param name="assRep"></param>
        /// <param name="absRep"></param>
        /// <param name="dayOffRep"></param>
        /// <param name="availRep"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="oldReader"></param>
        /// <param name="assignmentConverter"></param>
        /// <param name="absConverter"></param>
        /// <param name="dayOffConverter"></param>
        /// <param name="availConverter"></param>
        /// <returns></returns>
        public int Convert(IRepository<AgentAssignment> assRep,
                           IRepository<AgentAbsence> absRep,
                           IRepository<AgentDayOff> dayOffRep,
                           IRepository<AgentAvailability> availRep,
                           IUnitOfWork unitOfWork,
                           IAgentDayReader oldReader,
                           Mapper<AgentAssignment, AgentDay> assignmentConverter,
                           Mapper<AgentAbsence, AgentDay> absConverter,
                           Mapper<AgentDayOff, AgentDay> dayOffConverter,
                           Mapper<AgentAvailability, AgentDay> availConverter)
        {

            int numConversions = 0;
            foreach (AgentDay theOld in oldReader.LoadAgentDays(new DatePeriod(_fromDate, _toDate)))
            {
                AgentAssignment theNew = assignmentConverter.Map(theOld);
                AgentAbsence theNewAgAbs = absConverter.Map(theOld);
                AgentDayOff theNewAgDayOff = dayOffConverter.Map(theOld);
                AgentAvailability theNewAgAvailability = availConverter.Map(theOld);
                if (theNew != null)
                {
                    if (theNew.MainShift != null || theNew.PersonalShiftCollection.Count > 0)
                    {
                        assRep.Add(theNew);
                        numConversions += 1;
                    }
                }
                if (theNewAgAbs != null)
                {
                    absRep.Add(theNewAgAbs);
                    numConversions += 1;
                }
                if (theNewAgDayOff != null)
                {
                    dayOffRep.Add(theNewAgDayOff);
                    numConversions += 1;
                }
                if (theNewAgAvailability != null)
                {
                    availRep.Add(theNewAgAvailability);
                    numConversions += 1;
                }
            }
            unitOfWork.PersistAll();
            return numConversions;
        }
    }
}