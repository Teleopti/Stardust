using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for converting 6x Units
    /// </summary>
    public class SiteMapper : Mapper<ISite, global::Domain.Unit>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public SiteMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
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
        public override ISite Map(global::Domain.Unit oldEntity)
        {
            Site newSite = null;
            if (oldEntity.AllUnits)
                return newSite;

            string oldName = oldEntity.Name;
            if (string.IsNullOrEmpty(oldName))
                oldName = MissingData.Name;
            while (newSite == null)
            {
                try
                {
                    newSite = new Site(oldName);
                    if (oldEntity.Deleted)
                        newSite.SetDeleted();
                }
                catch (ArgumentException)
                {
                    oldName = oldName.Remove(oldName.Length - 1);
                    newSite = null;
                }
            }

            return newSite;
        }
    }
}