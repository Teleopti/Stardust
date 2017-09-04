'use strict';

describe('RtaOverviewController selection', function () {

  beforeEach(module('wfm.rta'));

  var $stateParams;
  beforeEach(function () {
    module(function ($provide) {
      $provide.factory('$stateParams', function () {
        $stateParams = {};
        return $stateParams;
      });
    });
  });

  var
    $controllerBuilder,
    $fakeBackend,
    $scope,
    $state;
  beforeEach(inject(function (_FakeRtaBackend_, _ControllerBuilder_, _$state_) {
    $controllerBuilder = _ControllerBuilder_;
    $fakeBackend = _FakeRtaBackend_;
    $state = _$state_;
    $scope = $controllerBuilder.setup('RtaOverviewController39082');

    spyOn($state, 'go');
		$state.current.name = 'rta-refact';
  }));

  it('should select site', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'parisId'
      });
    var c = $controllerBuilder.createController();
    var vm = c.vm;

    c.apply(function () {
      vm.siteCards[0].isSelected = true;
    });

    expect(vm.siteCards[0].isSelected).toEqual(true);
  });

  it('should select site from url', function () {
    $stateParams.siteIds = ['parisId'];
    $fakeBackend
      .withSiteAdherence({
        Id: 'parisId'
      });

    var vm = $controllerBuilder.createController().vm;

    expect(vm.siteCards[0].isSelected).toEqual(true);
  });

  it('should select team from url', function () {
    $stateParams.teamIds = ['redId'];
    $fakeBackend
      .withSiteAdherence({
        Id: 'parisId'
      })
      .withTeamAdherence({
        SiteId: 'parisId',
        Id: 'redId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;

    expect(vm.siteCards[0].teams[0].isSelected).toEqual(true);
  });

  it('should open site of team in url', function () {
    $stateParams.teamIds = ['redId'];
    $fakeBackend
      .withSiteAdherence({
        Id: 'parisId'
      })
      .withTeamAdherence({
        SiteId: 'parisId',
        Id: 'redId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;

    expect(vm.siteCards[0].isOpen).toEqual(true);
  });

  it('should still display opened sites even though no team is selected', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'parisId'
      })
      .withTeamAdherence({
        SiteId: 'parisId',
        Id: 'redId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;
    c.apply(function () {
      vm.siteCards[0].isOpen = true;
    });

    expect(vm.siteCards[0].teams[0].isSelected).toEqual(false);
  });

});
