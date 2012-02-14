using System.Linq;
using Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.DatabaseConverter
{
    /// <summary>
    /// Determines if the shift contains overtime
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-11-25
    /// </remarks>
    public class ShiftContainsOvertime : Specification<ShiftBase>
    {
        private readonly LayerContainsOvertime _layerContainsOvertimeSpecification;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftContainsOvertime"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-25
        /// </remarks>
        public ShiftContainsOvertime(MappedObjectPair mappedObjectPair)
        {
            _layerContainsOvertimeSpecification = new LayerContainsOvertime(mappedObjectPair);
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if [is satisfied by] [the specified obj]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-25
        /// </remarks>
        public override bool IsSatisfiedBy(ShiftBase obj)
        {
            return obj.LayerCollection.Any(_layerContainsOvertimeSpecification.IsSatisfiedBy);
        }
    }

    /// <summary>
    /// Determines if the layer contains overtime
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-11-25
    /// </remarks>
    public class LayerContainsOvertime : Specification<ActivityLayer>
    {
        private readonly MappedObjectPair _mappedObjectPair;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerContainsOvertime"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-25
        /// </remarks>
        public LayerContainsOvertime(MappedObjectPair mappedObjectPair)
        {
            _mappedObjectPair = mappedObjectPair;
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if [is satisfied by] [the specified obj]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-11-25
        /// </remarks>
        public override bool IsSatisfiedBy(ActivityLayer obj)
        {
            return _mappedObjectPair.OvertimeActivity.Obj1Collection().Contains(obj.LayerActivity);
        }
    }
}
