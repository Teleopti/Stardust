using System.Reflection;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.GridBinding
{
	public class ModelProperty<T> : IModelProperty<T>
	{
		private readonly string _propertyName;
		private readonly PropertyInfo _propertyInfo;

		public ModelProperty(string propertyName)
		{
			_propertyName = propertyName;
			_propertyInfo = typeof(T).GetProperty(_propertyName);
		}

		public object GetModelValue(T model)
		{
			return _propertyInfo.GetValue(model, null);
		}

		public void SetModelValue(T model, object value)
		{
			_propertyInfo.SetValue(model, value, null);
		}

		public string PropertyName
		{
			get { return _propertyName; }
		}
	}
}