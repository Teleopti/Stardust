'use strict';

describe('rtaOverviewComponent', function () {
  var $fakeBackend,
    $httpBackend,
    $componentController,
    $controllerBuilder,
    ctrl,
    scope,
    vm;

  var stateParams = {};

  beforeEach(function () {
    module('wfm.rta');
  });

  beforeEach(function () {
    module(function ($provide) {
      $provide.factory('$stateParams', function () {
        stateParams = {};
        return stateParams;
      });
    });
  });

  beforeEach(inject(function (_$httpBackend_,_$componentController_, _ControllerBuilder_, _FakeRtaBackend_) {
    $httpBackend = _$httpBackend_;
    $componentController = _$componentController_;
    $controllerBuilder = _ControllerBuilder_;
    $fakeBackend = _FakeRtaBackend_;

    $fakeBackend.clear();
    scope = $controllerBuilder.setup('RtaMainController');

  }));

  it('should display site card data', function () {
    $fakeBackend.withSiteAdherence({
      Id: "londonGuid",
      Name: "London",
      AgentsCount: 11,
      InAlarmCount: 5,
      Color: "warning"
    });

    vm = $controllerBuilder.createController().vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    expect(ctrl.siteCards[0].site.Id).toEqual("londonGuid");
    expect(ctrl.siteCards[0].site.Name).toEqual("London");
    expect(ctrl.siteCards[0].site.AgentsCount).toEqual(11);
    expect(ctrl.siteCards[0].site.InAlarmCount).toEqual(5);
    expect(ctrl.siteCards[0].site.Color).toEqual("warning");
  });

  it('should display closed site cards by default', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: "londonGuid",
        Name: "London",
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: "warning"
      });

    var vm = $controllerBuilder.createController().vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    expect(ctrl.siteCards[0].isOpen).toEqual(false);
  });

  it('should display open site cards when open in url', function () {
    stateParams.open = "true";
    $fakeBackend
      .withSiteAdherence({
        Id: "londonGuid",
        Name: "London",
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: "warning"
      });

    var vm = $controllerBuilder.createController().vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    expect(ctrl.siteCards[0].isOpen).toEqual(true);
  });

  it('should display team data in site card when clicked', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: "londonGuid",
        Name: "London",
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: "warning"
      })
      .withTeamAdherence({
        SiteId: "londonGuid",
        Name: "Green",
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: "warning"
      });

    var vm = $controllerBuilder.createController().vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    ctrl.siteCards[0].isOpen = true;
    ctrl.siteCards[0].fetchTeamData(ctrl.siteCards[0]);
    $httpBackend.flush();

    expect(ctrl.siteCards[0].teams[0].SiteId).toEqual('londonGuid');
    expect(ctrl.siteCards[0].teams[0].Name).toEqual('Green');
    expect(ctrl.siteCards[0].teams[0].AgentsCount).toEqual(11);
    expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(5);
    expect(ctrl.siteCards[0].teams[0].Color).toEqual('warning');
  });

});
