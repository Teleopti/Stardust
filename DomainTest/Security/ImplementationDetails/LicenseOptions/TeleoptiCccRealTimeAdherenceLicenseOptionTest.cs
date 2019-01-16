using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.DomainTest.Security.LicenseOptions
{
	[TestFixture]
    public class TeleoptiCccRealTimeAdherenceLicenseOptionTest
    {
        private TeleoptiCccRealTimeAdherenceLicenseOption _target;

        [SetUp]
        public void Setup()
        {
            _target = new TeleoptiCccRealTimeAdherenceLicenseOption();
        }

        [Test]
        public void VerifyRealTimeAdherenceOverviewEnabled()
		{
            IList<IApplicationFunction> applicationFunctions = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
		
            _target.EnableApplicationFunctions(applicationFunctions);
			
            IList<IApplicationFunction> enabledFunctions = _target.EnabledApplicationFunctions;
			enabledFunctions.Should().Contain(ApplicationFunction.FindByPath(applicationFunctions,
				DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview));
		}        
		
		[Test]
        public void VerifyModifyAdherenceEnabled()
		{
            IList<IApplicationFunction> applicationFunctions = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
		
            _target.EnableApplicationFunctions(applicationFunctions);
			
            IList<IApplicationFunction> enabledFunctions = _target.EnabledApplicationFunctions;
			enabledFunctions.Should().Contain(ApplicationFunction.FindByPath(applicationFunctions,
				DefinedRaptorApplicationFunctionPaths.ModifyAdherence));
		}
		
		[Test]
		public void VerifyHistoricalOverviewEnabled()
		{
			IList<IApplicationFunction> applicationFunctions = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
		
			_target.EnableApplicationFunctions(applicationFunctions);
			
			IList<IApplicationFunction> enabledFunctions = _target.EnabledApplicationFunctions;
			enabledFunctions.Should().Contain(ApplicationFunction.FindByPath(applicationFunctions,
				DefinedRaptorApplicationFunctionPaths.HistoricalOverview));
		}
		
		[Test]
		public void VerifyAdjustAdherenceEnabled()
		{
			IList<IApplicationFunction> applicationFunctions = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
		
			_target.EnableApplicationFunctions(applicationFunctions);
			
			IList<IApplicationFunction> enabledFunctions = _target.EnabledApplicationFunctions;
			enabledFunctions.Should().Contain(ApplicationFunction.FindByPath(applicationFunctions,
				DefinedRaptorApplicationFunctionPaths.AdjustAdherence));
		}
    }
}