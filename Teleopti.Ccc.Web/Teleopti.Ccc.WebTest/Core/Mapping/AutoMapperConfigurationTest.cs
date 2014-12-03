using AutoMapper;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Mapping;

namespace Teleopti.Ccc.WebTest.Core.Mapping
{
	[TestFixture]
	public class AutoMapperConfigurationTest
	{

		[Test]
		public void ShouldConfigure()
		{
			var container = MockRepository.GenerateMock<IComponentContext>();
			var target = new AutoMapperConfiguration(new[] { new TestProfile() }, container);

			target.Execute(null);

			Mapper.AssertConfigurationIsValid();
			Mapper.Map<TestSource, TestTarget>(new TestSource{Prop = "value"});
		}

		public class TestSource
		{
			public string Prop { get; set; }
		}

		public class TestTarget
		{
			public string Prop { get; set; }
		}

		public class TestProfile : Profile
		{
			public override string ProfileName
			{
				get { return "TestProfile"; }
			}

			protected override void Configure()
			{
				CreateMap<TestSource, TestTarget>();
			}
		}
	}
}
