using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{

    /// <summary>
    /// ServiceAgreement mapper
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-02
    /// </remarks>
    public class SkillDataMapper : Mapper<ServiceAgreement,global::Domain.SkillData>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDataMapper"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public SkillDataMapper()
            : base(new MappedObjectPair(), null)
        {
        }


        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public override ServiceAgreement Map(global::Domain.SkillData oldEntity)
        {
            ServiceAgreement newServiceAgreement = new ServiceAgreement();
            
            if (oldEntity.ForecastedServicePercent() == 0)
                newServiceAgreement.ServiceLevel = new ServiceLevel(new Percent(1),
                                            (double)oldEntity.ForecastedServiceLevel());
            else
                newServiceAgreement.ServiceLevel= new ServiceLevel(new Percent((double)oldEntity.ForecastedServicePercent() / 100d), 
                                            (double)oldEntity.ForecastedServiceLevel());

            newServiceAgreement.MinOccupancy = new Percent((double)oldEntity.MinOCC / 100d);
            if (oldEntity.MaxOCC == 0)
            {
                newServiceAgreement.MaxOccupancy = new Percent(1.0d);
            }
            else
            {
                newServiceAgreement.MaxOccupancy = new Percent((double)oldEntity.MaxOCC / 100d);
            }

            return newServiceAgreement;
        }
    }
}