using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Converter
    /// </summary>
    public class WorkShiftRuleSetConverter : CccConverter<IWorkShiftRuleSet, global::Domain.ShiftClass>
    {
        ObjectPairCollection<global::Domain.ShiftClass, IWorkShiftRuleSet> _pairCollection;
        private readonly IRepository<IWorkShiftRuleSet> _rep;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="mapper"></param>
        public WorkShiftRuleSetConverter(IUnitOfWork unitOfWork, Mapper<IWorkShiftRuleSet, global::Domain.ShiftClass> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new WorkShiftRuleSetRepository(unitOfWork);    
        }

        /// <summary>
        /// get the repository
        /// </summary>
        public override IRepository<IWorkShiftRuleSet> Repository
        {
            get { return _rep; }
        }

        /// <summary>
        /// add to pair collection
        /// </summary>
        /// <param name="pairCollection"></param>
        protected override void OnPersisted(ObjectPairCollection<global::Domain.ShiftClass, IWorkShiftRuleSet> pairCollection)
        {
            _pairCollection = pairCollection;
        }

        /// <summary>
        /// gets the converted workshiftruleset from corresponding old shiftclass
        /// </summary>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        public IWorkShiftRuleSet GetNewValue(global::Domain.ShiftClass oldValue)
        {
            if (_pairCollection == null)
                return null;

            return _pairCollection.GetPaired(oldValue);
        }
    }
}
