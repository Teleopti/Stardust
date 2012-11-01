﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class ApplicationFunctionDtoTest
    {
        private ApplicationFunctionDto _target;
        private IApplicationFunction _applicationFunction;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/23/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            MockRepository mocks = new MockRepository();
            _applicationFunction = mocks.StrictMock<IApplicationFunction>();

            Guid guid = new Guid("018794DA-A984-4C0E-AFF9-FCCE1923ACF6");

            using (mocks.Record())
            {
                Expect.On(_applicationFunction)
                    .Call(_applicationFunction.Id)
                    .Return(guid)
                    .Repeat.Any();

                Expect.On(_applicationFunction)
                    .Call(_applicationFunction.FunctionDescription)
                    .Return("Desc")
                    .Repeat.Any();

                Expect.On(_applicationFunction)
                    .Call(_applicationFunction.ForeignId)
                    .Return("100")
                    .Repeat.Any();

                Expect.On(_applicationFunction)
                    .Call(_applicationFunction.ForeignSource)
                    .Return("")
                    .Repeat.Any();

                Expect.On(_applicationFunction)
                    .Call(_applicationFunction.FunctionCode)
                    .Return("code707")
                    .Repeat.Any();

                Expect.On(_applicationFunction)
                    .Call(_applicationFunction.FunctionPath)
                    .Return(@"Raptor\MyTime")
                    .Repeat.Any();

                Expect.On(_applicationFunction)
                    .Call(_applicationFunction.IsPreliminary )
                    .Return(true)
                    .Repeat.Any();
                
            }

	        _target = new ApplicationFunctionDto
		        {
			        FunctionDescription = _applicationFunction.FunctionDescription,
			        ForeignId = _applicationFunction.ForeignId,
			        ForeignSource = _applicationFunction.ForeignSource,
			        FunctionCode = _applicationFunction.FunctionCode,
			        FunctionPath = _applicationFunction.FunctionPath,
			        IsPreliminary = _applicationFunction.IsPreliminary
		        };
            Assert.IsNull(_target.Id);
        }

        /// <summary>
        /// Verifies the can set and get properties.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/23/2008
        /// </remarks>
        [Test]
        public void VerifyCanSetAndGetProperties()
        {
            _target.ForeignId = "100";
            Assert.AreEqual("100", _target.ForeignId);

            _target.FunctionDescription = "Desc";
            Assert.AreEqual("Desc", _target.FunctionDescription);

            _target.ForeignSource = "1001";
            Assert.AreEqual("1001", _target.ForeignSource);

            Assert.AreEqual("code707", _target.FunctionCode);

            _target.FunctionPath = @"Raptor\MyTime";
            Assert.AreEqual(@"Raptor\MyTime", _target.FunctionPath);

            _target.FunctionCode = "code";
            Assert.AreEqual("code", _target.FunctionCode);
        }

        [Test]
        public void VerifyCanCreateApplicationFunctionUsingFunctionCode()
        {
            _target = new ApplicationFunctionDto("/MyTime");

            Assert.IsNotNull(_target);
            Assert.AreEqual("/MyTime", _target.FunctionCode);

        }
    }
}