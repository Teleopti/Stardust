using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds info about permission stuff of a person
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-28
    /// </remarks>
    public interface IPermissionInformation : ICloneableEntity<IPermissionInformation>
    {
        /// <summary>
        /// Gets the application role collection.
        /// </summary>
        /// <value>The application role collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        IList<IApplicationRole> ApplicationRoleCollection { get; }

        /// <summary>
        /// Gets the belonging person.
        /// </summary>
        /// <value>The person.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        IPerson BelongsTo { get; }

        /// <summary>
        /// Gets a value indicating whether the person culture is RightToLeft.
        /// </summary>
        /// <value><c>true</c> if RightToLeft; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 11/30/2007
        /// </remarks>
        bool RightToLeftDisplay { get; }

        /// <summary>
        /// Determines whether the user has access to all business units.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if the user has access to all business units; otherwise, <c>false</c>.
        /// </returns>
        bool HasAccessToAllBusinessUnits();

        /// <summary>
        /// Returns available Businesses unit access collection.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-29
        /// </remarks>
        IList<IBusinessUnit> BusinessUnitAccessCollection();

        /// <summary>
        /// Sets the culture.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-30
        /// </remarks>
        void SetCulture(CultureInfo value);

        /// <summary>
        /// Gets the persons selected culture.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Applies to CurrentCulture of thread.
        /// 
        /// Created by: robink
        /// Created date: 2007-11-30
        /// </remarks>
        CultureInfo Culture();

        /// <summary>
        /// Cultures the LCID.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2/20/2008
        /// </remarks>
        int? CultureLCID();

        /// <summary>
        /// Sets the UI culture.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-30
        /// </remarks>
        void SetUICulture(CultureInfo value);

        /// <summary>
        /// Gets the persons's selected UI culture.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Applies to CurrentUICulture of thread.
        /// 
        /// Created by: robink
        /// Created date: 2007-11-30
        /// </remarks>
        CultureInfo UICulture();

        /// <summary>
        /// UIs the culture LCID.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2/20/2008
        /// </remarks>
        int? UICultureLCID();

        /// <summary>
        /// Adds an application role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        void AddApplicationRole(IApplicationRole role);

        /// <summary>
        /// Removes the application role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-29
        /// </remarks>
        void RemoveApplicationRole(IApplicationRole role);

        /// <summary>
        /// Gets the default time zone.
        /// </summary>
        /// <value>The default time zone.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        void SetDefaultTimeZone(ICccTimeZoneInfo value);

        /// <summary>
        /// Gets the default time zone.
        /// </summary>
        /// <value>The default time zone.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        ICccTimeZoneInfo DefaultTimeZone();
    }
}
