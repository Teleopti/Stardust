using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using NHibernate;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
	public class DefaultDataCreator
	{
		private readonly ISessionFactory _sessionFactory;
		private readonly string _businessUnitName;
		private readonly CultureInfo _cultureInfo;
		private readonly TimeZoneInfo _timeZone;
		private readonly string _newUserName;
		private readonly string _newUserPassword;
		private IPerson _person;
		private PersonCreator _personCreator;
		private BusinessUnitCreator _businessUnitCreator;
		private ApplicationRoleCreator _applicationRoleCreator;
		private AvailableDataCreator _availableDataCreator;
		private SkillTypeCreator _skillTypeCreator;
		private KeyPerformanceIndicatorCreator _keyPerformanceIndicatorCreator;

		public DefaultDataCreator(string businessUnitName, CultureInfo cultureInfo, TimeZoneInfo timeZone, string newUserName, string newUserPassword, ISessionFactory sessionFactory)
		{
			_businessUnitName = businessUnitName;
			_cultureInfo = cultureInfo;
			_timeZone = timeZone;
			_newUserName = newUserName;
			_newUserPassword = newUserPassword;
			_sessionFactory = sessionFactory;
		}

		public DefaultAggregateRoot Create()
		{
			//OBS !!! Ta höjd för att ett nytt businessunit ska skapas
			DefaultAggregateRoot defaultAggregateRoot = new DefaultAggregateRoot();

			//Create DbConverter Person
			createDefaultPersons();

			//Create (7) default KeyPerformanceIndicators
			createDefaultKeyPerformanceIndicators();

			//Create Businessunit
			IBusinessUnit businessUnit = createDefaultBusinessUnit();
			defaultAggregateRoot.BusinessUnit = businessUnit;

			//Create Roles (Role and available data)
			_applicationRoleCreator = new ApplicationRoleCreator(_person, businessUnit, _sessionFactory);
			_availableDataCreator = new AvailableDataCreator(_person, _sessionFactory);

			//BuiltInAdministrator
			IApplicationRole builtInAdministratorRole = _applicationRoleCreator.Create(ShippedApplicationRoleNames.AdministratorRole, "xxBuiltInAdministratorRole", false);
			defaultAggregateRoot.AdministratorRole = builtInAdministratorRole;

			//BuiltinBusinessUnitAdministrator
			IApplicationRole businessUnitAdministratorRole = _applicationRoleCreator.Create(ShippedApplicationRoleNames.BusinessUnitAdministratorRole, "xxBuiltInBusinessUnitAdministratorRole", false);
			defaultAggregateRoot.BusinessUnitAdministratorRole = businessUnitAdministratorRole;

			//SiteManagerRole
			IApplicationRole siteManagerRole = _applicationRoleCreator.Create(ShippedApplicationRoleNames.SiteManagerRole, "xxBuiltInSiteManagerRole", false);
			defaultAggregateRoot.SiteManagerRole = siteManagerRole;

			//TeamLeaderRole
			IApplicationRole teamLeaderRole = _applicationRoleCreator.Create(ShippedApplicationRoleNames.TeamLeaderRole, "xxBuiltInTeamLeaderRole", false);
			defaultAggregateRoot.TeamLeaderRole = teamLeaderRole;

			//AgentRole
			IApplicationRole agentRole = _applicationRoleCreator.Create(ShippedApplicationRoleNames.AgentRole, "xxBuildInStandardAgentRole", false);
			defaultAggregateRoot.AgentRole = agentRole;

			_skillTypeCreator = new SkillTypeCreator(_person, _sessionFactory);
			defaultAggregateRoot.SkillTypeInboundTelephony = _skillTypeCreator.Create(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony);
			defaultAggregateRoot.SkillTypeTime = _skillTypeCreator.Create(new Description("SkillTypeTime"), ForecastSource.Time);
			defaultAggregateRoot.SkillTypeEmail = _skillTypeCreator.Create(new Description("SkillTypeEmail"), ForecastSource.Email);
			defaultAggregateRoot.SkillTypeBackoffice = _skillTypeCreator.Create(new Description("SkillTypeBackoffice"), ForecastSource.Backoffice);
			defaultAggregateRoot.SkillTypeProject = _skillTypeCreator.Create(new Description("SkillTypeProject"), ForecastSource.Time);
			defaultAggregateRoot.SkillTypeFax = _skillTypeCreator.Create(new Description("SkillTypeFax"), ForecastSource.Facsimile);
			return defaultAggregateRoot;
		}

		public void Save(DefaultAggregateRoot defaultAggregateRoot)
		{
			//Save businessunit
			_businessUnitCreator.Save(defaultAggregateRoot.BusinessUnit);

			//Save admin role
			_applicationRoleCreator.Save(defaultAggregateRoot.AdministratorRole);
			IAvailableData availableData = _availableDataCreator.Create(defaultAggregateRoot.AdministratorRole, AvailableDataRangeOption.Everyone);
			_availableDataCreator.Save(availableData);

			//Update roles
			updateUserWithRole(_person, defaultAggregateRoot.AdministratorRole);

			//Create businessUnit role
			_applicationRoleCreator.Save(defaultAggregateRoot.BusinessUnitAdministratorRole);
			availableData = _availableDataCreator.Create(defaultAggregateRoot.BusinessUnitAdministratorRole, AvailableDataRangeOption.MyBusinessUnit);
			_availableDataCreator.Save(availableData);

			//Save sitemanager role
			_applicationRoleCreator.Save(defaultAggregateRoot.SiteManagerRole);
			availableData = _availableDataCreator.Create(defaultAggregateRoot.SiteManagerRole, AvailableDataRangeOption.MySite);
			_availableDataCreator.Save(availableData);

			//Save Teamleader
			_applicationRoleCreator.Save(defaultAggregateRoot.TeamLeaderRole);
			availableData = _availableDataCreator.Create(defaultAggregateRoot.TeamLeaderRole, AvailableDataRangeOption.MyOwn);
			_availableDataCreator.Save(availableData);

			//Save Agent role 
			_applicationRoleCreator.Save(defaultAggregateRoot.AgentRole);
			availableData = _availableDataCreator.Create(defaultAggregateRoot.AgentRole, AvailableDataRangeOption.MyTeam);
			_availableDataCreator.Save(availableData);


			//Save Skilltypes, if they exists fetch'em
			if (_skillTypeCreator.Save(defaultAggregateRoot.SkillTypeInboundTelephony))
				defaultAggregateRoot.SkillTypeInboundTelephony =
				    _skillTypeCreator.Fetch(defaultAggregateRoot.SkillTypeInboundTelephony.Description.Name);

			if (_skillTypeCreator.Save(defaultAggregateRoot.SkillTypeTime))
				defaultAggregateRoot.SkillTypeTime =
				    _skillTypeCreator.Fetch(defaultAggregateRoot.SkillTypeTime.Description.Name);

			if (_skillTypeCreator.Save(defaultAggregateRoot.SkillTypeEmail))
				defaultAggregateRoot.SkillTypeEmail =
				    _skillTypeCreator.Fetch(defaultAggregateRoot.SkillTypeEmail.Description.Name);

			if (_skillTypeCreator.Save(defaultAggregateRoot.SkillTypeBackoffice))
				defaultAggregateRoot.SkillTypeBackoffice =
				    _skillTypeCreator.Fetch(defaultAggregateRoot.SkillTypeBackoffice.Description.Name);

			if (_skillTypeCreator.Save(defaultAggregateRoot.SkillTypeProject))
				defaultAggregateRoot.SkillTypeProject =
				    _skillTypeCreator.Fetch(defaultAggregateRoot.SkillTypeProject.Description.Name);

			if (_skillTypeCreator.Save(defaultAggregateRoot.SkillTypeFax))
				defaultAggregateRoot.SkillTypeFax =
				    _skillTypeCreator.Fetch(defaultAggregateRoot.SkillTypeFax.Description.Name);
		}

		private IBusinessUnit createDefaultBusinessUnit()
		{
			_businessUnitCreator = new BusinessUnitCreator(_person, _sessionFactory);
			return _businessUnitCreator.Create(_businessUnitName);
		}

		private void createDefaultPersons()
		{
			_personCreator = new PersonCreator(_sessionFactory);

			//Create convert user
			//_person = _personCreator.Create("DatabaseConverter", "DatabaseConverter", "DatabaseConverter", "byseashare10", _cultureInfo,_timeZone);
			//_personCreator.Save(_person);

			//_person = new Person();
			//_person.SetId(new Guid(SuperUser.Id));
			//_person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
			//	{
			//		ApplicationLogOnName = SuperUser.UserName,
			//		Password = SuperUser.Password
			//	};

			_person = _personCreator.Create(null, null, SuperUser.UserName, null, null, null);
			//_person.PermissionInformation.SetCulture(_cultureInfo);
			//_person.PermissionInformation.SetUICulture(_cultureInfo);
			//_person.PermissionInformation.SetDefaultTimeZone(_timeZone);

			if (!string.IsNullOrEmpty(_newUserName) && !string.IsNullOrEmpty(_newUserPassword))
			{
				var sysAdmin = _personCreator.Create("Admin", "Administrator", _newUserName, _newUserPassword, _cultureInfo,
								     _timeZone);

				if (!string.IsNullOrEmpty(Environment.UserDomainName) && !string.IsNullOrEmpty(Environment.UserName))
				{
					var windowsAuthInfo = new WindowsAuthenticationInfo
								{
									DomainName = Environment.UserDomainName,
									WindowsLogOnName = Environment.UserName
								};
					if (!_personCreator.WindowsUserExists(windowsAuthInfo))
					{
						sysAdmin.WindowsAuthenticationInfo = windowsAuthInfo;
					}
				}

				_personCreator.Save(sysAdmin);
				updateUserWithSuperRole(sysAdmin);
			}
		}

		private void updateUserWithSuperRole(IPerson user)
		{
			if (user.PermissionInformation.ApplicationRoleCollection.Count > 0) return;

			InParameter.NotNull("user", user);
			SetUpdatedOn(user, DateTime.UtcNow);
			SetUpdatedBy(user, user);

			ISession sess = _sessionFactory.OpenSession();

			var role = sess.Get<ApplicationRole>(new Guid("193AD35C-7735-44D7-AC0C-B8EDA0011E5F"));
			if (role != null)
			{
				user.PermissionInformation.AddApplicationRole(role);

				sess.Update(user);
				sess.Flush();
			}
			sess.Close();
		}

		private void updateUserWithRole(IPerson user, IApplicationRole role)
		{
			InParameter.NotNull("user", user);
			user.PermissionInformation.AddApplicationRole(role);
			SetUpdatedOn(user, DateTime.UtcNow);
			SetUpdatedBy(user, user);

			ISession sess = _sessionFactory.OpenSession();
			sess.Update(user);
			sess.Flush();
			sess.Close();
		}

		private static void SetUpdatedOn(IChangeInfo aggregateRoot, DateTime date)
		{
			typeof(AggregateRoot).GetField("_updatedOn", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
			    aggregateRoot, date);
		}
		private static void SetUpdatedBy(IChangeInfo aggregateRoot, IPerson person)
		{
			typeof(AggregateRoot).GetField("_updatedBy", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
			    aggregateRoot, person);
		}

		private void createDefaultKeyPerformanceIndicators()
		{
			_keyPerformanceIndicatorCreator = new KeyPerformanceIndicatorCreator(_person, _sessionFactory);

			KeyPerformanceIndicator keyPerformanceIndicator = _keyPerformanceIndicatorCreator.Create("Readiness (%)", "KpiReadiness", EnumTargetValueType.TargetValueTypePercent, 80, 75, 80, Color.FromArgb(-256), Color.FromArgb(-65536), Color.FromArgb(-16744448));
			_keyPerformanceIndicatorCreator.Save(keyPerformanceIndicator);

			keyPerformanceIndicator = _keyPerformanceIndicatorCreator.Create("Average After Call Work (s)", "KpiAverageAfterCallWork", EnumTargetValueType.TargetValueTypeNumber, 20, 0, 40, Color.FromArgb(-16744448), Color.FromArgb(-16744448), Color.FromArgb(-65536));
			_keyPerformanceIndicatorCreator.Save(keyPerformanceIndicator);

			keyPerformanceIndicator = _keyPerformanceIndicatorCreator.Create("Average Handle Time (s)", "KpiAverageHandleTime", EnumTargetValueType.TargetValueTypeNumber, 140, 30, 180, Color.FromArgb(-16744448), Color.FromArgb(-256), Color.FromArgb(-65536));
			_keyPerformanceIndicatorCreator.Save(keyPerformanceIndicator);

			keyPerformanceIndicator = _keyPerformanceIndicatorCreator.Create("Answered Calls per Scheduled Phone Hour", "KpiAnsweredCallsPerScheduledPhoneHour", EnumTargetValueType.TargetValueTypeNumber, 25, 0, 10, Color.FromArgb(-256), Color.FromArgb(-65536), Color.FromArgb(-16744448));
			_keyPerformanceIndicatorCreator.Save(keyPerformanceIndicator);

			keyPerformanceIndicator = _keyPerformanceIndicatorCreator.Create("Adherence (%)", "KpiAdherence", EnumTargetValueType.TargetValueTypePercent, 80, 75, 80, Color.FromArgb(-256), Color.FromArgb(-65536), Color.FromArgb(-16744448));
			_keyPerformanceIndicatorCreator.Save(keyPerformanceIndicator);

			keyPerformanceIndicator = _keyPerformanceIndicatorCreator.Create("Average Talk Time (s)", "KpiAverageTalkTime", EnumTargetValueType.TargetValueTypeNumber, 120, 30, 160, Color.FromArgb(-16744448), Color.FromArgb(-256), Color.FromArgb(-65536));
			_keyPerformanceIndicatorCreator.Save(keyPerformanceIndicator);

			keyPerformanceIndicator = _keyPerformanceIndicatorCreator.Create("Absenteeism (%)", "KpiAbsenteeism", EnumTargetValueType.TargetValueTypePercent, 5, 4, 6, Color.FromArgb(-256), Color.FromArgb(-16744448), Color.FromArgb(-65536));
			_keyPerformanceIndicatorCreator.Save(keyPerformanceIndicator);
		}
	}
}