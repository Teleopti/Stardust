'use strict';
describe('AuditTrailCtrl', function () {
  var $httpBackend,
  $controller,
  NoticeService,
  fakeChanges = [
    {
      Id: '123',
      Name: 'Jimmy'
    }
  ];

  beforeEach(function () {
    module('wfm.reports');
    module('externalModules');
    module('localeLanguageSortingService');
  });

  beforeEach(inject(function (_$httpBackend_, _$controller_, _NoticeService_) {
    $httpBackend = _$httpBackend_;
    $controller = _$controller_;
    NoticeService = _NoticeService_;

    $httpBackend.whenGET('../api/Reports/PersonsWhoChangedSchedules').respond(function (method, url, data, headers) {
      return [200, fakeChanges]
    });
    $httpBackend.whenGET('../ToggleHandler/AllToggles').respond(function (method, url, data, headers) {
      return [200, true]
    });

    $httpBackend.whenPOST('../api/Reports/ScheduleAuditTrailReport').respond(function (method, url, data, headers) {
      return [200,
        [{
          Name: 'Agent'
        }]
      ];
    });
  }));

  it('should respond to search data', function () {
    var vm = $controller('AuditTrailController');
    var form = {
      drop: {
        Id: '123'
      }
    }
    $httpBackend.flush();

    vm.sendForm(form);
    $httpBackend.flush();

    expect(vm.changesData.length).toBe(1);
  });

  it('should get changed by list', function () {
    var vm = $controller('AuditTrailController');

    $httpBackend.flush();

    expect(vm.changedBy.length).toBe(2);
  });
});
