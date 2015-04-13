using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public interface ISkillVisualLayerCollectionDictionaryCreator
	{
		SkillVisualLayerCollectionDictionary CreateSiteVisualLayerCollectionDictionary(
			IList<IVisualLayerCollection> relevantProjections, DateOnly day);
	}

    public class SkillVisualLayerCollectionDictionaryCreator : ISkillVisualLayerCollectionDictionaryCreator
    {
        public SkillVisualLayerCollectionDictionary CreateSiteVisualLayerCollectionDictionary(IList<IVisualLayerCollection> relevantProjections, DateOnly day)
        {
            var ret = new SkillVisualLayerCollectionDictionary();

			foreach (var visualLayerCollection in relevantProjections)
			{
				IPersonPeriod personPeriod = visualLayerCollection.Person.Period(day);
				if (personPeriod == null)
					continue;

				if (personPeriod.Team.Site.MaxSeatSkill == null)
					continue;

				IList<IVisualLayerCollection> visList;
				if (!ret.TryGetValue(personPeriod.Team.Site.MaxSeatSkill, out visList))
				{
					visList = new List<IVisualLayerCollection>();
					ret.Add(personPeriod.Team.Site.MaxSeatSkill, visList);
				}
				visList.Add(visualLayerCollection);
			}

    		return ret;
        }
    }
}
