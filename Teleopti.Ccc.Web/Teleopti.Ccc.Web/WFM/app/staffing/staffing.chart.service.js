(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .service('ChartService', chartService);

    chartService.inject = ['$translate'];
    function chartService($translate) {
        this.config = staffingChartConfig;

        ////////////////
        function staffingChartConfig(data) {
            var staffingData = data;
            var staffing = generateOverUnderStaffing(data);
            var scaffold = generateScaffold(data);
            var chartColors = generateColorObject(staffing, scaffold);
            var types = generateTypeObject(data);
            var config = {
                bindto: '#staffingChart',
                point: {
                    show: false
                },
                legend: {
                    hide: [scaffold.under[0], scaffold.over[0]],
                },
                data: {
                    colors: chartColors,
                    order: 'null',
                    type: 'bar',
                    x: "x",
                    types: types,
                    columns: [
                        staffingData.time,
                        staffingData.forcastedStaffing,
                        staffingData.scheduledStaffing,
                        scaffold.over,
                        scaffold.under,
                        staffing.over,
                        staffing.under

                    ],
                    groups: [
                        [scaffold.over[0], staffing.over[0]],
                        [scaffold.under[0], staffing.under[0]]
                    ]
                },
                tooltip: {
                    contents: tooltip_contents
                },
                axis: {
                    x: {
                        label: {
                            text: $translate.instant('SkillTypeTime'),
                            position: 'outer-center'
                        },
                        type: 'category',
                        tick: {
                            culling: {
                                max: 24
                            },
                            fit: true,
                            centered: true,
                            multiline: false
                        }
                    }
                },
                zoom: {
                    enabled: false,
                },
            }
            return config;
        };

        function tooltip_contents(d, defaultTitleFormat, defaultValueFormat, color) {
            var root = this, config = root.config, CLASS = root.CLASS,
                titleFormat = config.tooltip_format_title || defaultTitleFormat,
                nameFormat = config.tooltip_format_name || function (name) { return name; },
                valueFormat = config.tooltip_format_value || defaultValueFormat,
                text, i, title, value, name, bgcolor;

            // You can access all of data like this:
            for (i = 0; i < d.length; i++) {
                if (!(d[i] && (d[i].value || d[i].value === 0))) { continue; }

                if (d[i].name === 'OverStaffScaffold' || d[i].name === 'UnderStaffScaffold') { continue; }

                if (!text) {
                    title = config.axis_x_categories[d[i].index];
                    text = "<table class='" + CLASS.tooltip + "'>" + (title || title === 0 ? "<tr><th colspan='2'>" + title + "</th></tr>" : "");
                }

                name = nameFormat(d[i].name);
                value = valueFormat(d[i].value, d[i].ratio, d[i].id, d[i].index);
                bgcolor = root.levelColor ? root.levelColor(d[i].value) : color(d[i].id);

                text += "<tr class='" + CLASS.tooltipName + "-" + d[i].id + "'>";
                text += "<td class='name'><span style='background-color:" + bgcolor + "'></span>" + name + "</td>";
                text += "<td class='value'>" + value + "</td>";
                text += "</tr>";
            }
            return text + "</table>";
        };
        function generateScaffold(staffingData) {
            var scaffold = {};
            scaffold.under = staffingData.scheduledStaffing.concat();
            scaffold.under.shift();
            scaffold.under.unshift('UnderStaffScaffold');

            scaffold.over = staffingData.forcastedStaffing.concat();
            scaffold.over.shift();
            scaffold.over.unshift('OverStaffScaffold');
            return scaffold;
        };

        function generateTypeObject(staffingData) {
            var types = {};
            var forcastingTypeKey = staffingData.forcastedStaffing[0];
            var staffingTypeKey = staffingData.scheduledStaffing[0];
            types[forcastingTypeKey] = 'line';
            types[staffingTypeKey] = 'line';
            return types;
        };

        function generateColorObject(staffingObj, scaffoldObj) {
            var colors = {};
            var overstaffColorKey = staffingObj.over[0];
            var understaffColorKey = staffingObj.under[0];
            var overScaffoldKey = scaffoldObj.over[0];
            var underScaffoldKey = scaffoldObj.under[0];

            colors[overstaffColorKey] = '#4286f4';
            colors[understaffColorKey] = '#f44141';
            colors[underScaffoldKey] = '#fff';
            colors[overScaffoldKey] = '#fff';
            return colors;
        };
        function generateOverUnderStaffing(staffingData) {
            var staffing = {};
            staffing.over = [];
            staffing.under = [];
            staffing.over.unshift($translate.instant('Overstaffing'));
            staffing.under.unshift($translate.instant('Understaffing'));

            for (var index = 0; index <= staffingData.absoluteDifference.length; index++) {
                var value = staffingData.absoluteDifference[index];
                value = parseFloat(value);
                if (value < 0) {
                    staffing.under.push(Math.abs(value.toFixed(1)));
                    staffing.over.push(0);
                } else if(value>=0){
                    staffing.over.push(value.toFixed(1));
                    staffing.under.push(0);
                }

            }

            return staffing
        }

    }
})();