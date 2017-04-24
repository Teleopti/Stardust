describe('ReportsOverviewController', function () {

  var $controller,
      vm;

  beforeEach(function() {
    module('wfm.reports');
    module('externalModules');
  });

  beforeEach(inject(function(_$controller_){
    $controller = _$controller_;
    vm = $controller('ReportsOverviewController');
  }));

  var fakeReports  = [
    {
      Category: "Category1",
      IsWebReport: false,
      Name: "Report1",
      Url: "www.ThisIsJustATest.com"
    },
    {
      Category: "Category2",
      IsWebReport: false,
      Name: "Report2",
      Url: "www.ThisIsJustATest.com"
    },
    {
      Category: "Category1",
      IsWebReport: false,
      Name: "Report3",
      Url: "www.ThisIsJustATest.com"
    },
    {
      Category: null, //Leave blank
      IsWebReport: false,
      Name: "Report4",
      Url: "www.ThisIsJustATest.com"
    }
  ];

  it("should generate grouped reports and isolate custom type", function() {

    expect(vm.groupReports(fakeReports).length).toEqual(2);
    expect(vm.groupReports(fakeReports)[0].Type).toEqual('Category1');
    expect(vm.groupReports(fakeReports)[1].Type).toEqual('Category2');
  });

﻿})
