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
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.Infrastructure.Audit
{
	public class PersonAccessContextReaderService : IPersonAccessContextReaderService
	{
		private readonly IPersonAccessAuditRepository _personAccessAuditRepository;
		private readonly IApplicationRoleRepository _applicationRoleRepository;
		
		private readonly ICommonAgentNameProvider _commonAgentNameProvider;
		private readonly IUserCulture _userCulture;

		public PersonAccessContextReaderService(IPersonAccessAuditRepository personAccessAuditRepository, IApplicationRoleRepository applicationRoleRepository,  ICommonAgentNameProvider commonAgentNameProvider, IUserCulture userCulture)
		{
			_personAccessAuditRepository = personAccessAuditRepository;
			_applicationRoleRepository = applicationRoleRepository;
			_commonAgentNameProvider = commonAgentNameProvider;
			_userCulture = userCulture;
		}

		private IEnumerable<AuditServiceModel> getAuditServiceModel(IEnumerable<IPersonAccess> personAccessAudit)
		{
			var auditServiceModelList = new List<AuditServiceModel>();
			var commonAgentNameSetting = _commonAgentNameProvider.CommonAgentNameSettings;
			foreach (var audit in personAccessAudit)
			{
				var auditServiceModel = new AuditServiceModel
				{
					TimeStamp = audit.TimeStamp,
					Context = Resources.AuditTrailPersonAccessContext,
					Action = Resources.ResourceManager.GetString(audit.Action, _userCulture.GetCulture()) ?? audit.Action,
					ActionPerformedBy = extractPersonAuditInfo(audit.ActionPerformedBy, commonAgentNameSetting)
						//audit.ActionPerformedBy.Name.ToString(NameOrderOption.FirstNameLastName)
				};
				var deserializedRole = JsonConvert.DeserializeObject<PersonAccessModel>(audit.Data);
				var appRole = deserializedRole.Name;
				var actionPerformedOn = extractPersonAuditInfo(audit.ActionPerformedOn, commonAgentNameSetting);
				auditServiceModel.Data = $"{Resources.AuditTrailPerson}: {actionPerformedOn}, {Resources.AuditTrailRole}: {appRole}";
				auditServiceModelList.Add(auditServiceModel);
			}

			return auditServiceModelList;
		}

		private string extractPersonAuditInfo(string person, ICommonNameDescriptionSetting commonAgentNameSetting)
		{
			var personInfo = JsonConvert.DeserializeObject<PersonAuditInfo>(person);
			return commonAgentNameSetting.BuildFor(personInfo);
		}

		

		public IEnumerable<AuditServiceModel> LoadAudits(IPerson personId, DateTime startDate, DateTime endDate, string searchword)
		{
			var personAccessAudit = _personAccessAuditRepository.LoadAudits(personId, startDate, endDate, searchword);
			return getAuditServiceModel(personAccessAudit);
		}

		
	}

	internal class PersonAccessModel
	{
		public Guid RoleId { get; set; }  
		public string Name { get; set; }  
	}
}
