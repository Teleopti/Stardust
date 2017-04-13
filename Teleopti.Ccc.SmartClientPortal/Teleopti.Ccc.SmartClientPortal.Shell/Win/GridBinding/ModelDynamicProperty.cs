using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.GridBinding
{
	public class ModelDynamicProperty<T, TItemType, TRowDefinition> : IModelProperty<T>
	{
		private readonly Func<T, TRowDefinition, object> _getModelValue;
		private readonly Action<T, TRowDefinition, object> _setModelValue;
		private readonly TRowDefinition _rowDefinition;

		public ModelDynamicProperty(Func<T, TRowDefinition, object> getModelValue, Action<T, TRowDefinition, object> setModelValue, TRowDefinition rowDefinition)
		{
			_getModelValue = getModelValue;
			_setModelValue = setModelValue;
			_rowDefinition = rowDefinition;
		}

		public object GetModelValue(T model)
		{
			return _getModelValue(model, _rowDefinition);
		}

		public void SetModelValue(T model, object value)
		{
			if (value is TItemType)
			{
				_setModelValue(model, _rowDefinition, value);
			}
		}

		public string PropertyName
		{
			get { return string.Empty; }
		}
	}
}