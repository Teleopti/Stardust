using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
    [Serializable]
    public class CommonNameDescriptionSettingScheduleExport : SettingValue, ICommonNameDescriptionSetting
	{
		public const string Key = "CommonNameDescriptionScheduleExport";

		/// <summary>
		/// Represents the FirstName
		/// </summary>
		public const string FirstName = "{FirstName}";
        /// <summary>
        /// Represents the LastName
        /// </summary>
        public const string LastName = "{LastName}";
        /// <summary>
        /// Represents the EmployeeNumber
        /// </summary>
        public const string EmployeeNumber = "{EmployeeNumber}";
        private const string DefaultCommonNameDescription = "{FirstName} {LastName}";
        private string _value;

        public CommonNameDescriptionSettingScheduleExport()
        {
            _value = DefaultCommonNameDescription;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonNameDescriptionSetting"/> class.
        /// </summary>
        /// <param name="aliasFormat">The alias format.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-07-07
        /// </remarks>
        public CommonNameDescriptionSettingScheduleExport(string aliasFormat)
        {
            _value = aliasFormat;
        }

        /// <summary>
        /// Gets or sets the alias format.
        /// </summary>
        /// <value>The alias format.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-07-09
        /// </remarks>
        public virtual string AliasFormat
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Builds the common name description.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-07-07
        /// </remarks>
        public virtual string BuildCommonNameDescription(IPerson person)
        {
            if(person == null)
                throw new ArgumentNullException(nameof(person));

            string builded = AliasFormat;
            builded = builded.Replace(FirstName, person.Name.FirstName);
            builded = builded.Replace(LastName, person.Name.LastName);
            builded = builded.Replace(EmployeeNumber, person.EmploymentNumber);
            return builded;
        }

        public string BuildCommonNameDescription(ILightPerson lightPerson)
        {
            if (lightPerson == null)
                throw new ArgumentNullException(nameof(lightPerson));

            string builded = AliasFormat;
            builded = builded.Replace(FirstName, lightPerson.FirstName);
            builded = builded.Replace(LastName, lightPerson.LastName);
            builded = builded.Replace(EmployeeNumber, lightPerson.EmploymentNumber);
            return builded;
        }

        public string BuildCommonNameDescription(string firstName, string lastName, string employmentNumber)
        {
            var builded = AliasFormat;
            builded = builded.Replace(FirstName, firstName);
            builded = builded.Replace(LastName, lastName);
            builded = builded.Replace(EmployeeNumber, employmentNumber);
            return builded;
        }
    }
}
