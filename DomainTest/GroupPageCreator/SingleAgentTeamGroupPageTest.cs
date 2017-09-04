using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
    [TestFixture]
    public class SingleAgentTeamGroupPageTest
    {
        private SingleAgentTeamGroupPage _target;
        private BaseLineData _baseLineData = new BaseLineData();

        [SetUp]
        public void Setup()
        {
            _target = new SingleAgentTeamGroupPage();
            _baseLineData.Person1.SetName(new Name("First1", "Last1"));
            _baseLineData.Person2.SetName(new Name("First2", "Last2"));
        }

        [Test]
        public void CheckTheGroupPageCreatedWithCorrectRootCount()
        {
            _baseLineData.GroupPageOptions.CurrentGroupPageName = "SingleAgentTeam";
            _baseLineData.GroupPageOptions.CurrentGroupPageNameKey  = "SingleAgentTeam";
            var gPage =  _target.CreateGroupPage(_baseLineData.PersonList,_baseLineData.GroupPageOptions );

            Assert.AreEqual("SingleAgentTeam", gPage.Description.Name);
            Assert.AreEqual("SingleAgentTeam", gPage.DescriptionKey);
            Assert.AreEqual(2, gPage.RootGroupCollection.Count);
        }

        [Test]
        public void CheckTheGroupPageCreatedWithCorrectPerson()
        {
            _baseLineData.GroupPageOptions.CurrentGroupPageName = "SingleAgentTeam";
            _baseLineData.GroupPageOptions.CurrentGroupPageNameKey = "SingleAgentTeam";
            var gPage = _target.CreateGroupPage(_baseLineData.PersonList, _baseLineData.GroupPageOptions);

            var nameList =new List<Name>{_baseLineData.Person1.Name,_baseLineData.Person2.Name};
            Assert.AreEqual(gPage.RootGroupCollection[0].PersonCollection.Count,1);
            Assert.AreEqual(gPage.RootGroupCollection[1].PersonCollection.Count, 1);
            Assert.IsTrue(nameList.Contains(gPage.RootGroupCollection[0].PersonCollection[0].Name) );
            Assert.IsTrue(nameList.Contains(gPage.RootGroupCollection[1].PersonCollection[0].Name));
        }

       [Test]
       public void RootGroupNameShouldBeTruncatedTo50Characters()
       {
		   _baseLineData.GroupPageOptions.CurrentGroupPageName = "SingleAgentTeam";
		   _baseLineData.GroupPageOptions.CurrentGroupPageNameKey = "SingleAgentTeam";
	       var person = PersonFactory.CreatePerson("ABCDEFGHIJKLMNOPQRSTUVWXYZ", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
		   var gPage = _target.CreateGroupPage(new[]{person}, _baseLineData.GroupPageOptions);
		   Assert.AreEqual(50, gPage.RootGroupCollection[0].Name.Length);
		   Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXYZ-ABCDEFGHIJKLMNOPQRSTUVW", gPage.RootGroupCollection[0].Name);
       }
    }
}
