(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .service('skillPrioService', skillPrioService);

    //Service.$inject = ['dependency1'];
    function skillPrioService() {
        this.getMockActivitys = getMockActivitys;
        this.getMockSkills = getMockSkills;

        ////////////////
        function getMockActivitys() {
            var activitys = [{
                name: 'Phone'
            }, {
                    name: 'Backoffice'
                }, {
                    name: 'Webchat'
                }, {
                    name: 'Store'
                }, {
                    name: 'Email'
                }, {
                    name: 'Social Media'
                },];
            return activitys
        };
        function getMockSkills() {
            var skills = [{
                name: "English",
                value: 0,
                siblings: []
            }, {
                    name: "Norwegian",
                    value: 0,
                    siblings: []
                }, {
                    name: "Swedish",
                    value: 0,
                    siblings: []
                }, {
                    name: "Finnish",
                    value: 0,
                    siblings: []
                }, {
                    name: "Danish",
                    value: 0,
                    siblings: []
                }, {
                    name: "Klingon",
                    value: 0,
                    siblings: []
                }];
            return skills
        };
    }
})();