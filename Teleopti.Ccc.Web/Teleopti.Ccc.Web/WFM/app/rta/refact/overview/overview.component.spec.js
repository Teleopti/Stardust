'use strict';

describe('rtaOverviewComponent', function () {
  var $fakeBackend,
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

  beforeEach(inject(function (_$componentController_, _ControllerBuilder_, _FakeRtaBackend_) {
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

    expect(ctrl.siteCards[0].Id).toEqual("londonGuid");
    expect(ctrl.siteCards[0].Name).toEqual("London");
    expect(ctrl.siteCards[0].AgentsCount).toEqual(11);
    expect(ctrl.siteCards[0].InAlarmCount).toEqual(5);
    expect(ctrl.siteCards[0].Color).toEqual("warning");
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

});
