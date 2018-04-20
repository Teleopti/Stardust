WFM Web
================
Overview of project files.

* app - angularjs code incl. routes
* css - styles for angularjs
* e2e - end-to-end tests with protractor
* src - Angular code
* .browserlistrc - tells tools like autoprefixer, babel etc. about which browsers to support


# Protractor (e2e/)
1. Upgrade chromedrivers
    * `npx webdriver-manager update`
2. Start selenium server
    * `npx webdriver-manager start`
3. Start IIS
    * `npm run iis`
4. Run protractor tests
    * Run all tests
        - `npx protractor e2e/protractor.conf.js`
    * Run module tests
        - `npx protractor e2e/people/protractor.conf.js`
    * Run single spec
        - `npx protractor e2e/protractor.conf.js --specs e2e/people/search.e2e-spec.ts`

