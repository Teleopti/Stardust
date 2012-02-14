using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Collections
{
    [TestFixture]
    public class TreeItemEntityHelperTest
    {
        IList<IParentChildEntity> _functions;

        [SetUp]
        public void Setup()
        {
            _functions = new List<IParentChildEntity>(ApplicationFunctionFactory.CreateApplicationFunctionStructure().OfType<IParentChildEntity>());
        }

        [Test]
        public void VerifyTransformEntityToTreeStructure()
        {
            TreeItemEntityHelper.TransformEntityCollectionToTreeStructure(_functions);


        }

        [Test]
        public void VerifyCreateTreeItemWithIEntityInterface()
        { 
            IEntity entity = _functions[0] as IEntity;
            TreeItem<TreeNode> retValue = TreeItemEntityHelper.CreateTreeItem(entity);
            Assert.IsNotNull(retValue);
            Assert.IsNotNull(retValue.Data);
            Assert.AreNotEqual(string.Empty, retValue.Key);
        }

        [Test]
        public void VerifyCreateTreeItemWithIAuthorizationInterface()
        {
            ApplicationFunction function = _functions[0] as ApplicationFunction;
            string descValue = "Description";
            function.FunctionDescription = descValue;
            IAuthorizationEntity entity = function as IAuthorizationEntity;
            TreeItem<TreeNode> retValue = TreeItemEntityHelper.CreateTreeItem(entity);
            Assert.IsNotNull(retValue);
            Assert.IsNotNull(retValue.Data);
            Assert.AreEqual(descValue, retValue.Text);
            Assert.AreNotEqual(retValue.Key, retValue.Text);
        }

        [Test]
        public void VerifyCreateTreeItemWithIListBoxPresenterInterface()
        {
            ApplicationFunction function = _functions[0] as ApplicationFunction;
            string descValue = "Description";
            function.FunctionDescription = descValue;
            AuthorizationEntityListBoxPresenter presenter = new AuthorizationEntityListBoxPresenter(function);
            IListBoxPresenter presenterInterface = presenter as IListBoxPresenter;
            TreeItem<TreeNode> retValue = TreeItemEntityHelper.CreateTreeItem(presenterInterface);
            Assert.IsNotNull(retValue);
            Assert.IsNotNull(retValue.Data);
            Assert.AreEqual(descValue, retValue.Text);
            Assert.AreNotEqual(retValue.Key, retValue.Text);
        }

    }
}
