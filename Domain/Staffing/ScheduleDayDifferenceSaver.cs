using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Staffing
{
	public interface IScheduleDayDifferenceSaver
	{
		void TakeSnapShot(IScheduleDictionary scheduleDictionary);
		void SaveDifferences();
	}

	public class ScheduleDayDifferenceSaver : IScheduleDayDifferenceSaver
	{
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private IScheduleDictionary _scheduleDictionary;

		public ScheduleDayDifferenceSaver(ISkillCombinationResourceRepository skillCombinationResourceRepository)
		{
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
		}
		public void TakeSnapShot(IScheduleDictionary scheduleDictionary)
		{
			_scheduleDictionary = scheduleDictionary;
			//do nothing yet
			// clone all days and put in a list
		}

		public void SaveDifferences()
		{
			//do nothing yet
			//compare whats in the snapshot with whats in the dictionary now
			//and get a list of deltas, IEnumerable<SkillCombinationResource>
			// that we save to _skillCombinationResourceRepository.Persist
		}
	}

	public class EmptyScheduleDayDifferenceSaver : IScheduleDayDifferenceSaver
	{
		public void TakeSnapShot(IScheduleDictionary scheduleDictionary)
		{
			//do nothing
		}

		public void SaveDifferences()
		{
			//do nothing
		}
	}

}