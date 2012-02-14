﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    public class PermissionInformation : IPermissionInformation
    {
        private IList<IApplicationRole> personInApplicationRole = new List<IApplicationRole>();
        private IWindowsAuthenticationInfo _windowsAuthenticationInfo;
        private IApplicationAuthenticationInfo _applicationAuthenticationInfo;
     
        private string defaultTimeZone;
        private ICccTimeZoneInfo _defaultTimeZoneCache;
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
        public PermissionInformation(IPerson parent) : this()
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
                    var identity = Thread.CurrentPrincipal.Identity as TeleoptiIdentity;
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

        public IWindowsAuthenticationInfo WindowsAuthenticationInfo
        {
            get
            {
                if (_windowsAuthenticationInfo == null)
                    _windowsAuthenticationInfo = new WindowsAuthenticationInfo();
                return _windowsAuthenticationInfo;
            }
            set
            {
                InParameter.NotNull("WindowsAuthenticationInfo", value);
                _windowsAuthenticationInfo = value;
                _windowsAuthenticationInfo.SetParent(BelongsTo);
            }
        }

        public IApplicationAuthenticationInfo ApplicationAuthenticationInfo
        {
            get
            {
                if (_applicationAuthenticationInfo == null)
                    _applicationAuthenticationInfo = new ApplicationAuthenticationInfo();
                return _applicationAuthenticationInfo;
            }
            set
            {
                InParameter.NotNull("ApplicationAuthenticationInfo", value);
                _applicationAuthenticationInfo = value;
                _applicationAuthenticationInfo.SetParent(BelongsTo);
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

        public void SetDefaultTimeZone(ICccTimeZoneInfo value)
        {
            InParameter.NotNull("DefaultTimeZone", value);
            _defaultTimeZoneCache = value;
            defaultTimeZone = value.Id;
        }

        public ICccTimeZoneInfo DefaultTimeZone()
        {
            if (_defaultTimeZoneCache==null || _defaultTimeZoneCache.Id!=defaultTimeZone)
            {
                _defaultTimeZoneCache = string.IsNullOrEmpty(defaultTimeZone)
                                            ? new CccTimeZoneInfo(TimeZoneInfo.Local)
                                            : new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById(defaultTimeZone));
            }
            return _defaultTimeZoneCache;
        }

        public void SetCulture(CultureInfo value)
        {
            if (value != null) culture = value.LCID;
            else culture = null;
        }

        public CultureInfo Culture()
        {
            if (!culture.HasValue) return Thread.CurrentThread.CurrentCulture;
            return CultureInfo.GetCultureInfo(culture.Value);
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

        public CultureInfo UICulture()
        {
            if (!uiCulture.HasValue) return Thread.CurrentThread.CurrentUICulture;
            return CultureInfo.GetCultureInfo(uiCulture.Value);
        }

        public int? UICultureLCID()
        {
            return uiCulture;
        }

        public void AddApplicationRole(IApplicationRole role)
        {
            InParameter.NotNull("role", role);
            if(!personInApplicationRole.Contains(role))
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
