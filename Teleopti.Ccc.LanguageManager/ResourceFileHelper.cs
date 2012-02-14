using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Xml.Xsl;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.LanguageManager
{
    /// <summary>
    /// Helper class for working with ResX files
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-05
    /// </remarks>
    internal class ResourceFileHelper
    {
        ResXResourceWriter resourceWriter;
        ILocalized myLocalizable;
        IEnumerable<DictionaryEntry> resourceEntries;
        string _mainResourceFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileHelper"/> class.
        /// </summary>
        /// <param name="mainResourceFile">The main resource file.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public ResourceFileHelper(string mainResourceFile)
        {
            _mainResourceFile = mainResourceFile;
        }

        /// <summary>
        /// Creates the new resources.
        /// </summary>
        /// <param name="formType">Type of the form.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public void CreateNewResources(Type formType)
        {
            ConstructorInfo[] constructors = formType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            ConstructorInfo constructorToUse = (from c in constructors
                                                where c.GetParameters().Count() == 0
                                                select c).FirstOrDefault();

            if (constructorToUse == null)
            {
                throw new InvalidOperationException("There must be at least one constructor without arguments.");
            }

            object myInstance = constructorToUse.Invoke(new object[] {});
            myLocalizable = myInstance as ILocalized;
            if (myLocalizable == null) return;

            using(var resourceReader = new ResXResourceReader(_mainResourceFile))
            {
                resourceReader.UseResXDataNodes = true;
                resourceEntries = resourceReader.Cast<DictionaryEntry>();
                //resourceReader.Close();
            }
            

            using (resourceWriter = new ResXResourceWriter(_mainResourceFile))
            {
                FieldInfo[] translateableFields = formType.GetFields(BindingFlags.Instance |
                                                                     BindingFlags.NonPublic);

                //Because the resourcewriter doesn't have "Append" mode, we need to write all old values, all over again
                foreach (DictionaryEntry item in resourceEntries)
                {
                    resourceWriter.AddResource((ResXDataNode)item.Value);
                }

                foreach (FieldInfo fieldInfo in translateableFields)
                {
                    GetControlText(fieldInfo.GetValue(myLocalizable) as System.ComponentModel.Component);
                }

                //Get texts from form
                GetControlText(myLocalizable as System.ComponentModel.Component);

                resourceWriter.Close();
            }

            
        }

        /// <summary>
        /// Synchronizes to language.
        /// </summary>
        /// <param name="localeId">The locale id.</param>
        /// <param name="useValueFromSource">if set to <c>true</c> [use value from source].</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public void SynchronizeToLanguage(int localeId, bool useValueFromSource)
        {
            SynchronizeToLanguage(localeId, _mainResourceFile, useValueFromSource);
        }

        /// <summary>
        /// Synchronizes neutral language to language.
        /// </summary>
        /// <param name="localeId">The locale id.</param>
        /// <param name="sourceResourceFile">The source resource file.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public void SynchronizeToLanguage(int localeId, string sourceResourceFile, bool useValueFromSource)
        {
            ResXResourceReader resourceReader;
            using(resourceReader = new ResXResourceReader(sourceResourceFile))
            {
                resourceReader.UseResXDataNodes = true;
                //resourceEntries == Main language resource entries
                resourceEntries = resourceReader.Cast<DictionaryEntry>();
                //resourceReader.Close();
            }
            

            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(localeId);

            string targetResourceFile = _mainResourceFile.Substring(0, _mainResourceFile.Length - 5) +
                string.Format(CultureInfo.CurrentCulture, ".{0}.resx", cultureInfo.IetfLanguageTag);

            List<DictionaryEntry> targetExistingEntries;
            if (File.Exists(targetResourceFile))
            {
                using(resourceReader = new ResXResourceReader(targetResourceFile))
                {
                    resourceReader.UseResXDataNodes = true;
                    targetExistingEntries = resourceReader.Cast<DictionaryEntry>().ToList();
                    //resourceReader.Close();
                }
                
            }
            else
            {
                //No existing resources!
                targetExistingEntries = new List<DictionaryEntry>();
            }

            using (resourceWriter = new ResXResourceWriter(targetResourceFile))
            {
                //Because the resourcewriter doesn't have "Append" mode, we need to write all old values, all over again
                for (int i = 0; i < targetExistingEntries.Count; i++) //each (var item in targetExistingEntries)
                {
                    if (useValueFromSource)
                    {
                        DictionaryEntry matchingContent = resourceEntries.FirstOrDefault(e => (string)e.Key == (string)targetExistingEntries[i].Key);
                        if (matchingContent.Value != null)
                        {
                            targetExistingEntries.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }
                    resourceWriter.AddResource((ResXDataNode)targetExistingEntries[i].Value);
                }

                foreach (DictionaryEntry item in resourceEntries)
                {
                    DictionaryEntry matchingContent = targetExistingEntries.FirstOrDefault(e => (string)e.Key == (string)item.Key);
                    if (matchingContent.Value == null)
                    {
                        string currentValue = (string)((ResXDataNode)item.Value).GetValue((ITypeResolutionService)null);
                        if (string.IsNullOrEmpty(currentValue)) continue;

                        ResXDataNode newNode = new ResXDataNode((string)item.Key,
                            ((useValueFromSource) ? currentValue : String.Empty));

                        if (useValueFromSource)
                            newNode.Comment = ((ResXDataNode)item.Value).Comment;
                        else
                            newNode.Comment = currentValue;

                        resourceWriter.AddResource(newNode);
                    }
                }
                resourceWriter.Close();
            }
        }

        /// <summary>
        /// Creates the file for language translation.
        /// </summary>
        /// <param name="targetLocaleId">The target locale id.</param>
        /// <param name="targetFileName">Name of the target file.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public void CreateFileForLanguageTranslation(int targetLocaleId, string targetFileName)
        {
            ResXResourceReader resourceReader;
            using(resourceReader = new ResXResourceReader(_mainResourceFile))
            {
                resourceReader.UseResXDataNodes = true;
                //resourceEntries == Main language resource entries
                resourceEntries = resourceReader.Cast<DictionaryEntry>();
                //resourceReader.Close();
            }
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(targetLocaleId);

            string targetResourceFile = _mainResourceFile.Substring(0, _mainResourceFile.Length - 5) +
                string.Format(CultureInfo.CurrentCulture,".{0}.resx", cultureInfo.IetfLanguageTag);

            IEnumerable<DictionaryEntry> targetExistingEntries;
            if (File.Exists(targetResourceFile))
            {
                using(resourceReader = new ResXResourceReader(targetResourceFile))
                {
                    resourceReader.UseResXDataNodes = true;
                    targetExistingEntries = resourceReader.Cast<DictionaryEntry>();
                    //resourceReader.Close();
                }
            }
            else
            {
                //No existing resources!
                targetExistingEntries = new Dictionary<string, ResXDataNode>().Cast<DictionaryEntry>();
            }

            using (resourceWriter = new ResXResourceWriter(targetFileName.Replace(".xls", ".resx")))
            {
                foreach (DictionaryEntry item in resourceEntries)
                {
                    DictionaryEntry itemInTarget = targetExistingEntries.FirstOrDefault(e => (string)e.Key == (string)item.Key);
                    string originalValue = (string)((ResXDataNode)item.Value).GetValue((ITypeResolutionService)null);
                    if (itemInTarget.Value == null ||
                        ((ResXDataNode)itemInTarget.Value).Comment != originalValue)
                    {
                        ResXDataNode newNode = new ResXDataNode((string)item.Key, String.Empty);
                        newNode.Comment = originalValue;
                        resourceWriter.AddResource(newNode);
                    }
                }
                resourceWriter.Close();
            }

            XslCompiledTransform xt = new XslCompiledTransform();
            xt.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResxToExcel.xslt"));
            xt.Transform(targetFileName.Replace(".xls", ".resx"), targetFileName);

            File.Delete(targetFileName.Replace(".xls", ".resx"));
        }

        /// <summary>
        /// Exports the language summary.
        /// </summary>
        /// <param name="targetLocaleId">The target locale id.</param>
        /// <param name="targetFileName">Name of the target file.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-07
        /// </remarks>
        public void ExportLanguageSummary(int? targetLocaleId, string targetFileName)
        {
            string targetResourceFile;
            if (targetLocaleId.HasValue)
            {
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(targetLocaleId.Value);
                targetResourceFile = _mainResourceFile.Substring(0, _mainResourceFile.Length - 5) +
                    string.Format(CultureInfo.CurrentCulture, ".{0}.resx", cultureInfo.IetfLanguageTag);
            }
            else
            {
                targetResourceFile = _mainResourceFile;
            }

            if (File.Exists(targetResourceFile))
            {
                XslCompiledTransform xt = new XslCompiledTransform();
                xt.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ResxToExcel.xslt"));
                xt.Transform(targetResourceFile, targetFileName);
            }
        }

        /// <summary>
        /// Gets the controls current text and add it to the resource file if key is missing.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-04
        /// </remarks>
        private void GetControlText(System.ComponentModel.Component component)
        {
            if (component == null) return;
            var descriptorCollection = from d in System.ComponentModel.TypeDescriptor.GetProperties(component).Cast<System.ComponentModel.PropertyDescriptor>()
                                       where d.IsLocalizable &&
                                             d.PropertyType == typeof(string)
                                       select d;

            foreach (var descriptor in descriptorCollection)
            {
                object currentValueAsObject = descriptor.GetValue(component);
                if (currentValueAsObject == null) continue;
                string currentValue = currentValueAsObject as string;
                
                if (!String.IsNullOrEmpty(currentValue))
                {
                    if (currentValue.StartsWith("yy", StringComparison.OrdinalIgnoreCase) ||
                        currentValue.StartsWith("yyy", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (currentValue.StartsWith("xxx", StringComparison.OrdinalIgnoreCase))
                        currentValue = currentValue.Substring(3);

                    if (currentValue.StartsWith("xx", StringComparison.OrdinalIgnoreCase))
                        currentValue = currentValue.Substring(2);

                    string resourceKey = currentValue;
                    if (resourceEntries.FirstOrDefault(e => ((string)e.Key).ToUpperInvariant() == resourceKey.ToUpperInvariant()).Value == null)
                    {
                        for (int i = 0; i < currentValue.Length; i++)
                        {
                            if (i>0 &&
                                char.IsUpper(currentValue, i))
                            {
                                currentValue = currentValue.Insert(i, " ");
                                i++;
                            }
                        }

                        ResXDataNode newNode = new ResXDataNode(resourceKey.Replace(" ",""), currentValue);
                        newNode.Comment = currentValue;
                        resourceWriter.AddResource(newNode);
                    }
                }
            }
        }
    }
}
