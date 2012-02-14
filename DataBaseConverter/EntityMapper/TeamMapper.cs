using System;
using Domain;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for converting 6x Teams
    /// </summary>
    public class TeamMapper : Mapper<ITeam, UnitSub>
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public TeamMapper(MappedObjectPair mappedObjectPair) : base(mappedObjectPair, null)
        {}

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public override ITeam Map(UnitSub oldEntity)
        {
            Team newTeam = null;
            string oldName = oldEntity.Name;
            
            while (newTeam == null)
            {
                try
                {
                    ISite parent;
                    newTeam = new Team();
                    newTeam.Description = new Description(oldName);

                    if(oldEntity.Deleted)
                    ((IDeleteTag)newTeam).SetDeleted();
                    
                    //rk: smutslösning pga brist på obj-ref
                    
                    foreach (ObjectPair<Unit, ISite> pair in MappedObjectPair.Site)
                    {
                        if (pair.Obj1.Id == oldEntity.ParentId)
                        {
                            parent = pair.Obj2;
                            parent.AddTeam(newTeam);
                            break;
                        }
                    }
                }
                catch (ArgumentException)
                {
                    oldName = oldName.Remove(oldName.Length - 1);
                    newTeam = null;
                }
            }
            return newTeam;
        }
    }
}