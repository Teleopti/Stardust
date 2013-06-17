define(
    [
        'moment'
    ], function (
        momentX
    ) {

        return {
            Date: {
                ToMoment: function (date) {
                    if (moment.isMoment(date))
                        return date;
                    // "D" is added by the message broker
                    if (date.substring(0, 1) == "D")
                        return moment(date.substring(1));
                    return moment(date);
                }
            }
        };

    });
