using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using AbsenceFactory = Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData.AbsenceFactory;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
    [TestFixture]
    public class RequestTransformTest
    {
        private DataTable _dataTable;
        private RequestTransformer _target;
        private IList<IPersonRequest> _personRequestList;
        private IList<IShiftTradeSwapDetail> _shiftTradeSwapDetails;
        
      
        [SetUp]
        public void Setup()
        {
            _dataTable = new DataTable();
            _dataTable.Locale = Thread.CurrentThread.CurrentCulture;
            RequestInfrastructure.AddColumnsToDataTable(_dataTable);
        }

       [Test]
        public void ShouldInitializeAbsenceRequest()
        {
            IPerson person2 = PersonFactory.CreatePerson("Test", "person2");
            person2.SetId(Guid.NewGuid());
            IPersonRequest personRequest2 = new PersonRequest(person2);
            personRequest2.Request = new AbsenceRequest(AbsenceFactory.CreateAbsenceCollection().ElementAt(0), new DateTimePeriod(2012, 01, 28, 2012, 01, 28));
            personRequest2.SetId(Guid.NewGuid());
            personRequest2.Deny(person2, "test", new PersonRequestAuthorizationCheckerForTest());
            RaptorTransformerHelper.SetUpdatedOn(personRequest2, DateTime.UtcNow);
            _personRequestList = new List<IPersonRequest>() { personRequest2 };
            _target = new RequestTransformer();
            _target.Transform(_personRequestList, 96, _dataTable);
            Assert.AreEqual(1, _personRequestList.Count);
          
        }

        [Test]
        public void ShouldInitializeTextRequest()
        {
            IPerson person = PersonFactory.CreatePerson("hello", "test");
            person.SetId(Guid.NewGuid());
            IPersonRequest personRequest1 = new PersonRequest(person);
            personRequest1.SetId(Guid.NewGuid());
            personRequest1.Pending();
            RaptorTransformerHelper.SetUpdatedOn(personRequest1, DateTime.UtcNow);

            personRequest1.Request = new TextRequest(new DateTimePeriod(2012,01,28,2012,01,28));
            _personRequestList = new List<IPersonRequest>() { personRequest1};
            _target = new RequestTransformer();
            _target.Transform(_personRequestList, 96, _dataTable);
            Assert.AreEqual(1, _personRequestList.Count); 
        }

        [Test]
        public void ShouldInitializeApprovedAbsenceRequest()
        {
           
            IPerson person2 = PersonFactory.CreatePerson("Test", "person2");
            person2.SetId(Guid.NewGuid());
           
            IPersonRequest personRequest3 = new PersonRequest(person2);
            personRequest3.Request = new AbsenceRequest(AbsenceFactory.CreateAbsenceCollection().ElementAt(0), new DateTimePeriod(2012, 01, 26, 2012, 01, 26));
            personRequest3.SetId(Guid.NewGuid());
            personRequest3.Pending();
            personRequest3.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());
            RaptorTransformerHelper.SetUpdatedOn(personRequest3, DateTime.UtcNow);

            _personRequestList = new List<IPersonRequest>() { personRequest3 };
            _target = new RequestTransformer();
            _target.Transform(_personRequestList, 96, _dataTable);
            Assert.AreEqual(1, _personRequestList.Count);
            
        }

        [Test]
        public void ShouldInitializeShiftTradeRequest()
        {
            IPerson person2 = PersonFactory.CreatePerson("Test", "person2");
            person2.SetId(Guid.NewGuid());

            IPerson person3 = PersonFactory.CreatePerson("Test", "person3");
            person3.SetId(Guid.NewGuid());
            IPersonRequest personRequest4 = new PersonRequest(person3);
            var shiftTradeSwap1 = new ShiftTradeSwapDetail(person3, person2, new DateOnly(2012, 01, 01), new DateOnly(2012, 01, 01));
            _shiftTradeSwapDetails = new List<IShiftTradeSwapDetail>() { shiftTradeSwap1 };
            personRequest4.Request = new ShiftTradeRequest(_shiftTradeSwapDetails);
            personRequest4.SetId(Guid.NewGuid());
            personRequest4.Pending();
            RaptorTransformerHelper.SetUpdatedOn(personRequest4, DateTime.UtcNow);

            _personRequestList = new List<IPersonRequest>() { personRequest4 };
            _target = new RequestTransformer();
            _target.Transform(_personRequestList, 96,  _dataTable);
            Assert.AreEqual(1, _personRequestList.Count);
            
        }
    }
}
