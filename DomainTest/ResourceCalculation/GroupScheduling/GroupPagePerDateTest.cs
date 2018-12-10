using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation.GroupScheduling
{
	[TestFixture]
	public class GroupPagePerDateTest
	{
		private IGroupPagePerDate _target;
		private IDictionary<DateOnly, IGroupPage> dic;

		[SetUp]
		public void Setup()
		{
			dic = new Dictionary<DateOnly, IGroupPage>();
			_target = new GroupPagePerDate(dic);
		}

		[Test]
		public void ShouldReturnGroupPageByKey()
		{
			dic.Add(new DateOnly(2010, 1, 1), new GroupPage("asd"));
			dic.Add(new DateOnly(2010, 1, 2), new GroupPage("qwe"));

			IGroupPage page = _target.GetGroupPageByDate(new DateOnly(2010, 1, 1));

			Assert.AreEqual("asd", page.Description.Name);
		}
	}
}