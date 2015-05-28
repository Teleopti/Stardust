using System.Collections.Generic;

namespace Teleopti.Ccc.Win.GridBinding
{
	public class ModelDictionaryProperty<T, TItemType> : IModelProperty<T>
	{
		private readonly string _dictionaryKeyName;
		private readonly TItemType _defaultValue;
		private readonly IModelProperty<T> _innerModelProperty;

		public ModelDictionaryProperty(string propertyName, string dictionaryKeyName, TItemType defaultValue)
		{
			_dictionaryKeyName = dictionaryKeyName;
			_defaultValue = defaultValue;
			_innerModelProperty = new ModelProperty<T>(propertyName);
		}

		public object GetModelValue(T model)
		{
			var targetDictionary = (IDictionary<string, TItemType>)_innerModelProperty.GetModelValue(model);

			TItemType modelValue;
			if (!targetDictionary.TryGetValue(_dictionaryKeyName, out modelValue))
			{
				modelValue = _defaultValue;
				targetDictionary.Add(_dictionaryKeyName, modelValue);
			}
			return modelValue;
		}

		public void SetModelValue(T model, object value)
		{
			if (value is TItemType)
			{
				var targetDictionary = (IDictionary<string, TItemType>)_innerModelProperty.GetModelValue(model);
				if (targetDictionary.ContainsKey(_dictionaryKeyName))
				{
					targetDictionary[_dictionaryKeyName] = (TItemType)value;
				}
				else
				{
					targetDictionary.Add(_dictionaryKeyName, (TItemType)value);
				}
			}
		}

		public string PropertyName
		{
			get { return _innerModelProperty.PropertyName; }
		}
	}
}