using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Impl;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PeopleForShiftTradeFinder : IPeopleForShiftTradeFinder
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PeopleForShiftTradeFinder(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IList<IAuthorizeOrganisationDetail> GetPeople (IPerson personFrom, DateOnly shiftTradeDate, IList<Guid> groupIdList,
			string name, NameFormatSetting nameFormat = NameFormatSetting.FirstNameThenLastName)
		{
			var useChineseNameFormat = name != null && StringHelper.StringContainsChinese(name);
			var firstNameFirst = nameFormat == NameFormatSetting.FirstNameThenLastName;

			const string sql = "exec ReadModel.FindPeopleForShiftTrade @scheduleDate=:scheduleDate, "
							   + "@groupIdList=:groupIdList, @businessUnitId=:businessUnitId, @name=:name, "
							   + "@noSpaceInName = :noSpaceInName, "
							   + "@firstNameFirst = :firstNameFirst, "
							   + "@workflowControlSetId = :workflowControlSetId, "
							   + "@fromPersonId = :fromPersonId, "
							   + "@shiftStartUTC = :shiftStartUTC, "
							   + "@shiftEndUTC = :shiftEndUTC";
			var result = new List<IAuthorizeOrganisationDetail>();
			//ROBTODO: Temporary - Person From Shift Period is currently not being passed to Get People, this is currently not being used by the query,
			//	but is a requested parameter to add for further optimisation
			var dummyDateTimePeriod = new DateTimePeriod(DateTime.Today.ToUniversalTime(), DateTime.Now.ToUniversalTime());
			groupIdList.Batch(100).ForEach(l =>
			{
				var batchResult = ((NHibernateUnitOfWork)_unitOfWork.Current()).Session.CreateSQLQuery(sql)
				.SetDateOnly("scheduleDate", shiftTradeDate)
				.SetString("groupIdList", string.Join(",", l))
				.SetString("name", name)
				.SetBoolean("noSpaceInName", useChineseNameFormat)
				.SetBoolean("firstNameFirst", firstNameFirst)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetGuid("workflowControlSetId", personFrom.WorkflowControlSet.Id.GetValueOrDefault())
				.SetGuid("fromPersonId", personFrom.Id.GetValueOrDefault())
				.SetDateTime("shiftStartUTC", dummyDateTimePeriod.StartDateTime)
				.SetDateTime("shiftEndUTC", dummyDateTimePeriod.EndDateTime)
				.SetResultTransformer(Transformers.AliasToBean(typeof(PersonSelectorShiftTrade)))
				.SetReadOnly(true)
				.List<IAuthorizeOrganisationDetail>();
				result.AddRange(batchResult);
			});
			return result;
		}

		private Guid getBusinessUnitId()
		{
			var filter = (FilterImpl)_unitOfWork.Session().GetEnabledFilter("businessUnitFilter");
			object businessUnitId;
			if (!filter.Parameters.TryGetValue("businessUnitParameter", out businessUnitId))
			{
				return ((ITeleoptiIdentity)ClaimsPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();
			}
			return Guid.Parse(businessUnitId.ToString());
		}
	}
}
