(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('DemoMainController', Controller);

    Controller.$inject = ['$state'];

    function Controller($state) {
        var vm = this;

        vm.color1 = {
            render: 'class',
            className: 'brown'
        };

        vm.color2 = {
            render: 'condition',
            condition: { 'card 1': 'brown', 'card 2': 'orange', 'card 3': 'purple' }
        };

        vm.color3 = {
            render: 'linear',
            rgba: 'rgba(156, 39, 176, 1)'
        };

        vm.cards = [{
            Name: 'card 1',
            Selected: false,
            Content: 'Lorem Ipsum is simply dummy text of the printing and typesetting industry. -- by Card 1'
        }, {
            Name: 'card 2',
            Selected: true,
            Content: 'Lorem Ipsum is simply dummy text of the printing and typesetting industry. -- by Card 2'
        }, {
            Name: 'card 3',
            Selected: false,
            Content: 'Lorem Ipsum is simply dummy text of the printing and typesetting industry. -- by Card 3'
        }];


        var today = new Date();
        var fourDayLater = new Date().setDate(new Date().getDate() + 4)

        vm.data2 = {
            startDate: today
        }

        vm.data3 = {
            endDate: today
        }

        vm.data4 = {
            endDate: fourDayLater
        }

        vm.customValid = function (data) {
            return "this is custom validate";
        }

        vm.data6 = {
            startDate: today,
            endDate: fourDayLater
        }

        console.log(vm.data6)
    }
})();
