using System.Collections.Specialized;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
   public class PersonSkillModel : EntityContainer<IPersonSkill>
   {
   		private StringCollection _proficiencyValues;
	   private int _proficiency = 100;

		public IPersonSkill PersonSkill { get { return ContainedEntity; } }

   		public int TriState { get; set; }

		public int ActiveTriState { get; set; }

		public int PersonSkillExistsInPersonCount { get; set; }

		public int ActiveSkillsInPersonPeriodCount { get; set; }

		public string DescriptionText { get { return ContainedEntity.Skill.Name; } }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public StringCollection ProficiencyValues
   		{
			get
			{
				if(_proficiencyValues == null)
					return new StringCollection { Proficiency.ToString(CultureInfo.InvariantCulture) };
				if(_proficiencyValues.Count == 0)
					return new StringCollection { Proficiency.ToString(CultureInfo.InvariantCulture) };
				if(_proficiencyValues.Count > 1)
				{
					var ret = new StringCollection { UserTexts.Resources.SeveralValues};
					foreach (var proficiencyValue in _proficiencyValues)
					{
						ret.Add(proficiencyValue);
					}
					return ret;
				}
				return _proficiencyValues;
			}
			set { _proficiencyValues = value; }
   		}

	   public void AddProficiencyValue(string value)
	   {
		   if (_proficiencyValues == null)
			   _proficiencyValues =  new StringCollection { Proficiency.ToString(CultureInfo.InvariantCulture) };
		   if (!_proficiencyValues.Contains(value))
		   	_proficiencyValues.Add(value);
	   }

		public int Proficiency 
		{
			get
	   		{
	   			return _proficiency;
	   		}
			set
			{
				if(value < 1 || value > 999)
					return;

				_proficiency = value;
				_proficiencyValues = new StringCollection { value.ToString(CultureInfo.InvariantCulture) };
			}
		}
    }
}
