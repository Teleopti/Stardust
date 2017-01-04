using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CascadingPersonSkillProvider : PersonSkillProvider
	{
		private readonly CascadingPersonalSkills _cascadingPersonalSkills = new CascadingPersonalSkills();
		private readonly PersonalSkills _personalSkills = new PersonalSkills();

		protected override OriginalPersonSkills PersonSkills(IPersonPeriod personPeriod)
		{
			return new OriginalPersonSkills(_cascadingPersonalSkills.PersonSkills(personPeriod),_personalSkills.PersonSkills(personPeriod));
		}
	}
}