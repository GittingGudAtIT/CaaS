import { AuthConfig } from 'angular-oauth2-oidc';

// INTROSPECTION
// with Keycloak 11.0.2
// siehe https://www.keycloak.org/docs/latest/securing_apps/index.html#endpoints-2

export const authConfig: AuthConfig = {
  issuer: 'http://localhost:8080/auth/realms/wea5',
  loginUrl: 'http://localhost:8080/auth/realms/wea5/protocol/openid-connect/auth',
  logoutUrl: 'http://localhost:8080/realms/wea5/protocol/openid-connect/logout',
  tokenEndpoint: 'http://localhost:8080/auth/realms/wea5/protocol/openid-connect/token',
  sessionCheckIFrameUrl: 'http://localhost:8080/auth/realms/wea5/protocol/openid-connect/login-status-iframe.html',
  userinfoEndpoint: 'http://localhost:8080/auth/realms/wea5/protocol/openid-connect/userinfo',
  clientId: 'wea5-demo',
  redirectUri: window.location.origin + '/login',
  silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
  scope: 'profile email',
  silentRefreshTimeout: 5000, // For faster testing
  timeoutFactor: 0.25, // For faster testing
  sessionChecksEnabled: true,
  showDebugInformation: true, // Also requires enabling "Verbose" level in devtools
  clearHashAfterLogin: false, // https://github.com/manfredsteyer/angular-oauth2-oidc/issues/457#issuecomment-431807040
};
