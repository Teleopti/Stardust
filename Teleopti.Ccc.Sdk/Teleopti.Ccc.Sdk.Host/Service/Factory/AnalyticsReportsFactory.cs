using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    internal static class AnalyticsReportsFactory
    {

        internal static ICollection<MatrixReportInfoDto> Create()
        {
            var dtoList = new List<MatrixReportInfoDto>();
            var appFuncList = PrincipalAuthorization.Current_DONTUSE().GrantedFunctions().FilterBySpecification(new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix));

            IStatisticRepository staticRepository = StatisticRepositoryFactory.Create();
            ICollection<MatrixReportInfo> matrixReportCollection = staticRepository.LoadReports();
            if (matrixReportCollection == null)
                return dtoList;
            foreach (IApplicationFunction appFunc in appFuncList)
            {
                var foreignId = new Guid(appFunc.ForeignId);
                MatrixReportInfo matrixReportInfo = MatrixReportInfo.FindByReportId(matrixReportCollection, foreignId);
                var dto = new MatrixReportInfoDto
                              {
                                  Id = matrixReportInfo.ReportId,
                                  ReportName = matrixReportInfo.ReportName,
                                  ReportUrl = matrixReportInfo.ReportUrl.Replace("~", string.Empty),
                                  TargetFrame = matrixReportInfo.TargetFrame
                              };
                dtoList.Add(dto);
            }

            return dtoList;
        }
    }
}