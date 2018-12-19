namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Holds an entity that possibly not known when 
	/// this object is declared.
	/// </summary>
	public interface IAuditSetter
	{
		/// <summary>
		/// Sets the entity
		/// </summary>
		/// <param name="entity"></param>
		void SetEntity(IAuditSetting entity);
	}
}