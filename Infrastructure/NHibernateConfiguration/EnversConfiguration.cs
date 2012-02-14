﻿using System;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Envers.Configuration;
using NHibernate.Envers.Configuration.Fluent;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class EnversConfiguration : IEnversConfiguration
	{
		public const string AuditingSchema = "Auditing";
		private readonly AuditSetter _auditSettingProvider;

		public EnversConfiguration()
		{
			Func<ISession, IAuditSetting> auditSettingDel = s =>
			                                 {
			                                    var auditSetting = s.GetSession(EntityMode.Poco).Get<AuditSetting>(AuditSetting.TheId);
															if(auditSetting==null)
																throw new DataSourceException(AuditSettingRepository.MissingAuditSetting);
			                                 	return auditSetting;
			                                 };
			_auditSettingProvider = new AuditSetter(auditSettingDel);
		}

		public IAuditSetter AuditSettingProvider
		{
			get { return _auditSettingProvider; }
		}

		public void Configure(Configuration nhibConfiguration)
		{

			var eventListener = new TeleoptiAuditEventListener(_auditSettingProvider);
			var fluentCfg = new FluentConfiguration();
			configureSchedule(fluentCfg);
			nhibConfiguration.SetProperty(ConfigurationKey.StoreDataAtDelete, "true");
			nhibConfiguration.SetProperty(ConfigurationKey.DoNotAuditOptimisticLockingField, "false");
			nhibConfiguration.SetProperty(ConfigurationKey.DefaultSchema, AuditingSchema);
			fluentCfg.SetRevisionEntity<Revision>(rev => rev.Id, rev => rev.ModifiedAt, new RevisionListener(new UnsafePersonProvider()));
			nhibConfiguration.IntegrateWithEnvers(eventListener, fluentCfg);
		}

		private static void configureSchedule(FluentConfiguration fluentCfg)
		{
			//assignment
			fluentCfg.Audit<PersonAssignment>()
				.Exclude(pa => pa.CreatedBy)
				.Exclude(pa => pa.UpdatedBy)
				.Exclude(pa => pa.CreatedOn)
				.Exclude(pa => pa.UpdatedOn)
				.ExcludeRelationData(pa => pa.Person)
				.ExcludeRelationData(pa => pa.Scenario)
				.ExcludeRelationData(pa => pa.BusinessUnit);
			fluentCfg.Audit<MainShift>()
				.ExcludeRelationData(ms => ms.ShiftCategory);
			fluentCfg.Audit<MainShiftActivityLayer>()
				.ExcludeRelationData(al => al.Payload);
			fluentCfg.Audit<PersonalShift>();
			fluentCfg.Audit<PersonalShiftActivityLayer>()
				.ExcludeRelationData(al => al.Payload);
			fluentCfg.Audit<OvertimeShift>();
			fluentCfg.Audit<OvertimeShiftActivityLayer>()
				.ExcludeRelationData(al => al.DefinitionSet)
				.ExcludeRelationData(al => al.Payload);

			//personabsence
			fluentCfg.Audit<PersonAbsence>()
				.Exclude(pa => pa.CreatedBy)
				.Exclude(pa => pa.UpdatedBy)
				.Exclude(pa => pa.CreatedOn)
				.Exclude(pa => pa.UpdatedOn)
				.ExcludeRelationData(pa => pa.Person)
				.ExcludeRelationData(pa => pa.Scenario)
				.ExcludeRelationData(pa => pa.BusinessUnit);
			fluentCfg.Audit<AbsenceLayer>()
				.ExcludeRelationData(pa => pa.Payload);

			//persondayoff
			fluentCfg.Audit<PersonDayOff>()
				.Exclude(pa => pa.CreatedBy)
				.Exclude(pa => pa.UpdatedBy)
				.Exclude(pa => pa.CreatedOn)
				.Exclude(pa => pa.UpdatedOn)
				.ExcludeRelationData(pa => pa.Person)
				.ExcludeRelationData(pa => pa.Scenario)
				.ExcludeRelationData(pa => pa.BusinessUnit);
		}
	}
}