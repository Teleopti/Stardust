using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class ApplicationFunctionFactory
    {

        /// <summary>
        /// Creates a application function.
        /// </summary>
        /// <param name="functionCode">The function code.</param>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        public static IApplicationFunction CreateApplicationFunction(string functionCode, IApplicationFunction parent)
        {
            IApplicationFunction app = new ApplicationFunction(functionCode, parent);
            app.SetId(Guid.NewGuid());
            return app;
        }

        /// <summary>
        /// Creates an application function list.
        /// </summary>
        /// <param name="rootName">The rootname.</param>
        /// <param name="childName">The childname.</param>
        /// <param name="childs">The childs number.</param>
        /// <returns></returns>
        public static IList<IApplicationFunction> CreateApplicationFunctionList(string rootName, string childName, int childs)
        {
            IList<IApplicationFunction> result = new List<IApplicationFunction>(childs + 1);
            IApplicationFunction app = CreateApplicationFunction(rootName);
            result.Add(app);
            for(int index = 0; index < childs; index++)
            {
                IApplicationFunction func = CreateApplicationFunction(childName + index.ToString(CultureInfo.CurrentCulture), app);
                result.Add(func);
            }
            return result;
        }

        /// <summary>
        /// Creates a application function.
        /// </summary>
        /// <param name="functionCode">The function code.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="foreignSource">The foreign source.</param>
        /// <param name="foreignId">The foreign id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        public static IApplicationFunction CreateApplicationFunction(string functionCode, IApplicationFunction parent, string foreignSource, string foreignId)
        {
            IApplicationFunction app = CreateApplicationFunction(functionCode, parent);
            app.ForeignSource = foreignSource;
            app.ForeignId = foreignId;
            return app;
        }

        /// <summary>
        /// Creates a application function.
        /// </summary>
        /// <param name="functionCode">The function code.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        public static IApplicationFunction CreateApplicationFunction(string functionCode)
        {
            IApplicationFunction app = new ApplicationFunction(functionCode);
            app.SetId(Guid.NewGuid());
            return app;
        }

        /// <summary>
        /// Creates the application function structure.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/5/2007
        /// </remarks>
        public static IList<IApplicationFunction> CreateApplicationFunctionStructure()
        {
            IApplicationFunction raptorRoot = ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);
            IApplicationFunction level1Item1 = CreateApplicationFunction("LEVEL1ITEM1", raptorRoot);
            IApplicationFunction level1Item2 = CreateApplicationFunction("LEVEL1ITEM2", raptorRoot);
            IApplicationFunction level2Item1 = CreateApplicationFunction("LEVEL2ITEM1", level1Item1);
            IApplicationFunction level2Item2 = CreateApplicationFunction("LEVEL2ITEM2", level1Item2);
            IApplicationFunction level2Item3 = CreateApplicationFunction("LEVEL2ITEM3", level1Item2);
            IApplicationFunction level3Item1 = CreateApplicationFunction("LEVEL3ITEM1", level2Item3);
            IList<IApplicationFunction> retList = new List<IApplicationFunction>();
            
            retList.Add(level1Item2);
            retList.Add(level1Item1);
            retList.Add(level2Item2);
            retList.Add(level2Item1);
            retList.Add(raptorRoot);
            retList.Add(level3Item1);
            retList.Add(level2Item3);
            return retList;
        }

        /// <summary>
        /// Creates application function collection with matrix reports.
        /// </summary>
        /// <returns></returns>
        public static IList<IApplicationFunction> CreateApplicationFunctionWithMatrixReports()
        {
            IList<IApplicationFunction> functions = new List<IApplicationFunction>();

            IApplicationFunction root = CreateApplicationFunction(DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication);
            IApplicationFunction matrix = CreateApplicationFunction(ApplicationFunction.GetCode(DefinedRaptorApplicationFunctionPaths.AccessToReports), root);
            var idOne = "09DB7510-ED3C-49CE-B49C-D43D94EC7263";
            var idTwo = "1C2BDC8C-BFED-4BB3-AD13-6614488310BE";
            var idThree = "19D90EB4-8249-4585-ABBD-8F7DC7E6DF54";
            functions.Add(root);
            functions.Add(matrix);
            functions.Add(CreateApplicationFunction("Function1", root));
            functions.Add(CreateApplicationFunction("AgentReport", matrix, DefinedForeignSourceNames.SourceMatrix, idOne));
            functions.Add(CreateApplicationFunction("Function2", root));
            functions.Add(CreateApplicationFunction("ReportToRemove", matrix, DefinedForeignSourceNames.SourceMatrix, idTwo));
            functions.Add(CreateApplicationFunction("SiteReport", matrix, DefinedForeignSourceNames.SourceMatrix, idThree));

            return functions;
        }
    }
}
