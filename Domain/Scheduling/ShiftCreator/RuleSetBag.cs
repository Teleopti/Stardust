using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// A bag of workshiftrulesets
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-27
    /// </remarks>
    public class RuleSetBag : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IRuleSetBag, IDeleteTag
    {

		#region Fields (2) 

        private Description _description;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;
        private bool _isDeleted;

        #endregion Fields 

		#region Constructors (1) 

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleSetBag"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        public RuleSetBag()
        {
            _description = new Description();
            _ruleSetCollection = new List<IWorkShiftRuleSet>();
        }

	    public RuleSetBag(params IWorkShiftRuleSet[] workShiftRuleSets) : this()
	    {
		    foreach (var workShiftRuleSet in workShiftRuleSets)
		    {
			    AddRuleSet(workShiftRuleSet);
			}
	    }

		#endregion Constructors 

		#region Properties (2) 

	    /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual Description Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets the rule set collection.
        /// </summary>
        /// <value>The rule set collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        public virtual ReadOnlyCollection<IWorkShiftRuleSet> RuleSetCollection
        {
            get { return new ReadOnlyCollection<IWorkShiftRuleSet>(RuleSetCollectionWritable); }
        }

        /// <summary>
        /// Gets the internal modifiable rule set collection.
        /// </summary>
        /// <value>The rule set collection internal.</value>
        public virtual IList<IWorkShiftRuleSet> RuleSetCollectionWritable
        {
            get
            {
                return _ruleSetCollection;
            }
        }

		public virtual IWorkTimeMinMax MinMaxWorkTime(IWorkShiftWorkTime workShiftWorkTime, DateOnly onDate, IWorkTimeMinMaxRestriction restriction)
        {
            if (restriction == null) return null;
			if (!restriction.MayMatchWithShifts()) return null;

		  	var validRuleSets = _ruleSetCollection.Where(workShiftRuleSet => workShiftRuleSet.IsValidDate(onDate)).ToArray();
            
            var nonRestrictionSets = validRuleSets.Where(workShiftRuleSet => !workShiftRuleSet.OnlyForRestrictions).ToList();
				var retVal = worktimeForRuleSetsAndRestriction(restriction, nonRestrictionSets, workShiftWorkTime);

            if(retVal == null && restriction.MayMatchBlacklistedShifts())
            {
                var restrictionSets = validRuleSets.Where(workShiftRuleSet => workShiftRuleSet.OnlyForRestrictions).ToList();
					 retVal = worktimeForRuleSetsAndRestriction(restriction, restrictionSets, workShiftWorkTime);
            }

            return retVal;
        }

	    private static IWorkTimeMinMax worktimeForRuleSetsAndRestriction(IWorkTimeMinMaxRestriction restriction,
	                                                                     IEnumerable<IWorkShiftRuleSet> validRuleSets,
	                                                                     IWorkShiftWorkTime workShiftWorkTime)
	    {
		    IWorkTimeMinMax retVal = null;
		    foreach (var workShiftRuleSet in validRuleSets)
		    {
			    if (!restriction.Match(workShiftRuleSet.TemplateGenerator.Category))
				    continue;

			    var ruleSetWorkTimeMinMax = workShiftWorkTime.CalculateMinMax(workShiftRuleSet, restriction);
			    if (ruleSetWorkTimeMinMax != null)
			    {
				    if (retVal == null) retVal = new WorkTimeMinMax();

				    retVal = retVal.Combine(ruleSetWorkTimeMinMax);
			    }
		    }
		    return retVal;
	    }

	    #endregion Properties 

        public virtual void AddRuleSet(IWorkShiftRuleSet workShiftRuleSet)
        {
            InParameter.NotNull(nameof(workShiftRuleSet), workShiftRuleSet);
            WorkShiftRuleSet concrete = workShiftRuleSet as WorkShiftRuleSet;
	        concrete?.RuleSetBagCollectionWritable.Add(this);
	        _ruleSetCollection.Add(workShiftRuleSet);
        }

        public virtual void RemoveRuleSet(IWorkShiftRuleSet workShiftRuleSet)
        {
            WorkShiftRuleSet concrete = workShiftRuleSet as WorkShiftRuleSet;
	        concrete?.RuleSetBagCollectionWritable.Remove(this);
	        _ruleSetCollection.Remove(workShiftRuleSet);
        }

        public virtual void ClearRuleSetCollection()
        {
            for (int i = _ruleSetCollection.Count - 1; i >= 0; i--)
            {
                RemoveRuleSet(_ruleSetCollection[i]);
            }
        }

        public virtual IList<IShiftCategory> ShiftCategoriesInBag()
        {
            IList<IShiftCategory> categories = new List<IShiftCategory>();
            foreach (IWorkShiftRuleSet workShiftRuleSet in _ruleSetCollection)
            {
                if (!categories.Contains(workShiftRuleSet.TemplateGenerator.Category))
                    categories.Add(workShiftRuleSet.TemplateGenerator.Category);
            }
            return categories;
        }

        public virtual bool IsChoosable => !IsDeleted;

	    public virtual bool IsDeleted => _isDeleted;

	    #region ICloneableEntity<RuleSetBag> Members (2) 

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-07-07
        /// </remarks>
        public virtual IRuleSetBag NoneEntityClone()
        {
            RuleSetBag retObj = (RuleSetBag)MemberwiseClone();
            //retObj.Description = new Description(Description.Name + " - copy", Description.ShortName);
            retObj.SetId(null);

            retObj._ruleSetCollection = new List<IWorkShiftRuleSet>();
            foreach (IWorkShiftRuleSet ruleSet in _ruleSetCollection)
            {
                retObj.AddRuleSet(ruleSet);
            }
            return retObj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 2008-07-07
        /// </remarks>
        public virtual IRuleSetBag EntityClone()
        {
            RuleSetBag retObj = (RuleSetBag)MemberwiseClone();
            retObj._ruleSetCollection = new List<IWorkShiftRuleSet>();
            foreach (IWorkShiftRuleSet ruleSet in _ruleSetCollection)
            {
                retObj.AddRuleSet(ruleSet); // Jus link without cloning.
            }
            return retObj;
        }


        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-09-10
        /// </remarks>
        public virtual object Clone()
        {
            return EntityClone();
        }

        #endregion

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}
