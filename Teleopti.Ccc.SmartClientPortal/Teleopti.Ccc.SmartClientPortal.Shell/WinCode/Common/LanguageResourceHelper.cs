using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
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
	        if (targetControl == null)
		        return;

            //Make sure that the keys are searched for regardless of casing
            UserTexts.Resources.ResourceManager.IgnoreCase = true;

            _targetControl = targetControl;
            if (StateHolderReader.IsInitialized)
            {
                if (Thread.CurrentPrincipal is TeleoptiPrincipalForLegacy)
                {
                    _targetControl.RightToLeft =
                        (((ITeleoptiPrincipalForLegacy)TeleoptiPrincipal.CurrentPrincipal).UnsafePerson.PermissionInformation.RightToLeftDisplay)
                            ? RightToLeft.Yes
                            : RightToLeft.No;

                    var form = _targetControl as Form;
                    if (form!=null && _targetControl.RightToLeft == RightToLeft.Yes)
                    {
                        form.RightToLeftLayout = true;
                    }
                }
            }
            else
            {
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
                SetControlText(fieldInfo.GetValue(_targetControl) as System.ComponentModel.Component);
            }

            //Set texts for form
            SetControlText(_targetControl as System.ComponentModel.Component);
        }

        /// <summary>
        /// Sets the control text.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-03
        /// </remarks>
        private static void SetControlText(System.ComponentModel.Component component)
        {
            if (component == null) return;
            var descriptorCollection = from d in System.ComponentModel.TypeDescriptor.GetProperties(component).Cast<System.ComponentModel.PropertyDescriptor>()
                                       where d.IsLocalizable &&
                                             d.PropertyType == typeof(string)
                                       select d;

            foreach (var descriptor in descriptorCollection)
            {
                object keyAsObject = descriptor.GetValue(component);
                if (keyAsObject == null) continue;
                string keyName = keyAsObject.ToString();
                string resourceText = TranslateString(keyName);
                if (!String.IsNullOrEmpty(resourceText))
                    descriptor.SetValue(component, resourceText);
            }
        }

        private static string TranslateString(string stringToTranslate)
        {
            string translatedString = String.Empty;
	        bool toUpper = false;
            if (stringToTranslate.StartsWith("xxx", StringComparison.OrdinalIgnoreCase)) translatedString = stringToTranslate.Substring(3);
            if (stringToTranslate.StartsWith("xx", StringComparison.Ordinal)) translatedString = stringToTranslate.Substring(2);
			if (stringToTranslate.StartsWith("XX", StringComparison.Ordinal))
			{
				translatedString = stringToTranslate.Substring(2);
				toUpper = true;
			}

			string resourceText = UserTexts.Resources.ResourceManager.GetString(translatedString);
	        if (toUpper && !resourceText.IsNullOrEmpty() && StateHolderReader.IsInitialized)
	        {
		        if (Thread.CurrentPrincipal is TeleoptiPrincipalForLegacy)
		        {
			        var cultureInfo = ((ITeleoptiPrincipalForLegacy) TeleoptiPrincipal.CurrentPrincipal).UnsafePerson.PermissionInformation.UICulture();
			        resourceText = resourceText.ToUpper(cultureInfo);
		        }
	        }
	        return resourceText;
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
        public static IList<KeyValuePair<T, string>> TranslateEnumToList<T>()
        {
            Type type = typeof (T);
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		public static IDictionary<T,string> TranslateEnum<T>()
        {
        	var type = typeof (T);
            if (!type.IsEnum) throw new ArgumentException("The provided type must be enum.","type");
            IDictionary<T, string> translations = new Dictionary<T, string>();
            foreach (T item in Enum.GetValues(type))
            {
                string key = string.Concat(type.Name, item.ToString());
                string translatedText = UserTexts.Resources.ResourceManager.GetString(key);
                if (string.IsNullOrEmpty(translatedText)) throw new ArgumentNullException("T", String.Concat("All items in the enum must have translations. Missing key was:", key));
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

        public static string Translate(string valueToTranslate)
        {
            UserTexts.Resources.ResourceManager.IgnoreCase = true;
            return TranslateString(valueToTranslate);
        }
    }
}
