using Infrastructure;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Infrastructure;
using Skill=Domain.Skill;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter2
{
    /// <summary>
    /// Converts a skill
    /// </summary>
    public class SkillConverter
    {
        /// <summary>
        /// Converts
        /// </summary>
        /// <param name="unitOfWork">The uow.</param>
        /// <param name="converter">The converter.</param>
        /// <param name="oldReader">The old reader.</param>
        /// <param name="skillRep">The skill rep.</param>
        /// <returns></returns>
        public ObjectPairCollection<Skill, Domain.Forecasting.Skill> Convert(IUnitOfWork unitOfWork,
                                                                             Mapper<Domain.Forecasting.Skill, Skill> converter,
                                                                             ICacheBase<Skill> oldReader,
                                                                             IRepository<Domain.Forecasting.Skill> skillRep)
        {
            ObjectPairCollection<Skill, Domain.Forecasting.Skill> retList =
                new ObjectPairCollection<Skill, Domain.Forecasting.Skill>();
            foreach (Skill theOld in oldReader.GetAll().Values)
            {
                Domain.Forecasting.Skill theNew = converter.Map(theOld);
                if (theNew != null)
                {
                    skillRep.Add(theNew);
                    retList.Add(theOld, theNew);
                }
            }
            unitOfWork.PersistAll();
            return retList;
        }
    }
}