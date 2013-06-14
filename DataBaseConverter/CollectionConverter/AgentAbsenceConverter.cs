using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using System.Collections.Generic;
using System;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter
{
    /// <summary>
    /// Person absence converter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 10/31/2007
    /// </remarks>
    public class AgentAbsenceConverter : CccConverter<IPersonAbsence, global::Domain.AgentDay>
    {
        private readonly IRepository<IPersonAbsence> _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentAbsenceConverter"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="mapper">The mapper.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/31/2007
        /// </remarks>
        public AgentAbsenceConverter(IUnitOfWork unitOfWork, Mapper<IPersonAbsence, global::Domain.AgentDay> mapper)
            : base(unitOfWork, mapper)
        {
            _rep = new PersonAbsenceRepository(unitOfWork);
        }

        /// <summary>
        /// Gets the repository.
        /// </summary>
        /// <value>The repository.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/29/2007
        /// </remarks>
        public override IRepository<IPersonAbsence> Repository
        {
            get { return _rep; }
        }

        /// <summary>
        /// Convert and persist
        /// </summary>
        /// <param name="entitiesToConvert"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void ConvertAndPersist(IDictionary<global::Domain.Agent, IList<global::Domain.AgentDay>> entitiesToConvert)
        {
            //dic for all personAbsence per person
            IDictionary<IPerson, IList<IPersonAbsence>> personList = new Dictionary<IPerson, IList<IPersonAbsence>>();

            //map old absences to new absences
            foreach (KeyValuePair<global::Domain.Agent, IList<global::Domain.AgentDay>> kvp in entitiesToConvert)
            {
                foreach (global::Domain.AgentDay oldAgentDay in kvp.Value)
                {
                    IPersonAbsence personAbsence = base.Mapper.Map(oldAgentDay);

                    if (personAbsence != null)
                    {
                        if (personList.ContainsKey(personAbsence.Person))
                            personList[personAbsence.Person].Add(personAbsence);
                        else
                        {
                            IList<IPersonAbsence> list = new List<IPersonAbsence> { personAbsence };
                            personList.Add(personAbsence.Person, list);
                        }
                    }
                }
            }

            //merge absences
            foreach (KeyValuePair<IPerson, IList<IPersonAbsence>> kvp in personList)
            {
                //list with merged absences
                IList<IPersonAbsence> mergeList = MergeAbsences(kvp.Value);

                //add to repository
                foreach (IPersonAbsence personAbsence in mergeList)
                    Repository.Add(personAbsence);
            }

            //persist
            UnitOfWork.PersistAll();
        }
     
        /// <summary>
        /// Merge absences(create one absence from many where these intersect or is adjacent)
        /// </summary>
        /// <param name="personAbsenceList"></param>
        /// <returns></returns>
        private static IList<IPersonAbsence> MergeAbsences(IList<IPersonAbsence> personAbsenceList)
        {
            //list with merged absences
            IList<IPersonAbsence> mergedAbsences = new List<IPersonAbsence>();
            //result from a merge
            IPersonAbsence newPersonAbsence;
            //absences to remove when they are merged into one absence
            IList<IPersonAbsence> upForDelete = new List<IPersonAbsence>();

            //loop each unmerged personAbsence
            foreach(IPersonAbsence originalPersonAbsence in personAbsenceList)
            {
                
                newPersonAbsence = null;
                upForDelete.Clear();

                //loop each merged personAbsence
                foreach (IPersonAbsence mergedAbsence in mergedAbsences)
                {
                    //check if payloads are equal
                    if (originalPersonAbsence.Layer.Payload.Equals(mergedAbsence.Layer.Payload))
                    {
                        //check if periods intersect or is adjacent
                        if (originalPersonAbsence.Period.Intersect(mergedAbsence.Period) || originalPersonAbsence.Period.AdjacentTo(mergedAbsence.Period))
                        {
                            //set merged to be deleted
                            upForDelete.Add(mergedAbsence);
                            //set new absence
	                        var newLayer = new AbsenceLayer(mergedAbsence.Layer.Payload,
	                                                        mergedAbsence.Period.MaximumPeriod(originalPersonAbsence.Period));
							newPersonAbsence = new PersonAbsence(mergedAbsence.Person, mergedAbsence.Scenario, newLayer);

                        }
                    }
                }

                //if we have a new merged absence
                if (newPersonAbsence != null)
                {
                    //remove earlier merged absences
                    foreach (IPersonAbsence personAbsence in upForDelete)
                    {
                        mergedAbsences.Remove(personAbsence);
                    }
                    //add the new merged absence
                    mergedAbsences.Add(newPersonAbsence);
                }
                else
                {
                    //no merge occurred, add the original to merged list
                    mergedAbsences.Add(originalPersonAbsence);  
                }
            }

            //return list with merged absences
            return mergedAbsences;
        }
    }
}
