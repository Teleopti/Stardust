using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Login source mapper
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-10-31
    /// </remarks>
    public class ExternalLogOnMapper : Mapper<IExternalLogOn, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalLogOnMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-10-31
        /// </remarks>
        public ExternalLogOnMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone)
            : base(mappedObjectPair, timeZone)
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
        public override IExternalLogOn Map(int oldEntity)
        {
            string convStr = oldEntity.ToString(CultureInfo.CurrentCulture);
            return new ExternalLogOn(-1, oldEntity, convStr, convStr, true);
        }
    }
}
