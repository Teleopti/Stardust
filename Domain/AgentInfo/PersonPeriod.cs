using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class PersonPeriod : AggregateEntity, IPersonPeriod, IPersonPeriodModifySkills, IPersonPeriodModifyExternalLogon
	{
		private IPersonContract _personContract;
		private ITeam _team;
		private ISet<IPersonSkill> _personSkillCollection;
		private MaxSeatSkill _maxSeatSkill;
		private ISet<IExternalLogOn> _externalLogOnCollection;
		private DateOnly _startDate;
		private IRuleSetBag _ruleSetBag;
		private string _note;
		private IBudgetGroup _budgetGroup;
        //TODO remove new and map when saving to DB
	    private readonly IList<IPersonSkill> _personNonBlendSkillCollection = new List<IPersonSkill>();

		protected PersonPeriod()
		{
		}

		public PersonPeriod(DateOnly startDate, IPersonContract personContract, ITeam team)
			: this()
		{
			InParameter.NotNull(nameof(personContract), personContract);
			InParameter.NotNull(nameof(team), team);

			_personContract = personContract;
			_team = team;
			_personSkillCollection = new HashSet<IPersonSkill>();
			_externalLogOnCollection = new HashSet<IExternalLogOn>();
			_startDate = startDate >= new DateOnly(2059, 12, 31) ? new DateOnly(2059, 12, 30) : startDate;
		}

		public virtual DateOnly StartDate
		{
			get { return _startDate; }
			set {
				_startDate = value >= new DateOnly(2059, 12, 31) ? new DateOnly(2059, 12, 30) : value;
			}
		}

		public virtual DateOnly EndDate()
		{
			var person = (IPerson)Parent;
			IPersonPeriod nextPeriod = person.NextPeriod(this);
			if (nextPeriod == null)
			{
				return person.TerminalDate.GetValueOrDefault(Person.DefaultTerminalDate);
			}
			return nextPeriod.StartDate.AddDays(-1);
		}

		public virtual DateOnly internalEndDate
		{
			get { return EndDate(); }
			set { }
		}

		public virtual DateOnlyPeriod Period => new DateOnlyPeriod(StartDate, EndDate());

		public virtual IPersonContract PersonContract
		{
			get
			{
				return _personContract;
			}
			set
			{
				_personContract = value;
			}
		}

		public virtual ITeam Team
		{
			get { return _team; }
			set { _team = value; }
		}

		public virtual IEnumerable<IPersonSkill> PersonSkillCollection => _personSkillCollection.ToArray();

		public virtual IEnumerable<IPersonSkill> CascadingSkills()
		{
			return _personSkillCollection.Where(x => x.Active && !((IDeleteTag) x.Skill).IsDeleted && x.Skill.IsCascading()).OrderBy(x => x.Skill.CascadingIndex);
		}

		public virtual IRuleSetBag RuleSetBag
		{
			get { return _ruleSetBag; }
			set { _ruleSetBag = value; }
		}

		public virtual string Note
		{
			get { return _note; }
			set { _note = value; }
		}

		public virtual IEnumerable<IExternalLogOn> ExternalLogOnCollection => _externalLogOnCollection.ToArray();

		public virtual IBudgetGroup BudgetGroup
		{
			get { return _budgetGroup; }
			set { _budgetGroup = value; }
		}

		public virtual void AddPersonSkill(IPersonSkill personSkill)
		{
			InParameter.NotNull(nameof(personSkill), personSkill);

			if (_personSkillCollection.Any(p => p.Skill.Equals(personSkill.Skill)))
			{
				throw new ArgumentException("There is already an instance of the skill connected to this person period.",nameof(personSkill));
			}

			personSkill.SetParent(this);
			_personSkillCollection.Add(personSkill);
		}

		public virtual void DeletePersonSkill(IPersonSkill personSkill)
		{
			InParameter.NotNull(nameof(personSkill), personSkill);

			_personSkillCollection.Remove(personSkill);
		}

		public virtual void ResetPersonSkill()
		{
			_personSkillCollection.Clear();
		}

		public virtual void AddExternalLogOn(IExternalLogOn externalLogOn)
		{
			InParameter.NotNull(nameof(externalLogOn), externalLogOn);
			_externalLogOnCollection.Add(externalLogOn);
		}

		public virtual void RemoveExternalLogOn(IExternalLogOn externalLogOn)
		{
			InParameter.NotNull(nameof(externalLogOn), externalLogOn);

			_externalLogOnCollection.Remove(externalLogOn);
		}

		public virtual MaxSeatSkill MaxSeatSkill => _maxSeatSkill;

		public virtual void SetMaxSeatSkill(MaxSeatSkill maxSeatSkill)
		{
			_maxSeatSkill = maxSeatSkill;
		}

	    public virtual IList<IPersonSkill> PersonNonBlendSkillCollection => _personNonBlendSkillCollection;

		public virtual void AddPersonNonBlendSkill(IPersonSkill personSkill)
	    {
            if (personSkill == null)
                return;
            if (personSkill.Skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill)
                throw new ArgumentOutOfRangeException(nameof(personSkill), "The SkillType.ForecastSource of the Skill on the PersonSkill must be NonBlendSkill");
            _personNonBlendSkillCollection.Add(personSkill);
	    }

		public virtual void ResetExternalLogOn()
		{
			_externalLogOnCollection.Clear();
		}

		#region ICloneable Members

		///<summary>
		///Creates a new object that is a copy of the current instance.
		///</summary>
		///
		///<returns>
		///A new object that is a copy of this instance.
		///</returns>
		///<filterpriority>2</filterpriority>
		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		#endregion

		#region Implementation of ICloneableEntity<IPersonPeriod>

		public virtual IPersonPeriod NoneEntityClone()
		{
			var retobj = (PersonPeriod)MemberwiseClone();
			retobj.SetId(null);
			retobj._personSkillCollection = new HashSet<IPersonSkill>();
			retobj._externalLogOnCollection = new HashSet<IExternalLogOn>(_externalLogOnCollection);

			foreach (IPersonSkill personSkill in _personSkillCollection)
			{
				IPersonSkill personSkillClone = personSkill.NoneEntityClone();
				personSkillClone.SetParent(retobj);
				retobj._personSkillCollection.Add(personSkillClone);
			}

			retobj._personContract = (IPersonContract)_personContract.Clone();

			return retobj;
		}

		public virtual IPersonPeriod EntityClone()
		{
			var retobj = (PersonPeriod)MemberwiseClone();
			retobj._personSkillCollection = new HashSet<IPersonSkill>();
			retobj._externalLogOnCollection = new HashSet<IExternalLogOn>(_externalLogOnCollection);

			foreach (IPersonSkill personSkill in _personSkillCollection)
			{
				IPersonSkill personSkillClone = personSkill.EntityClone();
				personSkillClone.SetParent(retobj);
				retobj._personSkillCollection.Add(personSkillClone);
			}

			retobj._personContract = (IPersonContract)_personContract.Clone();

			return retobj;
		}

		#endregion
	}
}
