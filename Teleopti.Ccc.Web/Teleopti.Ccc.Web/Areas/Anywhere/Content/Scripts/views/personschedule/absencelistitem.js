define([
        'knockout',
        'moment',
        'navigation',
        'resources',
        'ajax'
    ], function(
        ko,
        moment,
        navigation,
        resources,
        ajax
    ) {

        return function(data) {

            var self = this;
            
            this.StartTime = ko.observable(moment(data.StartTime).format(resources.DateTimeFormatForMoment));
            this.EndTime = ko.observable(moment(data.EndTime).format(resources.DateTimeFormatForMoment));
            this.Name = ko.observable(data.Name);
            this.BackgroundColor = ko.observable(data.Color);
            
            this.AboutToRemove = ko.observable(false);
            this.Removing = ko.observable(false);
            
            this.Remove = function() {
                self.AboutToRemove(true);
            };

            this.ConfirmRemoval = function () {
                self.Removing(true);
                ajax.ajax(
                    {
                        url: 'PersonScheduleCommand/RemovePersonAbsence',
                        type: 'POST',
                        data: JSON.stringify({
                            PersonAbsenceId: data.Id
                        }),
                        success: function (responseData, textStatus, jqXHR) {
                        	navigation.GoToTeamSchedule(data.GroupId, data.Date);
                        }
                    }
                );
            };
        };
    });
