using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
	public class PersonGeneralModel : GridViewModelBase<IPerson>, IOptionalColumnView
	{
		private IList<IOptionalColumn> _optionalColumns;

		public static IWorkflowControlSet NullWorkflowControlSet = new WorkflowControlSet(string.Empty);
		private readonly IAuthorization _authorization;
		private readonly IPersonAccountUpdater _personAccountUpdater;
		private readonly IPasswordPolicy _passwordPolicy;
		private bool _isValid = true;
		private RoleCollectionParser _roleCollectionParser;
		private bool _logonDataCanBeChanged;
		private bool _rightsHaveBeenChecked;
		private readonly TenantAuthenticationData _tenantData;

		public PersonGeneralModel()
		{
			_optionalColumns = new List<IOptionalColumn>();
		}

		public PersonGeneralModel(IPerson person, IAuthorization authorization,
			IPersonAccountUpdater personAccountUpdater, LogonInfoModel logonInfoModel, IPasswordPolicy passwordPolicy)
			: this()
		{
			ContainedEntity = person;
			_authorization = authorization;
			_personAccountUpdater = personAccountUpdater;
			_passwordPolicy = passwordPolicy;
			_tenantData = new TenantAuthenticationData { PersonId = ContainedEntity.Id.GetValueOrDefault() };

			if (logonInfoModel != null)
			{
				if (!string.IsNullOrEmpty(logonInfoModel.LogonName))
					_tenantData.ApplicationLogonName = logonInfoModel.LogonName;
				if (!string.IsNullOrEmpty(logonInfoModel.Identity))
					_tenantData.Identity = logonInfoModel.Identity;

			}
		}

		public TenantAuthenticationData TenantData => _tenantData;

		public string FirstName
		{
			get
			{
				return ContainedEntity.Name.FirstName;
			}
			set
			{
				var name = new Name(value, LastName);
				ContainedEntity.SetName(name);
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
				ContainedEntity.SetName(name);
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
			set { ContainedEntity.SetEmploymentNumber(value); }
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

		public int? LanguageLCID => ContainedEntity.PermissionInformation.CultureLCID();

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

		public int? CultureLCID => ContainedEntity.PermissionInformation.UICultureLCID();

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
			get { return ContainedEntity.PermissionInformation.DefaultTimeZone(); }
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
				return _tenantData.Identity;
			}
			set
			{
				if (!logonDataCanBeChanged())
					return;
				value = value?.Trim();
				_tenantData.Identity = value;
				if (_tenantData.Identity == "")
					_tenantData.Identity = null;
				_tenantData.Changed = true;
			}
		}

		public string ApplicationLogOnName
		{
			get
			{
				return _tenantData.ApplicationLogonName;
			}
			set
			{
				if (!logonDataCanBeChanged())
					return;

				value = value?.Trim();
				if (string.IsNullOrEmpty(value))
				{
					_isValid = true;
					_tenantData.ApplicationLogonName = null;
					_tenantData.Password = null;
					_tenantData.Changed = true;
					return;
				}
				_tenantData.ApplicationLogonName = value;
				_tenantData.Changed = true;
				_isValid = _passwordPolicy.CheckPasswordStrength(Password);
			}
		}

		public string Password
		{
			get
			{
				return _tenantData.Password ?? string.Empty;
			}
			set
			{
				if (!logonDataCanBeChanged())
					return;
				_tenantData.Password = value;
				_tenantData.Changed = true;

				if (string.IsNullOrEmpty(_tenantData.ApplicationLogonName))
				{
					_isValid = true;
					return;
				}
				_isValid = _passwordPolicy.CheckPasswordStrength(value);
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

				if (terminateDate.HasValue)
				{
					IsTerminalDateChanged = true;
					ContainedEntity.TerminatePerson(terminateDate.Value, _personAccountUpdater);
				}
				else
				{
					ContainedEntity.ActivatePerson(_personAccountUpdater);
				}
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

		public void SetIsTerminalDateChanged(bool value)
		{
			IsTerminalDateChanged = value;
		}

		public IList<IOptionalColumn> OptionalColumns => _optionalColumns;

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

		public bool IsValid => _isValid;

		public bool CanGray => false;
		public bool IsTerminalDateChanged { get; set; }

		public void SetOptionalColumns(IList<IOptionalColumn> columns)
		{
			_optionalColumns = columns;
		}

		public bool logonDataCanBeChanged()
		{
			if (!_rightsHaveBeenChecked)
			{
				_logonDataCanBeChanged =
					 _authorization.IsPermitted(
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
