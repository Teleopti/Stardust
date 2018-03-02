describe('badgeCalculation', function () {
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

    it('should render badge calculation page', function () {
        var cmp = setupComponent(testScope());
        var startBtn = cmp[0].querySelector('button.wfm-btn.ng-binding');
        expect(startBtn).not.toBe(null);
    });

    function setupComponent(scope) {
        var el;

        var template = '<badge-calculation></badge-calculation>';

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
        return scope;
    }
}); 