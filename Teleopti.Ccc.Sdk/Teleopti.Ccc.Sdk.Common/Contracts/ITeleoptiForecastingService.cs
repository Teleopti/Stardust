using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
    /// <summary>
    /// Contains the operations to work with forecasting data
    /// </summary>
    [ServiceContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/",
        Name = "TeleoptiForecastingService",
        ConfigurationName = "Teleopti.Ccc.Sdk.Common.Contracts.ITeleoptiForecastingService")]
    public interface ITeleoptiForecastingService
    {
        /// <summary>
        /// Gets all skill data for one day in the given time zone.
        /// </summary>
        /// <param name="dateOnlyDto">The requested day.</param>
        /// <param name="timeZoneId">The requested time zone.</param>
        /// <returns></returns>
        [OperationContract]
        ICollection<SkillDayDto> GetSkillData(DateOnlyDto dateOnlyDto, string timeZoneId);


		  /// <summary>
		  /// Gets all skill data for the given query.
		  /// </summary>
		  /// <param name="queryDto">The <see cref="QueryDto"/> implementation for loading skill data.</param>
		  /// <returns>Skill data per day and skill with interval information.</returns>
		  [OperationContract]
	    ICollection<SkillDayDto> GetSkillDataByQuery(QueryDto queryDto);

			 /// <summary>
        /// Gets all available skills.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), OperationContract]
        ICollection<SkillDto> GetSkills();
    }
}