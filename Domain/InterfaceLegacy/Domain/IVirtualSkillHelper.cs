using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Functionality to work with virtual skills.
	/// </summary>
	public interface IVirtualSkillHelper
	{
		/// <summary>
		/// Loads the virtual skills.
		/// </summary>
		/// <param name="availableSkills">The available skills.</param>
		/// <returns></returns>
		IList<ISkill> LoadVirtualSkills(IList<ISkill> availableSkills);

		/// <summary>
		/// Save the virtual skill.
		/// </summary>
		/// <param name="virtualSkill">The virtual skill to save.</param>
		void SaveVirtualSkill(IAggregateSkill virtualSkill);

	    /// <summary>
	    /// Merge settings from the new skill to the current virtual skill.
	    /// </summary>
	    /// <param name="newVirtualSkill">The virtual skill with new details.</param>
	    /// <param name="oldName"></param>
	    void EditAndRenameVirtualSkill(IAggregateSkill newVirtualSkill, string oldName);
	}
}