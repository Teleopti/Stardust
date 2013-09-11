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
			fluentCfg.SetRevisionEntity<Revision>(rev => rev.Id, rev => rev.ModifiedAt, new RevisionListener(new UnsafePersonProvider()));
			nhibConfiguration.SetEnversProperty(ConfigurationKey.StoreDataAtDelete, true)
								.SetEnversProperty(ConfigurationKey.DoNotAuditOptimisticLockingField, false)
								.SetEnversProperty(ConfigurationKey.DefaultSchema, AuditingSchema)
								.IntegrateWithEnvers(eventListener, fluentCfg);
		}

		private static void configureSchedule(FluentConfiguration fluentCfg)
		{
			//assignment
			fluentCfg.Audit<PersonAssignment>()
				.Exclude(pa => pa.UpdatedBy)
				.Exclude(pa => pa.UpdatedOn)
				.ExcludeRelationData(pa => pa.Person)
				.ExcludeRelationData(pa => pa.Scenario)
				.ExcludeRelationData(pa => pa.ShiftCategory)
				.ExcludeRelationData("_dayOffTemplate");
			fluentCfg.Audit<ShiftLayer>()
			   .ExcludeRelationData(l => l.Payload);
			fluentCfg.Audit<MainShiftLayer>();
			fluentCfg.Audit<PersonalShiftLayer>();
			fluentCfg.Audit<OvertimeShiftLayer>()
				.ExcludeRelationData(al => al.DefinitionSet);

			//personabsence
			fluentCfg.Audit<PersonAbsence>()
				.Exclude(pa => pa.UpdatedBy)
				.Exclude(pa => pa.UpdatedOn)
				.ExcludeRelationData(pa => pa.Person)
				.ExcludeRelationData(pa => pa.Scenario)
				.ExcludeRelationData(pa => pa.BusinessUnit);
			fluentCfg.Audit<AbsenceLayer>()
				.ExcludeRelationData(pa => pa.Payload);
		}
	}
}