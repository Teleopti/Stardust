using System;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class FindExternalLogOnQueryHandlerTest
	{
		private FindExternalLogOnQueryHandler _target;
		private FakeExternalLogOnRepository _externalLogOnRepository;
		private FakeCurrentUnitOfWorkFactory _unitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			_externalLogOnRepository = new FakeExternalLogOnRepository();
			_unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			
			_target = new FindExternalLogOnQueryHandler(_externalLogOnRepository, _unitOfWorkFactory, new ExternalLogOnAssembler());
		}

		[Test]
		public void ShouldThrowFaultExceptionWhenSearchingForTooMany()
		{
			var query = new FindExternalLogOnQueryDto();
			51.Times(() => query.ExternalLogOnCollection.Add(Guid.NewGuid().ToString()));

			Assert.Throws<FaultException>(() => _target.Handle(query));
		}
		[Test]
		public void ShouldReturnEmptyListWhenNoMatches()
		{
			_externalLogOnRepository.Add(new ExternalLogOn {AcdLogOnName = "somethingelse"});
			var query = new FindExternalLogOnQueryDto();
			query.ExternalLogOnCollection.Add("abc123");
			
			var result = _target.Handle(query);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldReturnMatches()
		{
			var expected = new ExternalLogOn { AcdLogOnName = "abc123", AcdLogOnOriginalId = "321abc", DataSourceId = 1}.WithId(Guid.NewGuid());
			_externalLogOnRepository.Add(expected);
			var query = new FindExternalLogOnQueryDto();
			query.ExternalLogOnCollection.Add(expected.AcdLogOnName);

			var result = _target.Handle(query);

			result.Should().Not.Be.Empty();
			result.First().AcdLogOnName.Should().Be.EqualTo(expected.AcdLogOnName);
			result.First().AcdLogOnOriginalId.Should().Be.EqualTo(expected.AcdLogOnOriginalId);
			result.First().DataSourceId.Should().Be.EqualTo(expected.DataSourceId);
			result.First().Id.Should().Be.EqualTo(expected.Id);
		}
	}
}