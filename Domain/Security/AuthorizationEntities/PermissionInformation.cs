using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    public class PermissionInformation : IPermissionInformation
    {
        private IList<IApplicationRole> personInApplicationRole = new List<IApplicationRole>();
        private string defaultTimeZone;
        private TimeZoneInfo _defaultTimeZoneCache;
        private int? culture;
        private int? uiCulture;
        private IPerson _belongsTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionInformation"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        public PermissionInformation(IPerson parent)
            : this()
        {
            _belongsTo = parent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionInformation"/> class.
        /// Used by reflection by nhib
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-28
        /// </remarks>
        protected PermissionInformation()
        {
        }

        public IList<IApplicationRole> ApplicationRoleCollection
        {
            get
            {
                List<IApplicationRole> newPersonInApplicationRole = new List<IApplicationRole>();

                foreach (IApplicationRole applicationRole in personInApplicationRole)
                {
                    // here the person have all the application role to all business units whereas we only need
                    // those ApplicationRoles that are connected to the choosen business unit. We alos take into 
                    // consideration that the business unit can be null to those application roles that are valid
                    // to all business units like super roles.
                    var identity = Thread.CurrentPrincipal.Identity as ITeleoptiIdentity;
                    var businessUnit = identity != null ? identity.BusinessUnit : null;
                    if (!StateHolderReader.IsInitialized
                        && businessUnit != null
                        && (applicationRole.BusinessUnit != null && !applicationRole.BusinessUnit.Equals(businessUnit)))
                        continue;
                    newPersonInApplicationRole.Add(applicationRole);
                }
                return new ReadOnlyCollection<IApplicationRole>(newPersonInApplicationRole);
            }

        }

        public IPerson BelongsTo
        {
            get { return _belongsTo; }
            set { _belongsTo = value; }
        }

        public bool RightToLeftDisplay
        {
            get
            {
                return (UICulture().TextInfo.IsRightToLeft);
            }
        }

        public bool HasAccessToAllBusinessUnits()
        {
            foreach (IApplicationRole role in personInApplicationRole)
            {
                if (role.AvailableData != null)
                {
                    if (role.AvailableData.AvailableDataRange == AvailableDataRangeOption.Everyone)
                        return true;
                }
            }
            return false;
        }

        public IList<IBusinessUnit> BusinessUnitAccessCollection()
        {
            ICollection<IBusinessUnit> retColl = new HashSet<IBusinessUnit>();
            foreach (IApplicationRole role in personInApplicationRole)
            {
                if (!((IDeleteTag)role.BusinessUnit).IsDeleted)
                    retColl.Add(role.BusinessUnit);
            }
            return new List<IBusinessUnit>(retColl);
        }

        public void SetDefaultTimeZone(TimeZoneInfo value)
        {
            InParameter.NotNull("DefaultTimeZone", value);
            _defaultTimeZoneCache = value;
            defaultTimeZone = value.Id;
        }

        public TimeZoneInfo DefaultTimeZone()
        {
            if (_defaultTimeZoneCache == null || _defaultTimeZoneCache.Id != defaultTimeZone)
            {
                _defaultTimeZoneCache = string.IsNullOrEmpty(defaultTimeZone)
                                            ? TimeZoneInfo.Local
                                            : TimeZoneInfo.FindSystemTimeZoneById(defaultTimeZone);
            }
            return _defaultTimeZoneCache;
        }

        public void SetCulture(CultureInfo value)
        {
            if (value != null) culture = value.LCID;
            else culture = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "argumentException")]
        public CultureInfo Culture()
        {
            if (!culture.HasValue) return Thread.CurrentThread.CurrentCulture.FixPersianCulture();

            try
            {
				return CultureInfo.GetCultureInfo(culture.Value).FixPersianCulture();
            }
            catch (ArgumentException)
            {
				return Thread.CurrentThread.CurrentCulture.FixPersianCulture();
            }

        }

        public int? CultureLCID()
        {
            return culture;
        }

        public void SetUICulture(CultureInfo value)
        {
            if (value != null) uiCulture = value.LCID;
            else uiCulture = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "argumentException")]
        public CultureInfo UICulture()
        {
			if (!uiCulture.HasValue) return Thread.CurrentThread.CurrentUICulture.FixPersianCulture();

            try
            {
				return CultureInfo.GetCultureInfo(uiCulture.Value).FixPersianCulture();
            }
            catch (ArgumentException)
            {
				return Thread.CurrentThread.CurrentUICulture.FixPersianCulture();
            }
        }

        public int? UICultureLCID()
        {
            return uiCulture;
        }

        public void AddApplicationRole(IApplicationRole role)
        {
            InParameter.NotNull("role", role);
            if (!personInApplicationRole.Contains(role))
                personInApplicationRole.Add(role);
        }

        public void RemoveApplicationRole(IApplicationRole role)
        {
            personInApplicationRole.Remove(role);
        }

        #region ICloneableEntity<IPermissionInformation> Members

        public virtual IPermissionInformation NoneEntityClone()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-10-02
        /// </remarks>
        public virtual IPermissionInformation EntityClone()
        {
            PermissionInformation retObj = (PermissionInformation)MemberwiseClone();
            CopyRoleInto(retObj);
            return retObj;
        }

        protected void CopyRoleInto(IPermissionInformation permissionInfo)
        {
            ((PermissionInformation)permissionInfo).CreateApplicationRoleCollection(new List<IApplicationRole>());
            foreach (IApplicationRole role in personInApplicationRole)
            {
                permissionInfo.AddApplicationRole(role);
            }
        }

        private void CreateApplicationRoleCollection(IList<IApplicationRole> applicationRoles)
        {
            personInApplicationRole = applicationRoles;
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-10-02
        /// </remarks>
        public virtual object Clone()
        {
            return EntityClone();
        }

        #endregion
    }
}
