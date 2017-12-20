# OAuth2-Migration-Sample

The [Intuit Developer team](https://developer.intuit.com) has written this OAuth Migration Sample App to provide working examples on how to use the OAuth migration API to migrate your OAuth1 accesstoken and accesssecret for a given company to OAuth2 access token.

Note: This sample is not applicable for OAuth2 accounts and apps or an OAuth1 app that doesn't have any active connections.

## Prerequisites

1. Before you run the sample, make sure your developer.intuit.com account is whitelisted for migration (for alpha/beta developers only).
2. Once your account is whitelisted, make sure to login to your account and access the app that you want to be migrated. This will generate OAuth2 client id and secret for your app.

## Migration API input parameters

1. OAuth1 Tokens: The migration API requires the OAuth1 - consumerKey, consumerSecret, accessToken and accessSecret in the authorization header.
2. Scope: Depending on what scope was used for your OAuth1 tokens (eg: QuickBooks and/or Payments), pass com.intuit.quickbooks.accounting or com.intuit.quickbooks.payment or both as scope to the migration API
3. Redirect URI: Make sure the redirect URL specified in the "Redirect URI" section of your OAuth2 keys tab is the same that you will pass in the "redirect_uri" parameter of the migration API.
4. OAuth2 client id and secret: These are available in the OAuth2 keys tab of your app. These need to be sent in the POST request json along with scope and redirect url

## Samples

Samples are available in the following languages, use the readme within the individual language folder for additional instructions on how to run the sample
* [DotNet](dotnet)
* [Java](java)
* [Nodejs](nodejs)
* [PHP](php)
* [Python](python)
* [Ruby](ruby)
* [Postman](postman)






