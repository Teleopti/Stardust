using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;
using System.Globalization;

namespace Teleopti.Ccc.WinCode.Meetings
{
    /// <summary>
    /// Represents a .
    /// </summary>
    public class ContactPersonViewModel : EntityContainer<IPerson>
    {
        private readonly CommonNameDescriptionSetting _commonNameDescriptionSetting;
        private DateOnly _currentDate;
        private IPersonPeriod _personPeriod;

        public string FirstName
        {
            get { return ContainedEntity.Name.FirstName; }
        }

        public string LastName
        {
            get { return ContainedEntity.Name.LastName; }
        }

        public IPersonPeriod CurrentPeriod
        {
            get
            {
                return _personPeriod;
            }
        }

        public ITeam TeamBelong
        {
            get
            {
                if (CurrentPeriod == null) return null;
                return CurrentPeriod.Team;
            }
        }

        public ISite SiteBelong
        {
            get
            {
                if (CurrentPeriod == null) return null;
                return CurrentPeriod.Team.Site;
            }
        }

        public string Skills
        {
            get 
            {
                if (CurrentPeriod == null) return string.Empty;
                return string.Join("; ", CurrentPeriod.PersonSkillCollection.Select(s => s.Skill.Name).ToArray());
            }
        }

        public string Email
        {
            get { return ContainedEntity.Email; }
        }

        public DateOnly CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                _personPeriod = ContainedEntity.Period(_currentDate);
            }
        }

        public string FullName
        {
            get { return _commonNameDescriptionSetting.BuildCommonNameDescription(ContainedEntity); }
        }

        public string EmploymentNumber
        {
            get { return ContainedEntity.EmploymentNumber; }
        }

        public ContactPersonViewModel(IPerson entity) : this(entity,new CommonNameDescriptionSetting())
        {
        }

        public ContactPersonViewModel(IPerson entity, CommonNameDescriptionSetting commonNameDescriptionSetting) : base(entity)
        {
            CurrentDate = new DateOnly(DateTime.Today);
            _commonNameDescriptionSetting = commonNameDescriptionSetting;
        }

        public static IEnumerable<ContactPersonViewModel> Parse(IEnumerable<IPerson> people, CommonNameDescriptionSetting commonNameDescriptionSetting)
        {
            foreach (IPerson person in people)
            {
                yield return new ContactPersonViewModel(person,commonNameDescriptionSetting);
            }
        }

        public bool FilterByPeriod(DateTime? value)
        {
            if (!value.HasValue) return true;

            return ContainedEntity.Period(new DateOnly(value.Value)) != null;
        }

        public bool FilterByValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return true;

            var tmp = value.ToUpper(CultureInfo.CurrentUICulture);
            bool firstNameFound = !string.IsNullOrEmpty(FirstName) && FirstName.ToUpper(CultureInfo.CurrentUICulture).Contains(tmp);
            bool lastNameFound = !string.IsNullOrEmpty(LastName) && LastName.ToUpper(CultureInfo.CurrentUICulture).Contains(tmp);
            bool teamFound = TeamBelong != null && TeamBelong.Description.Name.ToUpper(CultureInfo.CurrentUICulture).Contains(tmp);
            bool siteFound = SiteBelong != null && SiteBelong.Description.Name.ToUpper(CultureInfo.CurrentUICulture).Contains(tmp);
            bool skillsFound = !string.IsNullOrEmpty(Skills) && Skills.ToUpper(CultureInfo.CurrentUICulture).Contains(tmp);
            bool emailFound = !string.IsNullOrEmpty(Email) && Email.ToUpper(CultureInfo.CurrentUICulture).Contains(tmp);
            bool employmentNumberFound = !string.IsNullOrEmpty(EmploymentNumber) && EmploymentNumber.ToUpper(CultureInfo.CurrentUICulture).Contains(tmp);

            return firstNameFound || lastNameFound || teamFound || siteFound || skillsFound || emailFound ||employmentNumberFound;
        }

        public bool FilterByPermission(string functionPath, DateOnly? dateOnly)
        {
            if ((functionPath != null) && (dateOnly.HasValue) && (ContainedEntity != null))
            {
                return PrincipalAuthorization.Instance().IsPermitted(functionPath,
                                                                       dateOnly.Value,ContainedEntity);
            }
            return false;
        }
    }
}
