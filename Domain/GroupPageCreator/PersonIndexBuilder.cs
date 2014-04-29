using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public interface IPersonIndexBuilder
    {
        void ChangePeriod(DateOnlyPeriod dateOnlyPeriod);
    }

    public class PersonIndexBuilder : ISearchIndexBuilder<IPerson>, IPersonIndexBuilder
    {
        private readonly IApplicationFunction _applicationFunction;
        private IEnumerable<IPerson> _personToIndex;
        private DateOnlyPeriod _datePeriod;

        public PersonIndexBuilder(IApplicationFunction applicationFunction, IEnumerable<IPerson> personToIndex, DateOnlyPeriod datePeriod)
        {
            _applicationFunction = applicationFunction;
            _personToIndex = personToIndex;
            _datePeriod = datePeriod;
        }

        public IDictionary<IPerson, string> BuildIndex()
        {
            var ret = new Dictionary<IPerson, string>();
             
            //Sorry don't know how to test this static shit
            var authorization = PrincipalAuthorization.Instance();
            var functionPath = _applicationFunction.FunctionPath;
            _personToIndex = _personToIndex.Where(p => authorization.IsPermitted(functionPath, _datePeriod.StartDate, p) || authorization.IsPermitted(functionPath, _datePeriod.EndDate, p));
           
            foreach (var person in _personToIndex)
            {
                if(person.TerminalDate<_datePeriod.StartDate)
                    continue;
                var sb = new StringBuilder();
                var delim = " ";

                foreach (var period in person.PersonPeriods(_datePeriod))
                {
                   
                    if (period.PersonContract != null)
                    {
                        if (period.PersonContract.Contract != null)
                            sb.Append(period.PersonContract.Contract.Description.Name + delim);
                        if (period.PersonContract.ContractSchedule != null)
                            sb.Append(period.PersonContract.ContractSchedule.Description + delim);
                    }
                    if (period.Team != null)
                    {
                        sb.Append(period.Team.Description + delim);
                        if (period.Team.Site != null) sb.Append(period.Team.Site.Description + delim);
                    }

                    if (period.PersonContract != null)
                        if (period.PersonContract.PartTimePercentage != null)
                            sb.Append(period.PersonContract.PartTimePercentage.Description + delim);
                    //if (period.BudgetGroup != null)
                    //    sb.Append(period.BudgetGroup.Name + delim);
                    sb.Append(period.Note);
                    if (period.RuleSetBag != null)
                        sb.Append(period.RuleSetBag.Description + delim);

                    foreach (var skill in period.PersonSkillCollection)
                    {
                        sb.Append(skill.Skill.Name + delim);
                    }
                }

                sb.Append(person.Name + delim);
                sb.Append(person.Email + delim);
                sb.Append(person.EmploymentNumber + delim);
                sb.Append(person.Note + delim);
                if (person.PermissionInformation != null)
                {
                    if (person.AuthenticationInfo != null)
                        sb.Append(person.AuthenticationInfo.Identity + delim);
                    if (person.ApplicationAuthenticationInfo != null)
                        sb.Append(person.ApplicationAuthenticationInfo.ApplicationLogOnName + delim);
                }

                ret.Add(person, sb.ToString());
            }
            return ret;
        }

        public void ChangePeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            _datePeriod = dateOnlyPeriod;
        }
    }
}