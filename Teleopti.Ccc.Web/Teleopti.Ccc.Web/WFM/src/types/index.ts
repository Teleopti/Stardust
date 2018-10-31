import { IStateProvider, IUrlRouterProvider } from 'angular-ui-router';

export interface DowngradeableComponent {
	ng1Name: string;
	ng2Component: any;
}

export type RouterConfigFunction = ($stateProvider: IStateProvider, $urlRouterProvider: IUrlRouterProvider) => void;
