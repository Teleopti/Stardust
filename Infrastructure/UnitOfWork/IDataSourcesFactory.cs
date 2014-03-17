using System.Collections.Generic;
using System.Xml.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Factory for LogonableDataSources
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-04-23
    /// </remarks>
    public interface IDataSourcesFactory
    {
        /// <summary>
        /// Creates the datasource using the specified nhibernate config xml structure.
        /// </summary>
        /// <param name="hibernateConfiguration">The nhibernate config xml structure.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-09
        /// </remarks>
        IDataSource Create(XElement hibernateConfiguration);


        /// <summary>
        /// Creates the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="statisticConnectionString">The statistic connection string.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-23
        /// </remarks>
        IDataSource Create(IDictionary<string, string> settings,
                            string statisticConnectionString);

        /// <summary>
        /// Tries to create the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="dataSource">The datasource if succeeded.</param>
        /// <returns>true if it succeeds</returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-10-11
        /// </remarks>
        bool TryCreate(string file, out IDataSource dataSource);

        /// <summary>
        /// Tries to create a DataSource from NhibConfiguration XML.
        /// </summary>
        /// <param name="element">The root element.</param>
        /// <param name="dataSource">The datasource if succeeded.</param>
        /// <returns>true if it succeeds</returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-11-09
        /// </remarks>
        bool TryCreate(XElement element, out IDataSource dataSource);
    }
}