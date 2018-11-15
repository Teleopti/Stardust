# Teleopti WFM Web

## Gotcha's

-   The project uses a 32-bit version of Node.js. If you want to use your own local version of Node.JS make sure it matches with the one in the project. See _Scripts for frontend development_ for how to use the projects Node.JS env.

-   To install dependencies use `npm ci` instead of `npm install`. If updating deps then use `npm install`.

-   Remember to **commit your `package-lock.json`** file

## Overview of project files

| Folder | Comment                                          |
| ------ | ------------------------------------------------ |
| app    | angularjs code                                   |
| css    | styles mostly for angularjs                      |
| html   | templates for angularjs                          |
| src    | angular root                                     |
| test   | karma configs                                    |
| vendor | vendor dependencies (prefer npm package install) |

## Scripts for frontend development

| Script                    | What                                  |
| ------------------------- | ------------------------------------- |
| `..\.node\UseNodeEnv.ps1` | Use build server Node.js env          |
| `npm run test`            | run all tests                         |
| `npm run iis:web`         | start the iis for web project         |
| `npm run iis:admin`       | start the iis for admin portal        |
| `npm run dev:watch`       | Watch and build the entire frontend   |
| `npm run dev:build`       | Build the entire frontend in dev env  |
| `npm run prod:build`      | Build the entire frontend in prod env |

## Angular

-   What is core module?
    -   Services and configs that needs to be available to all modules in the project is stored here.
-   What is shared?
    -   Some argue for the use of shared for _"stuff that is sometimes needed"_ but this provides no specificity and could amount to a mess. Avoid putting things in the global shared folder, break them out into modules instead.

### Module structure

| Folder / File | Comment                                         |
| ------------- | ----------------------------------------------- |
| pages         | for "components" mounted directly on the router |
| components    | used for composing pages                        |
| shared        | services shared across the module               |

Our Angular modules can export an array of `DowngradeableComponent` to be used within angularjs. For example we could export those components and then downgrade them in `main.ts` with `downgradeHelper()`:

```ts
// in my.module.ts
export const myComponents: DowngradeableComponent[] = [
	// list of components
];

// in main.ts
import { myComponents } from './my.module';
downgradeHelper(myComponents);
```

If our Angular module does not have any angularjs with it we can mount our routes in the following way:

```ts
// in my.module.ts
export function mymoduleRouterConfig($stateProvider: IStateProvider, $urlRouterProvider: IUrlRouterProvider) {
	// Configure your routes here
}

// in main.ts
import { mymoduleRouterConfig } from './my.module';
routerHelper(mymoduleRouterConfig);
```

### Testing

-   Take a look at [the Angular docs on testing](https://angular.io/guide/testing)
-   Make sure to test DOM interaction
-   Isolate your tests from dependent components & services
    -   Use Mocks and Spies
-   Avoid fake backends and global dependencies

#### Running tests

-   Rename `describe` to `fdescribe` to run a spec file.
-   Rename `it` to `fit` to run a single spec.
-   Rename `it` to `xit` to ignore a single spec. _(Avoid this)_

#### Mocks

We have application-wide mocks defined in `src/mocks/*` and these can be imported from `@wfm/mocks`.

When mocking services you can use `Partial` to implement part of a service:

```ts
class MockAreaService implements Partial<AreaService> {
	getAreas(): Observable<Area[]> {
		return of([]);
	}
}
```

#### Examples

-   `src\app\menu\components\side-menu\side-menu.component.spec.ts`
    -   Uses Mocks for services and PageObject for DOM interaction
