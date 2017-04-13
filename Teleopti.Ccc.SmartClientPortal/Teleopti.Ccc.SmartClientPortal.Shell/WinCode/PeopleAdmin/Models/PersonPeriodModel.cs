using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    public class PersonPeriodModel : IPersonPeriodModel
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public static IBudgetGroup NullBudgetGroup = new BudgetGroup { Name = String.Empty };

    	private IPersonPeriod _currentPeriod;
        private readonly IPerson _containedEntity;
        private bool _expandState;
        private readonly CommonNameDescriptionSetting _commonNameDescription;
        private readonly IList<IPersonSkill> _personSkillCollection;
        private readonly IList<SiteTeamModel> _siteTeamAdapterCollection;
        private readonly ExternalLogOnParser _externalLogOnParser;
    	private readonly PersonSkillStringParser _personSkillStringParser;

    	public PersonPeriodModel(DateOnly selectedDate, IPerson person, IList<IPersonSkill> personSkillCollection, IEnumerable<IExternalLogOn> externalLogOnCollection,
                                           IList<SiteTeamModel> siteTeamAdapterCollection,CommonNameDescriptionSetting commonNameDescription)
        {
            _containedEntity = person;
            _currentPeriod = _containedEntity.Period(selectedDate);
            _personSkillCollection = personSkillCollection;
            _siteTeamAdapterCollection = siteTeamAdapterCollection;
            _commonNameDescription = commonNameDescription;
            _externalLogOnParser = new ExternalLogOnParser(externalLogOnCollection, new ExternalLogOnDisplay());
        	_personSkillStringParser = new PersonSkillStringParser(_personSkillCollection);
        }

        /// <summary>
        /// Gets or sets the grid control.
        /// </summary>
        /// <value>The grid control.</value>
        public GridControl GridControl { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public IPerson Parent
        {
            get { return _containedEntity; }
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName
        {
            get
            {
	            return _commonNameDescription == null ? _containedEntity.Name.ToString() : _commonNameDescription.BuildCommonNameDescription(_containedEntity);
            }
        }

        /// <summary>
        /// Gets or sets the period date.
        /// </summary>
        /// <value>The period date.</value>
        public DateOnly? PeriodDate
        {
            get
            {
	            return _currentPeriod == null ? (DateOnly?) null : _currentPeriod.StartDate;
            }
	        set
            {
				if (_currentPeriod == null) return;
				if (!value.HasValue) return;

                if (value != _currentPeriod.StartDate)
                {
	                Parent.ChangePersonPeriodStartDate(value.Value, _currentPeriod);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current person contract.
        /// </summary>
        /// <value>The current person contract.</value>
        public IPersonContract PersonContract
        {
            get
            {
                if (_currentPeriod != null) return _currentPeriod.PersonContract;
                return null;
            }
            set
            {
                if (_currentPeriod != null) _currentPeriod.PersonContract = value;
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
                if (_currentPeriod != null && _currentPeriod.PersonContract != null && _currentPeriod.PersonContract.PartTimePercentage != null)
                {
                    return _currentPeriod.PersonContract.PartTimePercentage;
                }
                return null;
            }
            set
            {
                if (value != null && _currentPeriod != null && _currentPeriod.PersonContract != null && _currentPeriod.PersonContract.PartTimePercentage != null)
                {
                    _currentPeriod.PersonContract.PartTimePercentage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current rule set bag.
        /// </summary>
        /// <value>The current rule set bag.</value>
        public IRuleSetBag RuleSetBag
        {
            get
            {
                if (_currentPeriod != null)
                {
                    return _currentPeriod.RuleSetBag;
                }
                return null;
            }
            set
            {
                if (_currentPeriod != null)
                    _currentPeriod.RuleSetBag = value;
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
                if (_currentPeriod != null && _currentPeriod.PersonContract != null && _currentPeriod.PersonContract.Contract != null)
                {
                    return _currentPeriod.PersonContract.Contract;
                }
                return null;
            }
            set
            {
                if (value != null && _currentPeriod != null && _currentPeriod.PersonContract != null && _currentPeriod.PersonContract.Contract != null)
                {
                    _currentPeriod.PersonContract.Contract = value;
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
                if (_currentPeriod != null && _currentPeriod.PersonContract != null && _currentPeriod.PersonContract.ContractSchedule != null)
                {
                    return _currentPeriod.PersonContract.ContractSchedule;
                }
                return null;
            }
            set
            {
                if (value != null && _currentPeriod != null && _currentPeriod.PersonContract != null && _currentPeriod.PersonContract.ContractSchedule != null)
                {
                    _currentPeriod.PersonContract.ContractSchedule = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [expand state].
        /// </summary>
        /// <value><c>true</c> if [expand state]; otherwise, <c>false</c>.</value>
        public bool ExpandState
        {
            get { return _expandState; }
            set { _expandState = value; }
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
                if (_currentPeriod != null)
                {
                    Parent.ResetPersonSkills(_currentPeriod);
                    SetPersonSkills(value);
                }
            }
        }

        /// <summary>
        /// Sets the person skills.
        /// </summary>
        /// <param name="value">The value.</param>
        private void SetPersonSkills(string value)
        {
            if (_currentPeriod == null) return;
        	var personSkills = _personSkillStringParser.Parse(value);
        	foreach (var personSkill in personSkills)
        	{
        		Parent.AddSkill(personSkill,_currentPeriod);
        	}
        }

        /// <summary>
        /// Gets the person skills.
        /// </summary>
        /// <returns></returns>
        private string GetPersonSkills()
        {
            if (_currentPeriod != null)
            {
                StringBuilder personSkillString = new StringBuilder();


                if (_currentPeriod.PersonSkillCollection != null)
                {
                    IList<IPersonSkill> personSkillCollection = _currentPeriod.PersonSkillCollection.OrderBy(s => s.Skill.Name).ToList();

                    foreach (IPersonSkill personSkill in personSkillCollection)
                    {
                        if (((IDeleteTag)personSkill.Skill).IsDeleted == false)
                        {
                            if (!String.IsNullOrEmpty(personSkillString.ToString()))
                                personSkillString.Append(", " + personSkill.Skill.Name);
                            else 
                                personSkillString.Append(personSkill.Skill.Name);
                        }
                    }
                }
                return personSkillString.ToString();
            }

            return String.Empty;
        }

        /// <summary>
        /// Gets the current person period by date.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        /// <returns></returns>
        public IPersonPeriod GetCurrentPersonPeriodByDate(DateOnly selectedDate)
        {
            _currentPeriod = _containedEntity.Period(selectedDate);
            return _currentPeriod;
        }

        /// <summary>
        /// Gets a value indicating whether this instance can gray.
        /// </summary>
        /// <value><c>true</c> if this instance can gray; otherwise, <c>false</c>.</value>
        public bool CanGray
        {
            get
            {
                return _currentPeriod == null;
            }
        }

        public IPersonPeriod Period
        {
            get { return _currentPeriod; }
        }

        /// <summary>
        /// Gets the period count.
        /// </summary>
        /// <value>The period count.</value>
        public int PeriodCount
        {
            get
            {
                if (_currentPeriod != null && Parent.PersonPeriodCollection.Count() == 1)
                    return 0;
                return Parent.PersonPeriodCollection.Count();
            }
        }

        /// <summary>
        /// Gets or sets the person external log on names.
        /// </summary>
        /// <value>The person external log on names.</value>
        public string ExternalLogOnNames
        {
            get
            {
                if (_currentPeriod == null) return String.Empty;

                return _externalLogOnParser.GetExternalLogOnsDisplayText(_currentPeriod.ExternalLogOnCollection);
            }
            set
            {
                if (_currentPeriod != null)
                {
						 Parent.ResetExternalLogOn(_currentPeriod);
                    var selectedExternalLogOns = _externalLogOnParser.ParsePersonExternalLogOn(value);
                    foreach (var selectedExternalLogOn in selectedExternalLogOns)
                    {
							  Parent.AddExternalLogOn(selectedExternalLogOn, _currentPeriod);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the site team adapter.
        /// </summary>
        /// <value>The site team adapter.</value>
        public SiteTeamModel SiteTeam
        {
            get
            {
                if (_currentPeriod != null)
                {
	                return
		                _siteTeamAdapterCollection.FirstOrDefault(
			                s => s.Team == _currentPeriod.Team && s.Site == _currentPeriod.Team.Site);
                }
                return null;
            }
            set
            {
				if (value != null && _currentPeriod != null)
				{
					Parent.ChangeTeam(value.Team, _currentPeriod);
				}
            }
        }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        public String Note
        {
            get
            {
                if (_currentPeriod != null)
                {
                    return _currentPeriod.Note;
                }
                return String.Empty;
            }
            set
            {
                if (_currentPeriod != null)
                {
                    _currentPeriod.Note = value;
                }
            }
        }

        /// <summary>
        /// Resets the can bold property of child adapters.
        /// </summary>
        public void ResetCanBoldPropertyOfChildAdapters()
        {
            if (GridControl != null)
            {
                var childAdapters = GridControl.Tag as IList<PersonPeriodChildModel>;
                if (childAdapters != null)
                {
                    for (int i = 0; i < childAdapters.Count; i++)
                    {
                        childAdapters[i].CanBold = false;
                    }
                }

                GridControl.Invalidate();
            }
        }

		public bool AdapterOrChildCanBold()
		{
			if (CanBold) return true;

			if (GridControl != null)
			{
				var childAdapters = GridControl.Tag as IList<PersonPeriodChildModel>;

				if (childAdapters != null)
				{
					for (var i = 0; i < childAdapters.Count; i++)
					{
						if (childAdapters[i].CanBold) return true;
					}
				}
			}

			return false;
		}

		public bool CanBold { get; set; }

    	public IBudgetGroup BudgetGroup
    	{
			get
			{
				return _currentPeriod != null ? _currentPeriod.BudgetGroup : NullBudgetGroup;
			}
    		set
			{
				if (_currentPeriod == null) return;
				if (value==NullBudgetGroup)
				{
					value = null;
				}
				_currentPeriod.BudgetGroup = value;
			}
    	}
    }
}
