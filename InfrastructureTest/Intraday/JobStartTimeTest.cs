//using System;
//using System.Linq;
//using NUnit.Framework;
//using SharpTestsEx;
//using Teleopti.Ccc.Domain.Intraday;
//using Teleopti.Ccc.Infrastructure.Intraday;

//namespace Teleopti.Ccc.InfrastructureTest.Intraday
//{
//	[TestFixture]
//	[UnitOfWorkTest]
//	public class JobStartTimeRepositoryTest
//	{
//		public IJobStartTimeRepository Target { get; set; }

//		[Test]
//		public void ShouldPersist()
//		{
//			var datetime =new DateTime(2016,12,16,10,0,0);
//			var buId = Guid.NewGuid();
//			Target.Persist(buId, datetime);

//			var result = Target.LoadAll();
//			result.Keys.Count.Should().Be.EqualTo(1);
//			result.Keys.FirstOrDefault().Should().Be.EqualTo(buId);
//			result.Values.FirstOrDefault().Should().Be.EqualTo(datetime);
//		}

//		[Test]
//		public void ShouldDeleteAndPersist()
//		{
//			var datetime = new DateTime(2016, 12, 16, 10, 0, 0);
//			var buId = Guid.NewGuid();
//			Target.Persist(buId, datetime);

//			Target.Persist(buId, datetime.AddDays(1));

//			var result = Target.LoadAll();
//			result.Keys.Count.Should().Be.EqualTo(1);
//			result.Keys.FirstOrDefault().Should().Be.EqualTo(buId);
//			result.Values.FirstOrDefault().Should().Be.EqualTo(datetime.AddDays(1));
//		}
//	}

	
//}
