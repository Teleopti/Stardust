using System;
using System.Data;
using Domain;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Mapper for PersonAvailability
    /// </summary>
    /// <remarks>
    /// Created by: ZoeT
    /// Created date: 2008-12-05
    /// </remarks>
    public class PersonAvailabilityMapper : Mapper<IPersonAvailability, DataRow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAvailabilityMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public PersonAvailabilityMapper(MappedObjectPair mappedObjectPair)
            : base(mappedObjectPair, null)
        {
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ZoeT
        /// Created date: 2008-12-05
        /// </remarks>
        public override IPersonAvailability Map(DataRow oldEntity)
        {
            int availabilityId = (int)oldEntity["core_id"];
            int empId = (int)oldEntity["emp_id"];
            DateTime startDate = (DateTime)oldEntity["date_from"];

            IAvailabilityRotation availability = FindAvailability(availabilityId);
            IPerson person = FindPerson(empId);
	        return person == null ? 
							null : 
							new PersonAvailability(person, availability, new DateOnly(startDate));
        }

        private IPerson FindPerson(int oldIdOnPerson)
        {
            IPerson ret = null;
            foreach (ObjectPair<Agent, IPerson> pair in MappedObjectPair.Agent)
            {
                if (pair.Obj1.Id == oldIdOnPerson)
                    ret = pair.Obj2;
            }
            return ret;
        }

        private IAvailabilityRotation FindAvailability(int oldIdOnAvailability)
        {
            IAvailabilityRotation availability = null;
            foreach (ObjectPair<DataRow, IAvailabilityRotation> pair in MappedObjectPair.Availability)
            {
                if ((int)pair.Obj1["core_id"] == oldIdOnAvailability)
                    availability = pair.Obj2;
            }
            return availability;
        }
    }
}
