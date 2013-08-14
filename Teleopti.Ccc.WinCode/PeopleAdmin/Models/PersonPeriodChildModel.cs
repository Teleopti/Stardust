using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    public class PersonPeriodChildModel : GridViewModelBase<IPersonPeriod>, IPersonPeriodModel
    {
        private IList<IPersonSkill> _personSkillCollection;
        private IList<SiteTeamModel> _siteTeamAdapterCollection;
        private SiteTeamModel _siteTeamModel;
        private static readonly ExternalLogOnDisplay _externalLogOnDisplay = new ExternalLogOnDisplay();
        private ExternalLogOnParser _externalLogOnParser;
    	private PersonSkillStringParser _personSkillStringParser;

    	/// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>The full name.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-23
        /// </remarks>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the period date.
        /// </summary>
        /// <value>The period date.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-20
        /// </remarks>
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

        public IPerson Parent
        {
            get { return (IPerson)ContainedEntity.Root(); }
        }

        /// <summary>
        /// Gets or sets the current team.
        /// </summary>
        /// <value>The current team.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-20
        /// </remarks>
        public ITeam Team
        {
            get
            {
                return ContainedEntity.Team;
            }
            set
            {
                if (!ContainedEntity.Team.Equals(value))
                {
                    ContainedEntity.Team = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current person contract.
        /// </summary>
        /// <value>The current person contract.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-20
        /// </remarks>
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
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-20
        /// </remarks>
        public IPartTimePercentage PartTimePercentage
        {
            get
            {
                if (ContainedEntity.PersonContract != null && ContainedEntity.PersonContract.PartTimePercentage != null)
                {
                    return ContainedEntity.PersonContract.PartTimePercentage;
                }
                return null;
            }
            set
            {
                if (ContainedEntity.PersonContract != null && 
                    ContainedEntity.PersonContract.PartTimePercentage != null && 
                    !ContainedEntity.PersonContract.PartTimePercentage.Equals(value))
                {
                    ContainedEntity.PersonContract.PartTimePercentage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current contract.
        /// </summary>
        /// <value>The current contract.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-20
        /// </remarks>
        public IContract Contract
        {
            get
            {
                if (ContainedEntity.PersonContract != null && ContainedEntity.PersonContract.Contract != null)
                {
                    return ContainedEntity.PersonContract.Contract;
                }
                return null;
            }
            set
            {
                if (ContainedEntity.PersonContract != null && 
                    ContainedEntity.PersonContract.Contract != null &&
                    !ContainedEntity.PersonContract.Contract.Equals(value))
                {
                    ContainedEntity.PersonContract.Contract = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current contract schedule.
        /// </summary>
        /// <value>The current contract schedule.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-20
        /// </remarks>
        public IContractSchedule ContractSchedule
        {
            get
            {
                if (ContainedEntity.PersonContract != null && ContainedEntity.PersonContract.ContractSchedule != null)
                {
                    return ContainedEntity.PersonContract.ContractSchedule;
                }
                return null;
            }
            set
            {
                if (ContainedEntity.PersonContract != null && 
                    ContainedEntity.PersonContract.ContractSchedule != null &&
                    !ContainedEntity.PersonContract.ContractSchedule.Equals(value))
                {
                    ContainedEntity.PersonContract.ContractSchedule = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the person skills.
        /// </summary>
        /// <value>The person skills.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-05-27
        /// </remarks>
        public string PersonSkills
        {
            get
            {
                return GetPersonSkills();
            }
            set
            {
                ContainedEntity.ResetPersonSkill();
                SetPersonSkill(value);
            }
        }

        /// <summary>
        /// Sets the person skill collection.
        /// </summary>
        /// <value>The person skill collection.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-06-15
        /// </remarks>
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
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-30
        /// </remarks>
        private void SetPersonSkill(string value)
        {
			if (ContainedEntity == null) return;
			var personSkills = _personSkillStringParser.Parse(value);
			foreach (var personSkill in personSkills)
			{
				ContainedEntity.AddPersonSkill(personSkill);
			}
        }

        /// <summary>
        /// Gets the person skills.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-05-30
        /// </remarks>
        private string GetPersonSkills()
        {
            if (ContainedEntity != null)
            {
                StringBuilder personSkillString = new StringBuilder();

                if (ContainedEntity.PersonSkillCollection != null)
                {
                    IEnumerable<IPersonSkill> personSkillCollection = ContainedEntity.PersonSkillCollection.OrderBy(s => s.Skill.Name);

                    foreach (IPersonSkill personSkill in personSkillCollection)
                    {
                        if (personSkillString.Length>0)
                            personSkillString.Append(", ");

                        personSkillString.Append(personSkill.Skill.Name);
                    }
                }
                return personSkillString.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets or sets the current rule set bag.
        /// </summary>
        /// <value>The current rule set bag.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-27
        /// </remarks>
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
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-07-23
        /// </remarks>
        public bool CanGray
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the person external log on names.
        /// </summary>
        /// <value>The person external log on names.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-08-14
        /// </remarks>
        public string ExternalLogOnNames
        {
            get
            {
                if (ContainedEntity == null) return string.Empty;

                return _externalLogOnParser.GetExternalLogOnsDisplayText(ContainedEntity.ExternalLogOnCollection);
            }
            set
            {
                if (ContainedEntity != null)
                {
                    ContainedEntity.ResetExternalLogOn();
                    var selectedExternalLogOns = _externalLogOnParser.ParsePersonExternalLogOn(value);
                    foreach (var selectedExternalLogOn in selectedExternalLogOns)
                    {
                        ContainedEntity.AddExternalLogOn(selectedExternalLogOn);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        public string Note
        {
            get { return ContainedEntity.Note; }
            set { ContainedEntity.Note = value; }
        }

        /// <summary>
        /// Sets the person external log on collection.
        /// </summary>
        /// <param name="externalLogOnCollection">The external log on collection.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-08-15
        /// </remarks>
        public void SetPersonExternalLogOnCollection(IList<IExternalLogOn> externalLogOnCollection)
        {
            InParameter.NotNull("externalLogOnCollection", externalLogOnCollection);
            _externalLogOnParser = new ExternalLogOnParser(externalLogOnCollection,_externalLogOnDisplay);
        }

        /// <summary>
        /// Sets the site team adapter collection.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-08
        /// </remarks>
        public void SetSiteTeamAdapterCollection(IList<SiteTeamModel> siteTeamAdapterCollection)
        {
            InParameter.NotNull("siteTeamAdapterCollection", siteTeamAdapterCollection);
            _siteTeamAdapterCollection = siteTeamAdapterCollection;
        }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-18
        /// </remarks>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-18
        /// </remarks>
        public IPersonPeriod Period
        {
            get { return ContainedEntity; }
        }

        /// <summary>
        /// Gets or sets the site team.
        /// </summary>
        /// <value>The site team.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-08
        /// </remarks>
        public SiteTeamModel SiteTeam
        {
            get
            {
                List<SiteTeamModel> siteTeamAdapterCollection =
                                               _siteTeamAdapterCollection.Where(s => s.Team ==
                                               Period.Team && s.Site == Team.Site).ToList();

                if (!siteTeamAdapterCollection.IsEmpty())
                {
                    _siteTeamModel = siteTeamAdapterCollection.First();
                }

                return _siteTeamModel;
            }
            set
            {
                if (!ContainedEntity.Team.Equals(value.Team))
                {
                    ContainedEntity.Team = value.Team;
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
