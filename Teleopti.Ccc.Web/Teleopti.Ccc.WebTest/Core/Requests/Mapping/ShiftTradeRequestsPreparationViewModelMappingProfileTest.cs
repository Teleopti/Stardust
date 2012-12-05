using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeRequestsPreparationViewModelMappingProfileTest
	{
		[Test]
		public void ShouldMapHasWorkflowControlSetToFalse()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeRequestsPreparationViewModelMappingProfile()));

			var domainData = new ShiftTradeRequestsPreparationDomainData();

			var result = Mapper.Map<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>(domainData);

			result.HasWorkflowControlSet.Should().Be.False();
		}

		[Test]
		public void ShouldMapHasWorkflowControlSetToTrue()
		{
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeRequestsPreparationViewModelMappingProfile()));

			var domainData = new ShiftTradeRequestsPreparationDomainData{ WorkflowControlSet = new WorkflowControlSet()};

			var result = Mapper.Map<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>(domainData);

			result.HasWorkflowControlSet.Should().Be.True();
		}
	}
}
