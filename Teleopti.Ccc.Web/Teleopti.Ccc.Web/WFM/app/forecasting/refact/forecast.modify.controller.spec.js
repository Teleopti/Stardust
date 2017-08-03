xdescribe('ForecastCtrl', function () {

  var $controller,
    $httpBackend,
    vm;

  beforeEach(function () {
    module('wfm.forecasting');
  });

  beforeEach(inject(function (_$controller_, _$httpBackend_) {
    $controller = _$controller_;
    $httpBackend = _$httpBackend_;

    vm = $controller('ForecastModCtrl');
  }));

})
