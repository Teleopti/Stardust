using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Audit
{
	public class PersonAccessContextReaderService : IPersonAccessContextReaderService
	{
		private readonly IPersonAccessAuditRepository _personAccessAuditRepository;
		private readonly IApplicationRoleRepository _applicationRoleRepository;
		
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;

		public PersonAccessContextReaderService(IPersonAccessAuditRepository personAccessAuditRepository, IApplicationRoleRepository applicationRoleRepository,  ICommonAgentNameProvider commonAgentNameProvider)
		{
			_personAccessAuditRepository = personAccessAuditRepository;
			_applicationRoleRepository = applicationRoleRepository;
			_commonAgentNameProvider = commonAgentNameProvider;
		}

		private IEnumerable<AuditServiceModel> getAuditServiceModel(IEnumerable<IPersonAccess> personAccessAudit)
		{
			var auditServiceModelList = new List<AuditServiceModel>();
			var commonAgentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var audit in personAccessAudit)
			{
				var auditServiceModel = new AuditServiceModel
				{
					TimeStamp = audit.TimeStamp, Context = "PersonAccess", Action = audit.Action,
					ActionPerformedBy = extractPersonAuditInfo(audit.ActionPerformedBy, commonAgentNameSetting)
						//audit.ActionPerformedBy.Name.ToString(NameOrderOption.FirstNameLastName)
				};
				var deserializedRole = JsonConvert.DeserializeObject<PersonAccessModel>(audit.Data);
				var appRole = _applicationRoleRepository.Load(deserializedRole.RoleId);
				var actionPerformedOn = extractPersonAuditInfo(audit.ActionPerformedOn, commonAgentNameSetting);
				auditServiceModel.Data = $"Person: {actionPerformedOn} Role: {appRole.Name} Action: {audit.Action}";
				auditServiceModelList.Add(auditServiceModel);
			}

			return auditServiceModelList;
		}

		private string extractPersonAuditInfo(string person, ICommonNameDescriptionSetting commonAgentNameSetting)
		{
			var personInfo = JsonConvert.DeserializeObject<PersonAuditInfo>(person);
			return commonAgentNameSetting.BuildFor(personInfo);
		}

		

		public IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate)
		{
			var staffingAudit = _personAccessAuditRepository.LoadAudits(personId, startDate, endDate);
			return getAuditServiceModel(staffingAudit);
		}

		
	}

	internal class PersonAccessModel
	{
		public Guid RoleId { get; set; }  
		public string Name { get; set; }  
	}
}