using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
	public class PersonGeneralModel : GridViewModelBase<IPerson>, IOptionalColumnView
	{
		private IList<IOptionalColumn> _optionalColumns;

		public static IWorkflowControlSet NullWorkflowControlSet = new WorkflowControlSet(string.Empty);
		private readonly IUserDetail _userDetail;
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly IPersonAccountUpdater _personAccountUpdater;
		private bool _isValid = true;
		private RoleCollectionParser _roleCollectionParser;
		private bool _logonDataCanBeChanged;
		private bool _rightsHaveBeenChecked;
		private TenantAuthenticationData _tenantData;

		public PersonGeneralModel()
		{
			_optionalColumns = new List<IOptionalColumn>();
		}

		public PersonGeneralModel(IPerson person, IUserDetail userDetail, IPrincipalAuthorization principalAuthorization,
			IPersonAccountUpdater personAccountUpdater)
			: this()
		{
			ContainedEntity = person;
			_userDetail = userDetail;
			_principalAuthorization = principalAuthorization;
			_personAccountUpdater = personAccountUpdater;
			_tenantData = new TenantAuthenticationData();
			if (ContainedEntity.AuthenticationInfo != null)
				_tenantData.Identity = ContainedEntity.AuthenticationInfo.Identity;
			if (ContainedEntity.ApplicationAuthenticationInfo != null)
			{
				_tenantData.UserName = ContainedEntity.ApplicationAuthenticationInfo.ApplicationLogOnName;
				_tenantData.Password = ContainedEntity.ApplicationAuthenticationInfo.Password;
			}
			_tenantData.TerminalDate = ContainedEntity.TerminalDate;
		}

		public string FirstName
		{
			get
			{
				return ContainedEntity.Name.FirstName;
			}
			set
			{
				var name = new Name(value, LastName);
				ContainedEntity.Name = name;
			}
		}

		public string LastName
		{
			get
			{
				return ContainedEntity.Name.LastName;
			}
			set
			{
				var name = new Name(FirstName, value);
				ContainedEntity.Name = name;
			}
		}

		public string FullName
		{
			get
			{
				return ContainedEntity.Name.ToString();
			}
		}

		public string Email
		{
			get { return ContainedEntity.Email; }
			set { ContainedEntity.Email = value; }
		}

		public string EmployeeNumber
		{
			get { return ContainedEntity.EmploymentNumber; }
			set { ContainedEntity.EmploymentNumber = value; }
		}

		public string Note
		{
			get { return ContainedEntity.Note; }
			set { ContainedEntity.Note = value; }
		}

		public int Language
		{
			get { return ContainedEntity.PermissionInformation.Culture().LCID; }
			set
			{
				if (value != 0) ContainedEntity.PermissionInformation.SetCulture(new System.Globalization.CultureInfo(value));
				else if (value == 0) ContainedEntity.PermissionInformation.SetCulture(null);
			}
		}

		public int? LanguageLCID
		{
			get { return ContainedEntity.PermissionInformation.CultureLCID(); }
		}

		public Culture LanguageInfo
		{
			get
			{
				if (ContainedEntity.PermissionInformation.UICultureLCID() == null)
					return new Culture(0, UserTexts.Resources.ChangeYourCultureSettings);

				var culture = ContainedEntity.PermissionInformation.UICulture();
				return new Culture(culture.LCID, culture.DisplayName);
			}
			set
			{
				if (value.Id != 0) ContainedEntity.PermissionInformation.SetUICulture(System.Globalization.CultureInfo.GetCultureInfo(value.Id));
				else if (value.Id == 0) ContainedEntity.PermissionInformation.SetUICulture(null);
			}
		}

		public int Culture
		{
			get { return ContainedEntity.PermissionInformation.UICulture().LCID; }
			set
			{
				if (value != 0) ContainedEntity.PermissionInformation.SetUICulture(System.Globalization.CultureInfo.GetCultureInfo(value));
				else if (value == 0) ContainedEntity.PermissionInformation.SetUICulture(null);
			}
		}

		public int? CultureLCID
		{
			get { return ContainedEntity.PermissionInformation.UICultureLCID(); }
		}

		public Culture CultureInfo
		{
			get
			{
				if (ContainedEntity.PermissionInformation.CultureLCID() == null)
					return new Culture(0, UserTexts.Resources.ChangeYourCultureSettings);

				var culture = ContainedEntity.PermissionInformation.Culture();
				return new Culture(culture.LCID, culture.Name);
			}
			set
			{
				if (value.Id != 0) ContainedEntity.PermissionInformation.SetCulture(System.Globalization.CultureInfo.GetCultureInfo(value.Id));
				else if (value.Id == 0) ContainedEntity.PermissionInformation.SetCulture(null);
			}
		}

		public string TimeZone
		{
			get { return ContainedEntity.PermissionInformation.DefaultTimeZone().Id; }
			set
			{
				InParameter.NotStringEmptyOrNull(value, "TimeZone");
				ContainedEntity.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById(value));
			}
		}

		public TimeZoneInfo TimeZoneInformation
		{
			get { return (TimeZoneInfo)ContainedEntity.PermissionInformation.DefaultTimeZone(); }
			set
			{
				InParameter.NotNull("TimeZoneInfo", value);
				ContainedEntity.PermissionInformation.SetDefaultTimeZone(value);
			}
		}

		public string LogOnName
		{
			get
			{
				if (ContainedEntity.AuthenticationInfo == null) return "";
				return ContainedEntity.AuthenticationInfo.Identity;
			}
			set
			{
				if (!logonDataCanBeChanged())
					return;
				if (ContainedEntity.AuthenticationInfo == null)
					ContainedEntity.AuthenticationInfo = new AuthenticationInfo { Identity = value };
				else
					ContainedEntity.AuthenticationInfo.Identity = value;
				checkIdentity();
				_tenantData.Identity = value;
				_tenantData.Changed = true;
			}
		}

		private void checkIdentity()
		{
			if (ContainedEntity.AuthenticationInfo != null)
			{
				if (string.IsNullOrEmpty(ContainedEntity.AuthenticationInfo.Identity))
					ContainedEntity.AuthenticationInfo = null;
			}
		}
		public string ApplicationLogOnName
		{
			get
			{
				if (ContainedEntity.ApplicationAuthenticationInfo == null) return "";
				return ContainedEntity.ApplicationAuthenticationInfo.ApplicationLogOnName;
			}
			set
			{
				if (!logonDataCanBeChanged())
					return;

				if (string.IsNullOrEmpty(value))
				{
					ContainedEntity.ApplicationAuthenticationInfo = null;
					_isValid = true;
					return;
				}
				if (ContainedEntity.ApplicationAuthenticationInfo == null)
					ContainedEntity.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo { ApplicationLogOnName = value };
				else
					ContainedEntity.ApplicationAuthenticationInfo.ApplicationLogOnName = value;
				var policyService = StateHolder.Instance.StateReader.ApplicationScopeData.LoadPasswordPolicyService;
				_isValid = ContainedEntity.ChangePassword(ContainedEntity.ApplicationAuthenticationInfo.Password, policyService, _userDetail);
				if (!_isValid) //Is there a better solution for this?
					writeMessage();
				_tenantData.UserName = value;
				_tenantData.Changed = true;
			}
		}

		public string Password
		{
			get
			{
				if (ContainedEntity.ApplicationAuthenticationInfo == null) return "";
				return ContainedEntity.ApplicationAuthenticationInfo.Password;
			}
			set
			{
				if (!logonDataCanBeChanged())
					return;
				if (ContainedEntity.ApplicationAuthenticationInfo == null || string.IsNullOrEmpty(ContainedEntity.ApplicationAuthenticationInfo.ApplicationLogOnName))
				{
					_isValid = true;
					return;
				}
				var policyService = StateHolder.Instance.StateReader.ApplicationScopeData.LoadPasswordPolicyService;
				_isValid = ContainedEntity.ChangePassword(value, policyService, _userDetail);
				if (!_isValid) //Is there a better solution for this?
					writeMessage();
				_tenantData.Password = value;
				_tenantData.Changed = true;
			}
		}

		public bool IsAgent
		{
			get { return ContainedEntity.IsAgent(DateOnly.Today); }
		}

		public bool IsUser
		{
			get { return ContainedEntity.PermissionInformation != null; }
		}

		public DateOnly? TerminalDate
		{
			get { return ContainedEntity.TerminalDate; }
			set
			{
				var terminateDate = value;

				if (terminateDate != null)
					ContainedEntity.TerminatePerson(terminateDate.Value, _personAccountUpdater);
				else
				{
					ContainedEntity.ActivatePerson(_personAccountUpdater);
				}
				_tenantData.TerminalDate = value;
				_tenantData.Changed = true;
			}

		}

		public string Roles
		{
			get
			{
				return _roleCollectionParser.GetRoleCollectionDisplayText(ContainedEntity.PermissionInformation.ApplicationRoleCollection);
			}
			set
			{
				var currentRoles = new List<IApplicationRole>(ContainedEntity.PermissionInformation.ApplicationRoleCollection);
				var newRoles = _roleCollectionParser.ParseRoleCollection(value);

				foreach (var applicationRole in currentRoles)
				{
					ContainedEntity.PermissionInformation.RemoveApplicationRole(applicationRole);
				}

				foreach (var applicationRole in newRoles)
				{
					ContainedEntity.PermissionInformation.AddApplicationRole(applicationRole);
				}
			}

		}

		public void SetAvailableRoles(IList<IApplicationRole> value)
		{
			_roleCollectionParser = new RoleCollectionParser(value, new RoleDisplay());
		}

		public IList<IOptionalColumn> OptionalColumns
		{
			get
			{
				return _optionalColumns;
			}
		}

		public IWorkflowControlSet WorkflowControlSet
		{
			get { return ContainedEntity.WorkflowControlSet ?? NullWorkflowControlSet; }
			set
			{
				ContainedEntity.WorkflowControlSet = (value == NullWorkflowControlSet) ? null : value;
			}
		}

		public DayOfWeekDisplay FirstDayOfWeek
		{
			get
			{
				return DayOfWeekDisplay.ListOfDayOfWeek.SingleOrDefault(day => day.DayOfWeek == ContainedEntity.FirstDayOfWeek);
				//return DayOfWeekDisplay.ListOfDayOfWeek[(int)ContainedEntity.FirstDayOfWeek];
			}
			set
			{
				if (value != null)
					ContainedEntity.FirstDayOfWeek = value.DayOfWeek;
			}
		}

		public bool IsValid
		{
			get { return _isValid; }
		}

		public bool CanGray
		{
			get { return false; }
		}

		public void SetOptionalColumns(IList<IOptionalColumn> columns)
		{
			_optionalColumns = columns;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		private static void writeMessage()
		{
			MessageBoxAdv.Show(UserTexts.Resources.PasswordPolicyWarning,
				 UserTexts.Resources.ErrorMessage,
				 MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
		}

		public bool logonDataCanBeChanged()
		{
			if (!_rightsHaveBeenChecked)
			{
				_logonDataCanBeChanged =
					 _principalAuthorization.IsPermitted(
						  DefinedRaptorApplicationFunctionPaths.ModifyPersonNameAndPassword, DateOnly.Today,
						  ContainedEntity);
				_rightsHaveBeenChecked = true;
			}
			return _logonDataCanBeChanged;
		}

		public void ResetLogonDataCheck()
		{
			_rightsHaveBeenChecked = false;
		}
	}
}
