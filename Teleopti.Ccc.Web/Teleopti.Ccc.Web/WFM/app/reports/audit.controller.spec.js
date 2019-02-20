'use strict';
describe('AuditTrailCtrl', function () {
  var $httpBackend,
  $controller,
  NoticeService,
  currentUserInfo = new FakeCurrentUserInfo(),
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
    module(function ($provide) {
        $provide.service('CurrentUserInfo', function () {
            return currentUserInfo;
        });
    });
  });

  beforeEach(inject(function (_$httpBackend_, _$controller_, _NoticeService_) {
    $httpBackend = _$httpBackend_;
    $controller = _$controller_;
    NoticeService = _NoticeService_;

	  $httpBackend.whenPOST('../api/Reports/PersonsWhoChangedSchedules').respond(function (method, url, data, headers) {
      return [200, fakeChanges]
    });
    $httpBackend.whenGET('../ToggleHandler/AllToggles').respond(function (method, url, data, headers) {
      return [200, true]
    });
    $httpBackend.whenPOST('../api/Reports/OrganizationSelectionAuditTrail').respond(function (method, url, data, headers) {
      return [200,
        [
          {
            Children:[
              {
                Name: "BTS", Id: "9d013613-7c79-4621-b166-a39a00b9d634"
              }
            ],
            Id:"7a6c0754-4de8-48fb-8aee-a39a00b9d1c3",
            Name:"BTS"
          }
        ]
      ];
    });

    $httpBackend.whenPOST('../api/Reports/ScheduleAuditTrailReport').respond(function (method, url, data, headers) {
      return [200,
        [{
          Name: 'Agent'
        }]
      ];
    });
  }));

  function FakeCurrentUserInfo() {
      this.CurrentUserInfo = function () {
          return {
              DateFormatLocale: "en-US",
              Language: 'en-US'
          };
      };
  }

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

  it('should not allow invalid start or end date', function () {
    var vm = $controller('AuditTrailController');

    vm.dateChangeRange = {
			startDate: moment(),
			endDate: moment().subtract(3, 'days')
		};
		vm.dateModifyRange = {
      startDate: moment(),
			endDate: moment().subtract(3, 'days')
		};
    var form = {
      drop: {
        Id:'123'
      }
    }

    vm.sendForm(form);

    //expect http.post not to have been called
  });

  it('should not allow missing start or end date', function () {
    var vm = $controller('AuditTrailController');

    vm.dateChangeRange = {
			startDate: null,
			endDate: moment().subtract(3, 'days')
		};
		vm.dateModifyRange = {
      startDate: moment(),
			endDate: null
		};
    var form = {
      drop: {
        Id:'123'
      }
    }

    vm.sendForm(form);

    //expect http.post not to have been called
  });


  it('should find selected organization data', function () {
    var vm = $controller('AuditTrailController');
    var orgData = [
      {
        Id:"321",
        Name:"London",
        Type:"Site",
        Children: [
          {
            Id:"abc",
            Name:"Team1",
            Type:"Team"
          }
        ]
      },
      {
        Id:"123",
        Name:"Paris",
        Type:"Site",
        selected: true,
        Children: [
          {
            Id:"abc",
            Name:"Team1",
            Type:"Team"
          },
          {
            Id:"abc",
            Name:"Team1",
            Type:"Team",
            selected: true
          }
        ]
      }
    ];
    vm.calculateOrgSelection(orgData);
    expect(vm.filteredOrgData.length).toBe(1);
    expect(vm.filteredOrgData[0]).toBe('abc');
  });
});
