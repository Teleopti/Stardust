using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Mapper
    /// </summary>
    /// <typeparam name="TNew">The type of the new.</typeparam>
    /// <typeparam name="TOld">The type of the old.</typeparam>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/23/2007
    /// </remarks>
    public abstract class Mapper<TNew, TOld>
    {
        private readonly MappedObjectPair _mappedObjectPair;
        private readonly ICccTimeZoneInfo _timeZone;


        /// <summary>
        /// Initializes a new instance of the <see cref="Mapper{TNew,TOld}"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        protected Mapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone)
        {
            InParameter.NotNull("mappedObjectPair", mappedObjectPair);
            _mappedObjectPair = mappedObjectPair;
            _timeZone = timeZone;
        }

        /// <summary>
        /// Gets the mapped object pair.
        /// </summary>
        /// <value>The mapped object pair.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public MappedObjectPair MappedObjectPair
        {
            get { return _mappedObjectPair; }
        }

        /// <summary>
        /// Gets the time zone info.
        /// </summary>
        /// <value>The time zone info.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public ICccTimeZoneInfo TimeZone
        {
            get { return _timeZone; }
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
        public abstract TNew Map(TOld oldEntity);
    }
}
