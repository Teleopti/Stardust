describe('<measure-config-form2>', function () {
    var attachedElements = [];
    var $compile,
        $rootScope,
        $material,
        $document;

    beforeEach(function () {
        module('wfm.templates', 'externalModules', 'wfm.gamification', 'ngMaterial', 'ngMaterial-mock');

        inject(function ($injector) {
            $compile = $injector.get('$compile');
            $rootScope = $injector.get('$rootScope');
            $material = $injector.get('$material');
            $document = $injector.get('$document');
        });
    });

    afterEach(function () {
        attachedElements.forEach(function (element) {
            var elementScope = element.scope();

            elementScope && elementScope.$destroy();
            element.remove();
        });
        attachedElements = [];
    });

    it('should render glod, silver and bronze setting items.', function () {
        var cmp = setupComponent(testScope());
        var inputs = cmp[0].querySelectorAll('input');
        expect(inputs.length).toBe(3);
    });

    it('measure value should be in desc order', function () {
        var cmp = setupComponent(testScope());

        var ctrl = cmp.isolateScope().$ctrl;

        expect(ctrl.errorMsg.length).toBeGreaterThan(0);
    });

    function setupComponent(scope) {
        var el;

        var template = '<measure-config-form2 bronze-badge-threshold="bronzeBadgeThreshold" silver-badge-threshold="silverBadgeThreshold" gold-badge-threshold="goldBadgeThreshold" value-order="valueOrder" value-format="valueFormat" value-data-type="valueDataType"></measure-config-form2>';

        el = $compile(template)(scope || $rootScope);

        if (scope) {
            scope.$apply();
        } else {
            $rootScope.$digest();
        }

        attachedElements.push(el);
        $document[0].body.appendChild(el[0]);
        return el;
    }

    function testScope() {
        var scope = $rootScope.$new();
        scope.bronzeBadgeThreshold = 90;
        scope.silverBadgeThreshold = 60;
        scope.goldBadgeThreshold = 30;
        scope.valueOrder = "desc";
        scope.valueFormat = '^[0-9]+([.]{1}[0-9]{1,6})?$';
        scope.valueDataType = 0;

        return scope;
    }
}); 