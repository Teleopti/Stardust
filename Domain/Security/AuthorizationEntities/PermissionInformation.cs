using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	public class PermissionInformation : IPermissionInformation
	{
		private IList<IApplicationRole> personInApplicationRole = new List<IApplicationRole>();
		private string defaultTimeZone;
		private Lazy<TimeZoneInfo> _defaultTimeZoneCache;
		private int? culture;
		private int? uiCulture;
		private IPerson _belongsTo;

		public PermissionInformation(IPerson parent)
			: this()
		{
			_belongsTo = parent;
		}

		protected PermissionInformation()
		{
			_defaultTimeZoneCache = new Lazy<TimeZoneInfo>(() => string.IsNullOrEmpty(defaultTimeZone)
				? TimeZoneInfo.Local
				: TimeZoneInfo.FindSystemTimeZoneById(defaultTimeZone));
		}

		public IList<IApplicationRole> ApplicationRoleCollection
		{
			get
			{
				var newPersonInApplicationRole = new List<IApplicationRole>();
				foreach (IApplicationRole applicationRole in personInApplicationRole)
				{
					// here the person have all the application role to all business units whereas we only need
					// those ApplicationRoles that are connected to the choosen business unit. We alos take into 
					// consideration that the business unit can be null to those application roles that are valid
					// to all business units like super roles.

					var identity = Thread.CurrentPrincipal.Identity as ITeleoptiIdentity;
                    var businessUnit = identity?.BusinessUnit;
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
			InParameter.NotNull(nameof(DefaultTimeZone), value);
			_defaultTimeZoneCache = new Lazy<TimeZoneInfo>(()=>value);
			defaultTimeZone = value.Id;
		}

		public string DefaultTimeZoneString()
		{
			return defaultTimeZone;
		}

		public TimeZoneInfo DefaultTimeZone()
		{
			return _defaultTimeZoneCache.Value;
		}

		public void SetCulture(CultureInfo value)
		{
			if (value != null) culture = value.LCID;
			else culture = null;
		}
		
		public CultureInfo Culture()
		{

			if (TeleoptiPrincipalForLegacy.CurrentPrincipal == null || !TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.ForceUseGregorianCalendar)
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

			if (!culture.HasValue) return Thread.CurrentThread.CurrentCulture;

			try
			{
				return CultureInfo.GetCultureInfo(culture.Value);
			}
			catch (ArgumentException)
			{
				return Thread.CurrentThread.CurrentCulture;
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

			if (TeleoptiPrincipalForLegacy.CurrentPrincipal == null || !TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.ForceUseGregorianCalendar)
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

			if (!uiCulture.HasValue) return Thread.CurrentThread.CurrentUICulture;

			try
			{
				return CultureInfo.GetCultureInfo(uiCulture.Value);
			}
			catch (ArgumentException)
			{
				return Thread.CurrentThread.CurrentUICulture;
			}

		}

		public int? UICultureLCID()
		{
			return uiCulture;
		}

		public void AddApplicationRole(IApplicationRole role)
		{
			InParameter.NotNull(nameof(role), role);
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
