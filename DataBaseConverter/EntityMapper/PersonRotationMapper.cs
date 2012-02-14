using System;
using System.Data;
using Domain;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Maps PersonRotations
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-09-25    
    /// /// </remarks>
    public class PersonRotationMapper : Mapper<IPersonRotation, DataRow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRotationMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-25    
        /// /// </remarks>
        public PersonRotationMapper(MappedObjectPair mappedObjectPair)
            : base(mappedObjectPair, null)
        {   
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
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-09-25    
        /// /// </remarks>
        public override IPersonRotation Map(DataRow oldEntity)
        {
            int rotationId = (int)oldEntity["rotation_id"];
            int empId = (int)oldEntity["emp_id"];
            DateTime startDate = (DateTime)oldEntity["date_from"];
            int startDay = ((int)oldEntity["start"] -1) * 7;

            IRotation rot = FindRotation(rotationId);
            IPerson pers = FindPerson(empId);

            IPersonRotation ret =  new PersonRotation(pers,rot,new DateOnly(startDate), startDay );
            return ret;
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

        private IRotation FindRotation(int oldIdOnRotation)
        {
            IRotation ret = null;
            foreach (ObjectPair<DataRow, IRotation> pair in MappedObjectPair.Rotations)
            {
                if ((int)pair.Obj1["rotation_id"] == oldIdOnRotation)
                    ret = pair.Obj2;
            }
            return ret;
        }
    }
}
