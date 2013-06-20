namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IPersistedLayer<T> : ILayer<T>, IAggregateEntity
	{ }
	/// <summary>
	/// Base class for all layers
	/// </summary>
	/// <typeparam name="T">The type of the "payload"</typeparam>
	public interface ILayer<out T> : ICloneableEntity<ILayer<T>>, IPeriodized
	{
		/// <summary>
		/// Gets the name of the payload.
		/// </summary>
		/// <value>The name of the payload.</value>
		T Payload { get; }

		int OrderIndex { get; }
	}
}
