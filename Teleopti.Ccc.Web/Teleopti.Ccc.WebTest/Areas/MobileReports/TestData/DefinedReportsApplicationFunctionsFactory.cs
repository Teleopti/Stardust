namespace Teleopti.Ccc.WebTest.Areas.MobileReports.TestData
{
	using System.Collections.Generic;
	using System.Linq;

	using Teleopti.Ccc.Domain.Collection;
	using Teleopti.Ccc.Domain.Security.AuthorizationData;
	using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
	using Teleopti.Ccc.TestCommon.FakeData;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core.IoC;
	using Teleopti.Interfaces.Domain;

	internal class DefinedReportsApplicationFunctionsFactory
	{
		private readonly IList<IApplicationFunction> _applicationFunctions;

		public DefinedReportsApplicationFunctionsFactory(string remove )
		{
			this._applicationFunctions = ApplicationFunctionFactory.CreateApplicationFunctionWithMatrixReports();
			this.AddAllDefinedReports();
			if (!string.IsNullOrEmpty(remove))
			{
				this._applicationFunctions.Remove(this._applicationFunctions.First(x => remove.Equals(x.FunctionCode)));
			}

		}

		public IList<IApplicationFunction> ApplicationFunctions
		{
			get
			{
				return this._applicationFunctions;
			}
		}

		private void AddAllDefinedReports()
		{
			var martix = this.GetMatrix();
			DefinedReports.ReportInformations.Select(f => f.FunctionCode).Distinct().ForEach(
				df =>
				this._applicationFunctions.Add(
					ApplicationFunctionFactory.CreateApplicationFunction(
						df, martix, DefinedForeignSourceNames.SourceMatrix, "20")));
		}

		private IApplicationFunction GetMatrix()
		{
			var code = ApplicationFunction.GetCode(DefinedRaptorApplicationFunctionPaths.AccessToReports);
			return this._applicationFunctions.First(f => f.FunctionCode == code);
		}
	}
}