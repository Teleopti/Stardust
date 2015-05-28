namespace Teleopti.Ccc.SyncfusionGridBinding
{
    public interface IModelProperty<T>
    {
        object GetModelValue(T model);
        void SetModelValue(T model, object value);
        string PropertyName { get; }
    }
}