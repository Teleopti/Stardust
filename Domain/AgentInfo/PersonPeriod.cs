using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{

	/// <summary>
	/// Represents the concrete circumstances of a working period for a person (agent).
	/// Defines where, in which team, under what conditions the person WILL, and also 
	/// in what skills the person CAN work.  
	/// </summary>
	/// <remarks>
	/// Created by: Dinesh Ranasinghe
	/// Created date: 2008-01-09
	/// </remarks>
	public class PersonPeriod : AggregateEntity, IPersonPeriod
	{
		private IPersonContract _personContract;
		private ITeam _team;
		private IList<IPersonSkill> _personSkillCollection;
		private readonly IList<IPersonSkill> _personMaxSeatSkillCollection  = new List<IPersonSkill>();
		private IList<IExternalLogOn> _externalLogOnCollection;
		private DateOnly _startDate;
		private IRuleSetBag _ruleSetBag;
		private string _note;
		private IBudgetGroup _budgetGroup;
        //TODO remove new and map when saving to DB
	    private readonly IList<IPersonSkill> _personNonBlendSkillCollection = new List<IPersonSkill>();

	    /// <summary>
		/// Default constructordate
		/// </summary>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-01-09
		/// </remarks>
		protected PersonPeriod()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PersonPeriod"/> class.
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="personContract"></param>
		/// <param name="team"></param>
		/// <remarks>
		/// Created by: cs
		/// Created date: 2008-03-04
		/// </remarks>
		public PersonPeriod(DateOnly startDate, IPersonContract personContract, ITeam team)
			: this()
		{
			InParameter.NotNull("personContract", personContract);
			InParameter.NotNull("team", team);

			_personContract = personContract;
			_team = team;
			_personSkillCollection = new List<IPersonSkill>();
			_personMaxSeatSkillCollection = new List<IPersonSkill>();
			_externalLogOnCollection = new List<IExternalLogOn>();
			_startDate = startDate;
		}

		/// <summary>
		/// StartDate
		/// </summary>
		/// /// <remarks>
		/// Created by: cs
		/// Created date: 2008-03-08
		/// </remarks>
		public virtual DateOnly StartDate
		{
			get { return _startDate; }
			set { _startDate = value; }
		}

		/// <summary>
		/// Gets the period enddate
		/// </summary>
		/// <value>The end date.</value>
		public virtual DateOnly EndDate()
		{
			var person = (IPerson)Parent;
			IPersonPeriod nextPeriod = person.NextPeriod(this);
			if (nextPeriod == null)
			{
				return person.TerminalDate.GetValueOrDefault(new DateOnly(2059, 12, 31));
			}
			return nextPeriod.StartDate.AddDays(-1);
		}

		/// <summary>
		/// Gets the period
		/// </summary>
		/// <remarks>
		/// Created by: cs
		/// Created date: 2008-03-06
		/// </remarks>
		public virtual DateOnlyPeriod Period
		{
			get
			{
				var returnValue = new DateOnlyPeriod(StartDate, EndDate());
				return (returnValue);
			}
		}

		/// <summary>
		/// Represent Person contract
		/// </summary>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-01-09
		/// </remarks>
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

		/// <summary>
		/// Represent Team
		/// </summary>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-01-09
		/// </remarks>
		public virtual ITeam Team
		{
			get { return _team; }
			set { _team = value; }
		}

		/// <summary>
		/// Gets or sets the person skill collection.
		/// </summary>
		/// <value>The person skill collection.</value>
		/// <remarks>
		/// Created by: sumeda herath
		/// Created date: 2008-01-09
		/// </remarks>
		public virtual IList<IPersonSkill> PersonSkillCollection
		{
			get
			{
				return new ReadOnlyCollection<IPersonSkill>(_personSkillCollection);
			}
		}

		/// <summary>
		/// Gets or sets the rule set bag.
		/// </summary>
		/// <value>The rule set bag.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-03-27
		/// </remarks>
		public virtual IRuleSetBag RuleSetBag
		{
			get { return _ruleSetBag; }
			set { _ruleSetBag = value; }
		}

		/// <summary>
		/// Gets or sets the note.
		/// </summary>
		/// <value>The note.</value>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-10-08
		/// </remarks>
		public virtual string Note
		{
			get { return _note; }
			set { _note = value; }
		}

		/// <summary>
		/// Gets the ACD login collection.
		/// </summary>
		/// <value>The ACD login collection.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-18
		/// </remarks>
		public virtual ReadOnlyCollection<IExternalLogOn> ExternalLogOnCollection
		{
			get { return new ReadOnlyCollection<IExternalLogOn>(_externalLogOnCollection); }
		}

		public virtual IBudgetGroup BudgetGroup
		{
			get { return _budgetGroup; }
			set { _budgetGroup = value; }
		}

		/// <summary>
		/// Adds the person skill.
		/// </summary>
		/// <param name="personSkill">The person skill.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: sumeda herath
		/// Created date: 2008-01-09
		/// </remarks>
		public virtual void AddPersonSkill(IPersonSkill personSkill)
		{
			InParameter.NotNull("personSkill", personSkill);

			if (_personSkillCollection.Any(p => p.Skill.Equals(personSkill.Skill))) return;

			personSkill.SetParent(this);
			_personSkillCollection.Add(personSkill);
		}

		/// <summary>
		/// Deletes the person skill.
		/// </summary>
		/// <param name="personSkill">The person skill.</param>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-04-02
		/// </remarks>
		public virtual void DeletePersonSkill(IPersonSkill personSkill)
		{
			InParameter.NotNull("personSkill", personSkill);

			_personSkillCollection.Remove(personSkill);
		}


		/// <summary>
		/// Resets the person skill.
		/// </summary>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-08-05
		/// </remarks>
		public virtual void ResetPersonSkill()
		{
			_personSkillCollection.Clear();
		}

		/// <summary>
		/// Adds the login.
		/// </summary>
		/// <param name="externalLogOn">The log on.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-06-18
		/// </remarks>
		public virtual void AddExternalLogOn(IExternalLogOn externalLogOn)
		{
			InParameter.NotNull("logOn", externalLogOn);

			if (!_externalLogOnCollection.Contains(externalLogOn))
				_externalLogOnCollection.Add(externalLogOn);
		}

		/// <summary>
		/// Removes the external log on.
		/// </summary>
		/// <param name="externalLogOn">The external log on.</param>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-08-18
		/// </remarks>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-08-18
		/// </remarks>
		public virtual void RemoveExternalLogOn(IExternalLogOn externalLogOn)
		{
			InParameter.NotNull("externalLogOn", externalLogOn);

			_externalLogOnCollection.Remove(externalLogOn);
		}

		public virtual IList<IPersonSkill> PersonMaxSeatSkillCollection
		{
			get { return _personMaxSeatSkillCollection; }
		}

		public virtual void AddPersonMaxSeatSkill(IPersonSkill personSkill)
		{
			if (personSkill == null)
				return;
			if(personSkill.Skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
				throw new ArgumentOutOfRangeException("personSkill", "The SkillType.ForecastSource of the Skill on the PersonSkill must be MaxSeatSkill");
			_personMaxSeatSkillCollection.Add(personSkill);
		}

	    public virtual IList<IPersonSkill> PersonNonBlendSkillCollection
	    {
            get { return _personNonBlendSkillCollection; }
	    }

	    public virtual void AddPersonNonBlendSkill(IPersonSkill personSkill)
	    {
            if (personSkill == null)
                return;
            if (personSkill.Skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill)
                throw new ArgumentOutOfRangeException("personSkill", "The SkillType.ForecastSource of the Skill on the PersonSkill must be NonBlendSkill");
            _personNonBlendSkillCollection.Add(personSkill);
	    }

	    /// <summary>
		/// Resets the external log on.
		/// </summary>
		/// <remarks>
		/// Created by: Muhamad Risath
		/// Created date: 2008-08-15
		/// </remarks>
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
			retobj._personSkillCollection = new List<IPersonSkill>();
			retobj._externalLogOnCollection = new List<IExternalLogOn>(_externalLogOnCollection);

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
			retobj._personSkillCollection = new List<IPersonSkill>();
			retobj._externalLogOnCollection = new List<IExternalLogOn>(_externalLogOnCollection);

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
