import { ApiAccessToken, ExternalApplication } from '../../types';

export interface GrantBot {
	Id: 2;
	Name: 'GrantBot';
}
export interface StaffHub {
	Id: 3;
	Name: 'StaffHub';
}

export const grantBot: GrantBot = { Id: 2, Name: 'GrantBot' };
export const staffHub: StaffHub = { Id: 3, Name: 'StaffHub' };

export const APPLICATIONS: Array<ExternalApplication> = [grantBot, staffHub];
export const TOKEN: ApiAccessToken = { Token: '82470rh489y9y980' };
