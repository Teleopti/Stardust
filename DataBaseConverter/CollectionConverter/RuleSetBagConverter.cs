using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{

    /// <summary>
    /// Converter
    /// </summary>
    public class RuleSetBagConverter : CccConverter<IRuleSetBag, FakeOldEntityRuleSetBag>
    {
        private readonly IRepository<IRuleSetBag> _rep;
        private readonly MappedObjectPair _mappedObjectPair;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        public RuleSetBagConverter(IUnitOfWork unitOfWork, Mapper<IRuleSetBag, FakeOldEntityRuleSetBag> mapper, MappedObjectPair mappedObjectPair)
            : base(unitOfWork, mapper)
        {
            _rep = new RuleSetBagRepository(unitOfWork);
            _mappedObjectPair = mappedObjectPair;
        }

        /// <summary>
        /// get the repository
        /// </summary>
        public override IRepository<IRuleSetBag> Repository
        {
            get { return _rep; }
        }

        /// <summary>
        /// Creates fake old entities
        /// </summary>
        /// <param name="shiftClasses"></param>
        /// <param name="workShiftRuleSetConverter"></param>
        /// <returns></returns>
        public Collection<FakeOldEntityRuleSetBag> CreateFakeOldEntities(ICollection<global::Domain.ShiftClass> shiftClasses, WorkShiftRuleSetConverter workShiftRuleSetConverter)
        {
            Dictionary<string, FakeOldEntityRuleSetBag> existDictionary =
                new Dictionary<string, FakeOldEntityRuleSetBag>();
            Collection<FakeOldEntityRuleSetBag> fakeOldEntities = new Collection<FakeOldEntityRuleSetBag>();

            foreach (global::Domain.ShiftClass shiftClass in shiftClasses)
            {
                if(shiftClass.Unit.AllUnits)
                {
                    foreach (ObjectPair<Unit,ISite> sitePair in _mappedObjectPair.Site)
                    {
                        if(!sitePair.Obj1.AllUnits)
                        {
                            addFakeOldEntity(existDictionary, fakeOldEntities, workShiftRuleSetConverter, shiftClass, sitePair.Obj1);
                        }
                    }
                }
            }

            foreach (global::Domain.ShiftClass shiftClass in shiftClasses)
            {
                if(!shiftClass.Unit.AllUnits)
                {
                    addFakeOldEntity(existDictionary, fakeOldEntities, workShiftRuleSetConverter, shiftClass, shiftClass.Unit);
                }
                
            }

            return fakeOldEntities;
        }

        private static void addFakeOldEntity(IDictionary<string, FakeOldEntityRuleSetBag> existDictionary, ICollection<FakeOldEntityRuleSetBag> fakeOldEntities, 
            WorkShiftRuleSetConverter workShiftRuleSetConverter, ShiftClass shiftClass, Unit unit)
        {
            FakeOldEntityRuleSetBag fakeEntity =
                new FakeOldEntityRuleSetBag(unit, shiftClass.EmploymentType);

            if (!existDictionary.ContainsKey(fakeEntity.Description))
            {
                existDictionary.Add(fakeEntity.Description, fakeEntity);
                fakeOldEntities.Add(fakeEntity);
            }
            else
                fakeEntity = existDictionary[fakeEntity.Description];

            fakeEntity.AddWorkShiftRuleSet(workShiftRuleSetConverter.GetNewValue(shiftClass));
        }
    }
}
