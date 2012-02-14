using System;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for creating default definition sets for multiplicators
    /// </summary>
    public class MultiplicatorMapper : Mapper<IMultiplicatorDefinitionSet, IMultiplicator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiplicatorMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public MultiplicatorMapper(MappedObjectPair mappedObjectPair)
            : base(mappedObjectPair, null)
        {
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        public override IMultiplicatorDefinitionSet Map(IMultiplicator oldEntity)
        {
            IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet(oldEntity.Description.Name,
                                                                                       MultiplicatorType.Overtime);

            foreach (DayOfWeek value in Enum.GetValues(typeof(DayOfWeek)))
            {
                definitionSet.AddDefinition(new DayOfWeekMultiplicatorDefinition(oldEntity, value, new TimePeriod(0, 0, 24, 0)));
            }
            if (((IDeleteTag)oldEntity).IsDeleted)
            {
                ((IDeleteTag)definitionSet).SetDeleted();
            }

            return definitionSet;
        }
    }
}