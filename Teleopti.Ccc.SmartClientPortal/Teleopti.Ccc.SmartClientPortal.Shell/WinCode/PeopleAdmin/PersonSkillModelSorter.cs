using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin
{
	public class PersonSkillModelSorter
	{
		public IEnumerable<PersonSkillModel> Sort(IEnumerable<PersonSkillModel> personSkillModels)
		{
			var sorted = new List<PersonSkillModel>();
			var someHave = new List<PersonSkillModel>();
			var noneHave = new List<PersonSkillModel>();

			foreach (var personSkillModel in personSkillModels.OrderBy(x => x.DescriptionText))
			{
				switch (personSkillModel.TriState)
				{
					case 1:
						sorted.Add(personSkillModel);
						break;
					case 2:
						someHave.Add(personSkillModel);
						break;
					default:
						noneHave.Add(personSkillModel);
						break;
				}
			}

			sorted.AddRange(someHave);
			sorted.AddRange(noneHave);

			return sorted;
		}
	}
}
