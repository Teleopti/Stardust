describe('ReportsOverviewController', function () {

  var $controller,
    $httpBackend,
    vm,
    fakeReports = [
      {
        Category: "Category1",
        IsWebReport: false,
        Name: "Report1",
        Url: "https://www.teleopti.com/index.aspx"
      },
      {
        Category: "Category2",
        IsWebReport: false,
        Name: "Report2",
        Url: "https://www.teleopti.com/index.aspx"
      },
      {
        Category: "Category1",
        IsWebReport: false,
        Name: "Report3",
        Url: "http://www.teleopti.com/index.aspx"
      },
      {
        Category: null, //Leave blank
        IsWebReport: false,
        Name: "Report4",
        Url: "Iam/internal"
      }
    ];

  beforeEach(function () {
    module('wfm.reports');
    module('externalModules');
    module('localeLanguageSortingService');
  });

  beforeEach(inject(function (_$controller_, _$httpBackend_) {
    $controller = _$controller_;
    $httpBackend = _$httpBackend_;

     $httpBackend.whenGET('../api/Reports/NavigationsCategorized').respond(function (method, url, data, headers) {
      return [200, fakeReports]
    });

    $httpBackend.whenGET('../ToggleHandler/AllToggles').respond(function (method, url, data, headers) {
      return [200, true]
    });

    vm = $controller('ReportsOverviewController');
  }));

  afterEach(function () {
    $httpBackend.verifyNoOutstandingExpectation();
    $httpBackend.verifyNoOutstandingRequest();
  });

  it("should generate grouped reports and isolate custom type", function () {
    $httpBackend.flush();

    expect(vm.reports.length).toEqual(2);
    expect(vm.customTypes.length).toEqual(1);
    expect(vm.reports[0].Type).toEqual('Category1');
    expect(vm.reports[0].childNodes.length).toEqual(2);
    expect(vm.reports[0].childNodes[0].Name).toEqual('Report1');
    expect(vm.reports[1].Type).toEqual('Category2');
  });
})
