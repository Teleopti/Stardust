describe('quick forecasting feature', function() {
    var ptor;

    beforeEach(function() {
        browser.get('http://localhost:63342/WFM/index2.html#/forecasting');
        ptor = protractor.getInstance();
    });

    it('should create a new period', function() {

        element(by.css('.start-forecasting')).click();

        expect(ptor.isElementPresent(by.css('.forecasting-form'))).toBe(false);
    });
});