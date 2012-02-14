using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    ///<summary>
    ///Tool for converting scenarios from old version to new version
    ///</summary>
    public class ScenarioMapper : Mapper<IScenario, global::Domain.Scenario>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioMapper"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public ScenarioMapper(MappedObjectPair mappedObjectPair) : base(mappedObjectPair, null)
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
        public override IScenario Map(global::Domain.Scenario oldEntity)
        {
            IScenario scenario = new Scenario(oldEntity.Name);
            scenario.DefaultScenario = oldEntity.DefaultScenario;
            scenario.AuditTrail = oldEntity.AuditTrail;
            scenario.EnableReporting = oldEntity.DefaultScenario;

            return scenario;
        }
    }
}