using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts
{
    public class ShiftsDataHelper : IDataHelper
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly IList<IWorkShiftRuleSet> _ruleSetsToSaveOrUpdate = new List<IWorkShiftRuleSet>();
        private readonly IList<IRuleSetBag> _ruleBagsToSaveOrUpdate = new List<IRuleSetBag>();
        private readonly IList<IWorkShiftRuleSet> _ruleSetsToDelete = new List<IWorkShiftRuleSet>();
        private readonly IList<IRuleSetBag> _ruleBagsToDelete = new List<IRuleSetBag>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public ShiftsDataHelper(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IEventAggregator eventAggregator)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<RuleSetChanged>().Subscribe(ruleSetsChanged);
            _eventAggregator.GetEvent<RuleSetBagChanged>().Subscribe(ruleSetBagChanged);
        }

        private void ruleSetBagChanged(IRuleSetBag bag)
        {
            if(bag == null) return;
            save(bag);
        }

        private void ruleSetsChanged(IList<IWorkShiftRuleSet> ruleSets)
        {
            if(ruleSets == null) return;
            foreach (var workShiftRuleSet in ruleSets)
            {
                Save(workShiftRuleSet);
            }
        }

        public void Save(IWorkShiftRuleSet ruleSet)
        {
            if(!_ruleSetsToSaveOrUpdate.Contains(ruleSet))
                _ruleSetsToSaveOrUpdate.Add(ruleSet);
        }

        private void save(IRuleSetBag ruleSetBag)
        {
            if (!_ruleBagsToSaveOrUpdate.Contains(ruleSetBag))
                _ruleBagsToSaveOrUpdate.Add(ruleSetBag);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void PersistAll()
        {
            
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var ruleSetRepository = _repositoryFactory.CreateWorkShiftRuleSetRepository(uow);

                foreach (IWorkShiftRuleSet ruleSet in _ruleSetsToSaveOrUpdate)
                {
                        ruleSetRepository.Add(ruleSet);
                }
                foreach (var workShiftRuleSet in _ruleSetsToDelete)
                {
                    ruleSetRepository.Remove(workShiftRuleSet);
                }

                var ruleBagRepository = _repositoryFactory.CreateRuleSetBagRepository(uow);
                foreach (IRuleSetBag ruleBag in _ruleBagsToSaveOrUpdate)
                {
                     ruleBagRepository.Add(ruleBag);
                }
                foreach (var ruleSetBag in _ruleBagsToDelete)
                {
					//hack to persist removed collection items
                	ruleBagRepository.Add(ruleSetBag);
                     ruleBagRepository.Remove(ruleSetBag);
                }
                uow.PersistAll();

                _ruleBagsToSaveOrUpdate.Clear();
                _ruleSetsToSaveOrUpdate.Clear();
                _ruleSetsToDelete.Clear();
                _ruleBagsToDelete.Clear();
            }
            
        }

        public void Delete(IWorkShiftRuleSet ruleSet)
        {
            for (int i = ruleSet.RuleSetBagCollection.Count; i > 0; i--)
            {
				if (!_ruleBagsToSaveOrUpdate.Contains(ruleSet.RuleSetBagCollection[i - 1]))
					_ruleBagsToSaveOrUpdate.Add(ruleSet.RuleSetBagCollection[i - 1]);
            	ruleSet.RuleSetBagCollection[i-1].RemoveRuleSet(ruleSet);
            }
            _ruleSetsToDelete.Add(ruleSet);
        }

        public void Delete(IRuleSetBag bag)
        {
            var ruleSets = bag.RuleSetCollection;
            for (int index = 0; index < ruleSets.Count; index++)
            {
                bag.RemoveRuleSet(ruleSets[index]);
            }
            _ruleBagsToDelete.Add(bag);
           
        }

        public IList<IWorkShiftRuleSet> FindRuleSets(IUnitOfWork uow)
        {
            var ruleSets = _repositoryFactory.CreateWorkShiftRuleSetRepository(uow).FindAllWithLimitersAndExtenders();
            return ruleSets.OrderBy(r => r.Description.Name).ToList();
        }

        public IList<IRuleSetBag> FindRuleSetBags(IUnitOfWork uow)
        {
            var ruleSetBags = _repositoryFactory.CreateRuleSetBagRepository(uow).LoadAllWithRuleSets();
            return ruleSetBags.OrderBy(r => r.Description.Name).ToList();  
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public TypedBindingCollection<IActivity> FindAllActivities(IUnitOfWork uow)
        {
            var returnCollection = new TypedBindingCollection<IActivity>();

			using (uow.DisableFilter(QueryFilter.Deleted))
			{
				IList<IActivity> activities = _repositoryFactory.CreateActivityRepository(uow).LoadAllSortByName();
				var masterActivities = _repositoryFactory.CreateMasterActivityRepository(uow).LoadAll();
				var sortedMasters = from masterActivity in masterActivities
				                    where masterActivity.IsDeleted == false
				                    orderby masterActivity.Description.Name
				                    select masterActivity;

				foreach (var activity in activities.Where(activity => activity.IsDeleted == false))
				{
					returnCollection.Add(activity);
				}

				foreach (var masterActivity in sortedMasters)
				{
					returnCollection.Add(masterActivity);
				}
			}


        	return returnCollection;
        }

        public TypedBindingCollection<IShiftCategory> FindAllCategories(IUnitOfWork uow)
        {
            var returnCollection = new TypedBindingCollection<IShiftCategory>();

            IList<IShiftCategory> categories = _repositoryFactory.CreateShiftCategoryRepository(uow).FindAll();
            foreach (IShiftCategory category in categories.OrderBy(c => c.Description.Name))
                returnCollection.Add(category);
            
            return returnCollection;
        }

        public ReadOnlyCollection<string> FindAllOperatorLimits()
        {
            IList<KeyValuePair<OperatorLimiter, string>> operatorLimitCollection = LanguageResourceHelper.TranslateEnumToList<OperatorLimiter>();
            List<string> defaultAccessibilityNames = (from p in operatorLimitCollection select p.Value).ToList();
            return new ReadOnlyCollection<string>(defaultAccessibilityNames);
        }

        public ReadOnlyCollection<string> FindAllAccessibilities()
        {
            IList<KeyValuePair<DefaultAccessibility, string>> defaultAccessibilityList = LanguageResourceHelper.TranslateEnumToList<DefaultAccessibility>();
            string[] defaultAccessibilityNames = (from p in defaultAccessibilityList select p.Value).ToArray();
            return new ReadOnlyCollection<string>(defaultAccessibilityNames);
        }

        public IWorkShiftRuleSet CreateDefaultRuleSet(IActivity defaultActivity,
                                                      IShiftCategory category,
                                                      TimePeriod defaultStartPeriod,
                                                      TimeSpan defaultStartPeriodSegment,
                                                      TimePeriod defaultEndPeriod,
                                                      TimeSpan defaultEndPeriodSegment)
        {
            var defaultStart = new TimePeriodWithSegment(defaultStartPeriod, defaultStartPeriodSegment);
            var defaultEnd = new TimePeriodWithSegment(defaultEndPeriod, defaultEndPeriodSegment);
            IShiftCategory defaultCategory = category;

            IWorkShiftTemplateGenerator newTemplate = new WorkShiftTemplateGenerator(defaultActivity, defaultStart, defaultEnd, defaultCategory);
            IWorkShiftRuleSet ruleSet = new WorkShiftRuleSet(newTemplate)
                                            {
                                                Description = new Description(Resources.LessThanRuleSetNameGreaterThan)
                                            };
            Save(ruleSet);
            return ruleSet;
        }

        public IRuleSetBag CreateDefaultRuleSetBag()
        {
            IRuleSetBag bag = new RuleSetBag
                                  {
                                      Description = new Description(Resources.DefaultRuleSetBagName)
                                  };
            save(bag);
            return bag;
        }


        public ActivityTimeLimiter CreateDefaultActivityTimeLimiter(IWorkShiftRuleSet ruleSet, TimeSpan timeLimit)
        {
            var limiter = new ActivityTimeLimiter(ruleSet.TemplateGenerator.BaseActivity, timeLimit, OperatorLimiter.Equals);
            ruleSet.AddLimiter(limiter);
            Save(ruleSet);
            return limiter;
        }

        public int DefaultSegment()
        {
            int defaultSegment;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                defaultSegment =_repositoryFactory.CreateGlobalSettingDataRepository(uow).FindValueByKey("DefaultSegment", new DefaultSegment()).SegmentLength;
            }

            return defaultSegment;

        }

        public bool HasUnsavedData()
        {
            //return unitOfWork.IsDirty();
            return (_ruleBagsToDelete.Count + _ruleBagsToSaveOrUpdate.Count + _ruleSetsToDelete.Count
                    + _ruleSetsToSaveOrUpdate.Count) > 0;
        }

        
    }
}
