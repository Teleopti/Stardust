using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    public class PersonPeriodModel : IPersonPeriodModel
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public static IBudgetGroup NullBudgetGroup = new BudgetGroup { Name = String.Empty };

    	private IPersonPeriod _currentPeriod;
        private readonly IPerson _containedEntity;
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

        public GridControl GridControl { get; set; }

        public IPerson Parent => _containedEntity;

        public string FullName => _commonNameDescription == null ? _containedEntity.Name.ToString() : _commonNameDescription.BuildFor(_containedEntity);

        public DateOnly? PeriodDate
        {
            get => _currentPeriod?.StartDate;
			set
            {
				if (_currentPeriod == null) return;
				if (!value.HasValue) return;

                if (value != _currentPeriod.StartDate)
                {
	                Parent.ChangePersonPeriodStartDate(value.Value, _currentPeriod);
					addPersonEmploymentChangedEvent(true);
                }
            }
        }

        public IPersonContract PersonContract
        {
            get => _currentPeriod?.PersonContract;
			set
            {
                if (_currentPeriod != null) _currentPeriod.PersonContract = value;
            }
        }

        public IPartTimePercentage PartTimePercentage
        {
            get => _currentPeriod?.PersonContract?.PartTimePercentage;
			set
            {
                if (value != null && _currentPeriod?.PersonContract?.PartTimePercentage != null)
                {
                    _currentPeriod.PersonContract.PartTimePercentage = value;
					addPersonEmploymentChangedEvent();
                }
            }
        }

        public IRuleSetBag RuleSetBag
        {
            get => _currentPeriod?.RuleSetBag;
			set
            {
                if (_currentPeriod != null)
                    _currentPeriod.RuleSetBag = value;
            }
        }

        public IContract Contract
        {
            get => _currentPeriod?.PersonContract?.Contract;
			set
            {
                if (value != null && _currentPeriod?.PersonContract?.Contract != null)
                {
                    _currentPeriod.PersonContract.Contract = value;
					addPersonEmploymentChangedEvent();
				}
            }
        }

		private void addPersonEmploymentChangedEvent(bool getFromPreviousPeriod = false)
		{
			var startDate = _currentPeriod.StartDate;
			if (getFromPreviousPeriod)
			{
				var prev =_containedEntity.PreviousPeriod(_currentPeriod);
				if (prev != null)
					startDate = prev.StartDate;
			}
			Parent.AddPersonEmploymentChangeEvent(new PersonEmploymentChangedEvent
			{
				PersonId = _containedEntity.Id.GetValueOrDefault(),
				FromDate = startDate
			});
		}

        public IContractSchedule ContractSchedule
        {
            get => _currentPeriod?.PersonContract?.ContractSchedule;
			set
            {
                if (value != null && _currentPeriod?.PersonContract?.ContractSchedule != null)
                {
                    _currentPeriod.PersonContract.ContractSchedule = value;
					addPersonEmploymentChangedEvent();
				}
            }
        }

        public bool ExpandState { get; set; }

        public string PersonSkills
        {
            get => getPersonSkills();
			set
            {
                if (_currentPeriod != null)
                {
                    Parent.ResetPersonSkills(_currentPeriod);
                    SetPersonSkills(value);
                }
            }
        }

        private void SetPersonSkills(string value)
        {
            if (_currentPeriod == null) return;
        	var personSkills = _personSkillStringParser.Parse(value);
        	foreach (var personSkill in personSkills)
        	{
        		Parent.AddSkill(personSkill,_currentPeriod);
        	}
        }

        private string getPersonSkills()
        {
			if (_currentPeriod?.PersonSkillCollection != null)
			{
				return string.Join(", ",
					_currentPeriod.PersonSkillCollection.Where(s => !((IDeleteTag) s.Skill).IsDeleted).Select(s => s.Skill.Name).OrderBy(s => s));
			}

			return String.Empty;
        }

        public bool CanGray => _currentPeriod == null;

		public IPersonPeriod Period => _currentPeriod;

        public int PeriodCount
        {
            get
            {
                if (_currentPeriod != null && Parent.PersonPeriodCollection.Count == 1)
                    return 0;
                return Parent.PersonPeriodCollection.Count;
            }
        }

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
			get => _currentPeriod != null ? _currentPeriod.BudgetGroup : NullBudgetGroup;
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
