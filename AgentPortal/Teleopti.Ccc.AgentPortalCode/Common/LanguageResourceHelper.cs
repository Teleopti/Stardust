using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using System.Reflection;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using System.ComponentModel;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    /// <summary>
    /// Performs the translation
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-21
    /// </remarks>
    public class LanguageResourceHelper
    {
        #region Constants

        /// <summary>
        /// Placeholder for the text Singular.
        /// </summary>
        private const string Singular = "Singular";

        #endregion

        #region Set texts

        ILocalized _targetControl;

        /// <summary>
        /// Sets the texts.
        /// </summary>
        /// <param name="targetControl">The control to set texts for.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-03
        /// </remarks>
        public void SetTexts(ILocalized targetControl)
        {
            _targetControl = targetControl;
            if (StateHolderReader.IsInitialized)
            {
                CultureInfo uiCulture = CultureInfo.CurrentUICulture;
                _targetControl.RightToLeft = uiCulture.TextInfo.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
                if (_targetControl is Form &&
                    _targetControl.RightToLeft == RightToLeft.Yes)
                {
                    ((Form)_targetControl).RightToLeftLayout = true;
                }
            }
            else
            {
                //TODO: Inform about the error in designer mode?
                return;
            }

            GetFieldsAndTranslate();
        }

        /// <summary>
        /// Gets the fields and translate.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-03
        /// </remarks>
        private void GetFieldsAndTranslate()
        {
            FieldInfo[] translateableFields = _targetControl.GetType().GetFields(BindingFlags.Instance |
                                                                                 BindingFlags.NonPublic |
                                                                                 BindingFlags.Public);

            foreach (FieldInfo fieldInfo in translateableFields)
            {
                SetControlText(fieldInfo.GetValue(_targetControl) as Component);
            }

            //Set texts for form
            SetControlText(_targetControl as Component);
        }

        /// <summary>
        /// Sets the control text.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-03
        /// </remarks>
        private static void SetControlText(Component component)
        {
            if (component == null) return;

            PropertyDescriptorCollection descriptorCollection = GetDescriptorCollection(component);

            foreach (PropertyDescriptor descriptor in descriptorCollection)
            {
                object keyAsObject = descriptor.GetValue(component);
                if (keyAsObject == null) continue;
                string keyName = keyAsObject.ToString();

                if (keyName.StartsWith("xxx", StringComparison.OrdinalIgnoreCase)) keyName = keyName.Substring(3);
                if (keyName.StartsWith("xx", StringComparison.OrdinalIgnoreCase)) keyName = keyName.Substring(2);
                string resourceText = UserTexts.Resources.ResourceManager.GetString(keyName);
                if (!String.IsNullOrEmpty(resourceText))
                    descriptor.SetValue(component, resourceText);
            }
        }




        private static PropertyDescriptorCollection GetDescriptorCollection(Component component)
        {
            PropertyDescriptorCollection descriptorCollection = TypeDescriptor.GetProperties(component);
            PropertyDescriptorCollection descriptorCollectionFiltered = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor descriptor in descriptorCollection)
            {
                if ((descriptor.PropertyType == typeof(string)) && descriptor.IsLocalizable)
                    descriptorCollectionFiltered.Add(descriptor);
            }

            return descriptorCollectionFiltered;
        }



        #endregion

        /// <summary>
        /// Translates the enum to list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-10-14
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static IList<KeyValuePair<T, string>> TranslateEnumToList<T>()
        {
            Type type = typeof(T);
            if (!type.IsEnum) throw new ArgumentException("The provided type must be enum.", "type");
            IList<KeyValuePair<T, string>> translations = new List<KeyValuePair<T, string>>();
            foreach (T item in Enum.GetValues(type))
            {
                string key = string.Concat(type.Name, item.ToString());
                string translatedText = UserTexts.Resources.ResourceManager.GetString(key);
                if (string.IsNullOrEmpty(translatedText)) throw new ArgumentNullException("type", String.Concat("All items in the enum must have translations. Missing key was:", key));
                {
                    KeyValuePair<T, string> keyValuePair = new KeyValuePair<T, string>(item, translatedText);
                    translations.Add(keyValuePair);
                }
            }
            return translations;
        }

        /// <summary>
        /// Translates the enum.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-28
        /// </remarks>
        public static IDictionary<object, string> TranslateEnum(Type type)
        {
            if (!type.IsEnum) throw new ArgumentException("The provided type must be enum.", "type");
            IDictionary<object, string> translations = new Dictionary<object, string>();
            foreach (var item in Enum.GetValues(type))
            {
                string key = string.Concat(type.Name, item.ToString());
                string translatedText = UserTexts.Resources.ResourceManager.GetString(key);
                if (string.IsNullOrEmpty(translatedText)) throw new ArgumentNullException("type", String.Concat("All items in the enum must have translations. Missing key was:", key));
                translations.Add(item, translatedText);
            }
            return translations;
        }

        /// <summary>
        /// Translates the enum value.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-10
        /// </remarks>
        public static string TranslateEnumValue(object enumValue)
        {
            Type enumType = enumValue.GetType();
            if (!enumType.IsEnum) throw new ArgumentException("The provided object must be an enum value.", "enumValue");
            string key = string.Concat(enumType.Name, enumValue.ToString());
            return UserTexts.Resources.ResourceManager.GetString(key);
        }

        /// <summary>
        /// Translates the enum value.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: shirang
        /// Created date: 2008-12-8
        /// </remarks>
        public static string TranslateEnumValueAndGetSingular(object enumValue)
        {
            Type enumType = enumValue.GetType();
            if (!enumType.IsEnum) throw new ArgumentException("The provided object must be an enum value.", "enumValue");
            string key = string.Concat(enumType.Name, enumValue.ToString(), Singular);
            return UserTexts.Resources.ResourceManager.GetString(key);
        }

        public static string Translate(string valueToTranslate)
        {
            UserTexts.Resources.ResourceManager.IgnoreCase = true;
            return TranslateString(valueToTranslate);
        }

        private static string TranslateString(string stringToTranslate)
        {
            string translatedString = stringToTranslate;
            if (string.IsNullOrEmpty(stringToTranslate))
            {
                translatedString = string.Empty;
                stringToTranslate = string.Empty;
            }

            if (stringToTranslate.StartsWith("xxx", StringComparison.OrdinalIgnoreCase)) translatedString = stringToTranslate.Substring(3);
            if (stringToTranslate.StartsWith("xx", StringComparison.OrdinalIgnoreCase)) translatedString = stringToTranslate.Substring(2);
            return UserTexts.Resources.ResourceManager.GetString(translatedString);
        }
    }
}
