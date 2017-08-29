﻿'use strict';

describe('RtaMainController', function () {

  return;

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
    $scope = $controllerBuilder.setup('RtaMainController');
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
});
