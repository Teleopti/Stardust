using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
	public interface IPersonIndexBuilder
	{
		IDictionary<string, IList<IPerson>> BuildIndex(DateOnlyPeriod dateOnlyPeriod);
	}

	public class PersonIndexBuilder : IPersonIndexBuilder
	{
		private readonly IApplicationFunction _applicationFunction;
		private readonly IEnumerable<IPerson> _personToIndex;
		private readonly ITenantLogonDataManagerClient _tenantDataManager;

		public PersonIndexBuilder(IApplicationFunction applicationFunction, IEnumerable<IPerson> personToIndex, ITenantLogonDataManagerClient tenantDataManager)
		{
			_applicationFunction = applicationFunction;
			_personToIndex = personToIndex;
			_tenantDataManager = tenantDataManager;
		}

		public IDictionary<string, IList<IPerson>> BuildIndex(DateOnlyPeriod datePeriod)
		{
			var ret = new ConcurrentDictionary<string, IList<IPerson>>();

			//Sorry don't know how to test this static shit
			var authorization = PrincipalAuthorization.Current();
			var functionPath = _applicationFunction.FunctionPath;
			var personToIndex = _personToIndex.Where(p => authorization.IsPermitted(functionPath, datePeriod.StartDate, p) || authorization.IsPermitted(functionPath, datePeriod.EndDate, p)).ToArray();

			var guids = personToIndex.Select(person => person.Id.GetValueOrDefault()).ToArray();
			var logonInfos = _tenantDataManager.GetLogonInfoModelsForGuids(guids).ToArray();

			foreach (var person in personToIndex)
			{
				if (person.TerminalDate < datePeriod.StartDate)
					continue;
				
				foreach (var period in person.PersonPeriods(datePeriod))
				{
					if (period.PersonContract != null)
					{
						if (period.PersonContract.Contract != null)
							ret.AddOrUpdate(period.PersonContract.Contract.Description.Name, addValueFunction(person), updateValueFunction(person));
						if (period.PersonContract.ContractSchedule != null)
							ret.AddOrUpdate(period.PersonContract.ContractSchedule.Description.Name, addValueFunction(person), updateValueFunction(person));
						if (period.PersonContract.PartTimePercentage != null)
							ret.AddOrUpdate(period.PersonContract.PartTimePercentage.Description.Name, addValueFunction(person), updateValueFunction(person));
					}
					if (period.Team != null)
					{
						ret.AddOrUpdate(period.Team.Description.ToString(), addValueFunction(person), updateValueFunction(person));
						if (period.Team.Site != null) ret.AddOrUpdate(period.Team.Site.Description.ToString(), addValueFunction(person), updateValueFunction(person));
					}

					if (!string.IsNullOrEmpty(period.Note))
						ret.AddOrUpdate(period.Note, addValueFunction(person), updateValueFunction(person));

					if (period.RuleSetBag != null)
					{
						ret.AddOrUpdate(period.RuleSetBag.Description.ToString(), addValueFunction(person), updateValueFunction(person));
					}

					foreach (var skill in period.PersonSkillCollection)
					{
						ret.AddOrUpdate(skill.Skill.Name, addValueFunction(person), updateValueFunction(person));
					}
				}

				string personName = person.Name.ToString();
				if (!string.IsNullOrEmpty(personName))
					ret.AddOrUpdate(personName, addValueFunction(person), updateValueFunction(person));
				if (!string.IsNullOrEmpty(person.Email))
					ret.AddOrUpdate(person.Email, addValueFunction(person), updateValueFunction(person));
				if (!string.IsNullOrEmpty(person.EmploymentNumber))
					ret.AddOrUpdate(person.EmploymentNumber, addValueFunction(person), updateValueFunction(person));
				if (!string.IsNullOrEmpty(person.Note))
					ret.AddOrUpdate(person.Note, addValueFunction(person), updateValueFunction(person));

				var foundInfo = logonInfos.FirstOrDefault(l => l.PersonId.Equals(person.Id));
				if (foundInfo != null)
				{
					if(!string.IsNullOrEmpty(foundInfo.Identity))
						ret.AddOrUpdate(foundInfo.Identity, addValueFunction(person), updateValueFunction(person));

					if (!string.IsNullOrEmpty(foundInfo.LogonName))
						ret.AddOrUpdate(foundInfo.LogonName, addValueFunction(person), updateValueFunction(person));
				}
			}
			return ret;
		}

		private static Func<string, IList<IPerson>, IList<IPerson>> updateValueFunction(IPerson person)
		{
			Func<string, IList<IPerson>, IList<IPerson>> updateValueFunction = (k, v) =>
			{
				v.Add(person);
				return v;
			};
			return updateValueFunction;
		}

		private static List<IPerson> addValueFunction(IPerson person)
		{
			return new List<IPerson>{person};
		}
	}
}