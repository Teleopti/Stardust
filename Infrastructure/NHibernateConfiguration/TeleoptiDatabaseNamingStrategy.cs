using System;
using NHibernate.Cfg;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
    /// <summary>
    /// Naming strategy when generating
    /// db scheme from nhib mapping files.
    /// Implemented as a singleton.
    /// </summary>
    public class TeleoptiDatabaseNamingStrategy : INamingStrategy
    {
        private readonly static INamingStrategy _instance = new TeleoptiDatabaseNamingStrategy();

        private TeleoptiDatabaseNamingStrategy()
        {
        }

        public static INamingStrategy Instance
        {
            get { return _instance; }
        }

        public string ClassToTableName(string className)
        {
            var clsName = className ?? string.Empty;
            return clsName.Substring(clsName.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase) + 1);
        }


        public string PropertyToColumnName(string propertyName)
        {
            var propName = propertyName ?? string.Empty;
            return propName.Substring(propName.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase) + 1);
        }

        public string TableName(string tableName)
        {
            return tableName;
        }

        public string ColumnName(string columnName)
        {
            return columnName;
        }

        public string PropertyToTableName(string className, string propertyName)
        {
            var propName = propertyName ?? string.Empty;
            return propName.Substring(propName.LastIndexOf(".", StringComparison.CurrentCultureIgnoreCase) + 1);
        }

        public string LogicalColumnName(string columnName, string propertyName)
        {
            //rk - fix later
            throw new NotImplementedException();
        }
    }
}