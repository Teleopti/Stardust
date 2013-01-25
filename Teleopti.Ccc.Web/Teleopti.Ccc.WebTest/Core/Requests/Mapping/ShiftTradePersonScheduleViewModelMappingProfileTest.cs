using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradePersonScheduleViewModelMappingProfileTest
	{
		private IProjectionProvider _projectionProvider;

		[SetUp]
		public void Setup()
		{
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradePersonScheduleViewModelMappingProfile(() => _projectionProvider)));
		}

		[Test]
		public void Should()
		{

		}
	}
}
