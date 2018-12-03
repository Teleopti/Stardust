using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    public class PersonPeriodChildModel : GridViewModelBase<IPersonPeriod>, IPersonPeriodModel
    {
        private IList<IPersonSkill> _personSkillCollection;
        private IList<SiteTeamModel> _siteTeamAdapterCollection;
        private static readonly ExternalLogOnDisplay _externalLogOnDisplay = new ExternalLogOnDisplay();
        private ExternalLogOnParser _externalLogOnParser;
    	private PersonSkillStringParser _personSkillStringParser;

        public string FullName { get; set; }

        public DateOnly PeriodDate
        {
            get => ContainedEntity.StartDate;
			set
            {
                if (value != ContainedEntity.StartDate)
                {
					Parent.ChangePersonPeriodStartDate(value,ContainedEntity);
					addPersonEmploymentChangedEvent(true);
                }
            }
        }

        public IPerson Parent => (IPerson)ContainedEntity.Root();

        public IPersonContract PersonContract
        {
            get => ContainedEntity.PersonContract;
			set
            {
                if (!ContainedEntity.PersonContract.Equals(value))
                {
                    ContainedEntity.PersonContract = value;
                }
            }
        }

        public IPartTimePercentage PartTimePercentage
        {
            get => ContainedEntity.PersonContract?.PartTimePercentage;
			set
            {
                if (ContainedEntity.PersonContract?.PartTimePercentage != null && !ContainedEntity.PersonContract.PartTimePercentage.Equals(value))
                {
                    ContainedEntity.PersonContract.PartTimePercentage = value;
					addPersonEmploymentChangedEvent();

				}
            }
        }

        public IContract Contract
        {
            get => ContainedEntity.PersonContract?.Contract;
			set
            {
                if (ContainedEntity.PersonContract?.Contract != null && !ContainedEntity.PersonContract.Contract.Equals(value))
                {
                    ContainedEntity.PersonContract.Contract = value;
					addPersonEmploymentChangedEvent();

				}
            }
        }

		private void addPersonEmploymentChangedEvent(bool getFromPreviousPeriod = false)
		{
			var startDate = ContainedEntity.StartDate;
			if (getFromPreviousPeriod)
			{
				var previousPeriod = ((Person) ContainedEntity.Parent).PreviousPeriod(ContainedEntity);
				if (previousPeriod != null)
					startDate = previousPeriod.StartDate;
			}
			Parent.AddPersonEmploymentChangeEvent(new PersonEmploymentChangedEvent
			{
				PersonId = ContainedEntity.Parent.Id.GetValueOrDefault(),
				FromDate = startDate
			});
		}

		public IContractSchedule ContractSchedule
        {
            get => ContainedEntity.PersonContract?.ContractSchedule;
			set
            {
                if (ContainedEntity.PersonContract?.ContractSchedule != null && !ContainedEntity.PersonContract.ContractSchedule.Equals(value))
                {
                    ContainedEntity.PersonContract.ContractSchedule = value;
					addPersonEmploymentChangedEvent();

				}
            }
        }

        public string PersonSkills
        {
            get => getPersonSkills();
			set
            {
                Parent.ResetPersonSkills(ContainedEntity);
                SetPersonSkill(value);
            }
        }

        public void SetPersonSkillCollection(IList<IPersonSkill> personSkillCollection)
        {
            InParameter.NotNull("personSkillCollection", personSkillCollection);
            _personSkillCollection = personSkillCollection;
			_personSkillStringParser = new PersonSkillStringParser(_personSkillCollection);
        }


        private void SetPersonSkill(string value)
        {
			if (ContainedEntity == null) return;
			var personSkills = _personSkillStringParser.Parse(value);
			foreach (var personSkill in personSkills)
			{
				Parent.AddSkill(personSkill,ContainedEntity);
			}
        }

        private string getPersonSkills()
        {
	        if (ContainedEntity?.PersonSkillCollection == null) return string.Empty;

			return string.Join(", ", ContainedEntity.PersonSkillCollection.Select(s => s.Skill.Name).OrderBy(s => s));
		}

        public IRuleSetBag RuleSetBag
        {
            get => ContainedEntity.RuleSetBag;
			set
            {
                if (ContainedEntity.RuleSetBag == null || !ContainedEntity.RuleSetBag.Equals(value))
                    ContainedEntity.RuleSetBag = value;
            }
        }

        public bool CanGray => false;

        public string ExternalLogOnNames
        {
            get => ContainedEntity == null ? string.Empty : _externalLogOnParser.GetExternalLogOnsDisplayText(ContainedEntity.ExternalLogOnCollection);
			set
            {
                if (ContainedEntity != null)
                {
					Parent.ResetExternalLogOn(ContainedEntity);
                    var selectedExternalLogOns = _externalLogOnParser.ParsePersonExternalLogOn(value);
                    foreach (var selectedExternalLogOn in selectedExternalLogOns)
                    {
                        Parent.AddExternalLogOn(selectedExternalLogOn,ContainedEntity);
                    }
                }
            }
        }

        public string Note
        {
            get => ContainedEntity.Note;
			set => ContainedEntity.Note = value;
		}

        public void SetPersonExternalLogOnCollection(IList<IExternalLogOn> externalLogOnCollection)
        {
            InParameter.NotNull("externalLogOnCollection", externalLogOnCollection);
            _externalLogOnParser = new ExternalLogOnParser(externalLogOnCollection,_externalLogOnDisplay);
        }


        public void SetSiteTeamAdapterCollection(IList<SiteTeamModel> siteTeamAdapterCollection)
        {
            InParameter.NotNull("siteTeamAdapterCollection", siteTeamAdapterCollection);
            _siteTeamAdapterCollection = siteTeamAdapterCollection;
        }

        public IPersonPeriod Period => ContainedEntity;

        public SiteTeamModel SiteTeam
        {
            get
            {
				return _siteTeamAdapterCollection.FirstOrDefault(s => s.Team == Period.Team && s.Site == Period.Team.Site);
            }
            set
            {
                if (!ContainedEntity.Team.Equals(value.Team))
                {
					Parent.ChangeTeam(value.Team, ContainedEntity);
                }
            }
        }

		public IBudgetGroup BudgetGroup
		{
			get
			{
				if (ContainedEntity != null)
				{
					return ContainedEntity.BudgetGroup;
				}
				return PersonPeriodModel.NullBudgetGroup;
			}
			set
			{
				if (ContainedEntity != null)
				{
					if (value == PersonPeriodModel.NullBudgetGroup)
					{
						value = null;
					}
					ContainedEntity.BudgetGroup = value;
				}
			}
		}
    }
}
