using System;
using NUnit.Framework;
using Stardust.Manager.Helpers;

namespace ManagerTest.Helpers
{
    [TestFixture()]
    public class UriHelperTests
    {
        [Test()]
        public void CreateCorrectUriTest()
        {
            Uri uri = UriHelper.CreateCorrectUri("localhost",
                                                 null);

            Assert.IsNotNull(uri);
        }
    }
}