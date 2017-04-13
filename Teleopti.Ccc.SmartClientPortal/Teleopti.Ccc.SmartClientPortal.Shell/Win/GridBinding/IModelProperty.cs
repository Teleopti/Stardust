namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.GridBinding
{
	public interface IModelProperty<T>
	{
		object GetModelValue(T model);
		void SetModelValue(T model, object value);
		string PropertyName { get; }
	}
}