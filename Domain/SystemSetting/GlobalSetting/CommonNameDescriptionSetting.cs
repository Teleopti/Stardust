using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting.GlobalSetting
{
    /// <summary>
    /// CommonNameDescriptionSetting, inherited Setting.
    /// </summary>
    /// <remarks>
    /// Created by: henryg
    /// Created date: 2008-07-07
    /// </remarks>
    [Serializable]
    public class CommonNameDescriptionSetting : SettingValue, ICommonNameDescriptionSetting
    {
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
        private const string defaultCommonNameDescription = "{FirstName} {LastName}";
        private string _value;

        public CommonNameDescriptionSetting()
        {
            _value = defaultCommonNameDescription;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonNameDescriptionSetting"/> class.
        /// </summary>
        /// <param name="aliasFormat">The alias format.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-07-07
        /// </remarks>
        public CommonNameDescriptionSetting(string aliasFormat)
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
	        var builded = AliasFormat;
	        builded = builded.Replace(FirstName, person.Name.FirstName);
            builded = builded.Replace(LastName, person.Name.LastName);
            builded = builded.Replace(EmployeeNumber, person.EmploymentNumber);
            return builded;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public string BuildCommonNameDescription(ILightPerson lightPerson)
        {
            var builded = AliasFormat;
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

	    public string BuildSqlUpdateForAnalytics()
	    {
		    var sqlConcat = $"'{AliasFormat.Replace("'", "''")}'"; // Replace to prevent sql injections
			sqlConcat = sqlConcat.Replace(FirstName, "' + [first_name] + '");
			sqlConcat = sqlConcat.Replace(LastName, "' + [last_name] + '");
			sqlConcat = sqlConcat.Replace(EmployeeNumber, "' + [employment_number] + '");
		    sqlConcat = sqlConcat.Replace("'' + ", "");
			sqlConcat = sqlConcat.Replace(" + ''", "");
			return $"UPDATE mart.dim_person SET person_name = {sqlConcat} WHERE person_id != -1";
		}
    }
}