using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    public class PersonPeriodChildModel : GridViewModelBase<IPersonPeriod>, IPersonPeriodModel
    {
        private IList<IPersonSkill> _personSkillCollection;
        private IList<SiteTeamModel> _siteTeamAdapterCollection;
        private static readonly ExternalLogOnDisplay _externalLogOnDisplay = new ExternalLogOnDisplay();
        private ExternalLogOnParser _externalLogOnParser;
    	private PersonSkillStringParser _personSkillStringParser;

    	/// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the period date.
        /// </summary>
        /// <value>The period date.</value>
        public DateOnly PeriodDate
        {
            get
            {
                return ContainedEntity.StartDate;
            }
            set
            {
                if (value != ContainedEntity.StartDate)
                {
					Parent.ChangePersonPeriodStartDate(value,ContainedEntity);
                }
            }
        }

        public IPerson Parent => (IPerson)ContainedEntity.Root();

	    /// <summary>
        /// Gets or sets the current person contract.
        /// </summary>
        /// <value>The current person contract.</value>
        public IPersonContract PersonContract
        {
            get
            {
                return ContainedEntity.PersonContract;
            }
            set
            {
                if (!ContainedEntity.PersonContract.Equals(value))
                {
                    ContainedEntity.PersonContract = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current part time percentage.
        /// </summary>
        /// <value>The current part time percentage.</value>
        public IPartTimePercentage PartTimePercentage
        {
            get
            {
	            return ContainedEntity.PersonContract?.PartTimePercentage;
            }
            set
            {
                if (ContainedEntity.PersonContract?.PartTimePercentage != null && !ContainedEntity.PersonContract.PartTimePercentage.Equals(value))
                {
                    ContainedEntity.PersonContract.PartTimePercentage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current contract.
        /// </summary>
        /// <value>The current contract.</value>
        public IContract Contract
        {
            get
            {
	            return ContainedEntity.PersonContract?.Contract;
            }
            set
            {
                if (ContainedEntity.PersonContract?.Contract != null && !ContainedEntity.PersonContract.Contract.Equals(value))
                {
                    ContainedEntity.PersonContract.Contract = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current contract schedule.
        /// </summary>
        /// <value>The current contract schedule.</value>
        public IContractSchedule ContractSchedule
        {
            get
            {
	            return ContainedEntity.PersonContract?.ContractSchedule;
            }
            set
            {
                if (ContainedEntity.PersonContract?.ContractSchedule != null && !ContainedEntity.PersonContract.ContractSchedule.Equals(value))
                {
                    ContainedEntity.PersonContract.ContractSchedule = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the person skills.
        /// </summary>
        /// <value>The person skills.</value>
        public string PersonSkills
        {
            get
            {
                return GetPersonSkills();
            }
            set
            {
                Parent.ResetPersonSkills(ContainedEntity);
                SetPersonSkill(value);
            }
        }

        /// <summary>
        /// Sets the person skill collection.
        /// </summary>
        /// <value>The person skill collection.</value>
        public void SetPersonSkillCollection(IList<IPersonSkill> personSkillCollection)
        {
            InParameter.NotNull("personSkillCollection", personSkillCollection);
            _personSkillCollection = personSkillCollection;
			_personSkillStringParser = new PersonSkillStringParser(_personSkillCollection);
        }

        /// <summary>
        /// Sets the person skill.
        /// </summary>
        /// <param name="value">The value.</param>
        private void SetPersonSkill(string value)
        {
			if (ContainedEntity == null) return;
			var personSkills = _personSkillStringParser.Parse(value);
			foreach (var personSkill in personSkills)
			{
				Parent.AddSkill(personSkill,ContainedEntity);
			}
        }

        /// <summary>
        /// Gets the person skills.
        /// </summary>
        /// <returns></returns>
        private string GetPersonSkills()
        {
	        if (ContainedEntity == null) return string.Empty;
	        var personSkillString = new StringBuilder();

	        if (ContainedEntity.PersonSkillCollection != null)
	        {
		        IEnumerable<IPersonSkill> personSkillCollection = ContainedEntity.PersonSkillCollection.OrderBy(s => s.Skill.Name);

		        foreach (var personSkill in personSkillCollection)
		        {
			        if (personSkillString.Length>0)
				        personSkillString.Append(", ");

			        personSkillString.Append(personSkill.Skill.Name);
		        }
	        }
	        return personSkillString.ToString();
        }

        /// <summary>
        /// Gets or sets the current rule set bag.
        /// </summary>
        /// <value>The current rule set bag.</value>
        public IRuleSetBag RuleSetBag
        {
            get
            {
                return ContainedEntity.RuleSetBag;
            }
            set
            {
                if (ContainedEntity.RuleSetBag == null || !ContainedEntity.RuleSetBag.Equals(value))
                    ContainedEntity.RuleSetBag = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can gray.
        /// </summary>
        /// <value><c>true</c> if this instance can gray; otherwise, <c>false</c>.</value>
        public bool CanGray => false;

	    /// <summary>
        /// Gets or sets the person external log on names.
        /// </summary>
        /// <value>The person external log on names.</value>
        public string ExternalLogOnNames
        {
            get
            {
	            return ContainedEntity == null ? string.Empty : _externalLogOnParser.GetExternalLogOnsDisplayText(ContainedEntity.ExternalLogOnCollection);
            }
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

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        public string Note
        {
            get { return ContainedEntity.Note; }
            set { ContainedEntity.Note = value; }
        }

        /// <summary>
        /// Sets the person external log on collection.
        /// </summary>
        /// <param name="externalLogOnCollection">The external log on collection.</param>
        public void SetPersonExternalLogOnCollection(IList<IExternalLogOn> externalLogOnCollection)
        {
            InParameter.NotNull("externalLogOnCollection", externalLogOnCollection);
            _externalLogOnParser = new ExternalLogOnParser(externalLogOnCollection,_externalLogOnDisplay);
        }

        /// <summary>
        /// Sets the site team adapter collection.
        /// </summary>
        public void SetSiteTeamAdapterCollection(IList<SiteTeamModel> siteTeamAdapterCollection)
        {
            InParameter.NotNull("siteTeamAdapterCollection", siteTeamAdapterCollection);
            _siteTeamAdapterCollection = siteTeamAdapterCollection;
        }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        public IPersonPeriod Period => ContainedEntity;

	    /// <summary>
        /// Gets or sets the site team.
        /// </summary>
        /// <value>The site team.</value>
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
