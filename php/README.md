# OAuth2-Migration-Sample
Run the following command:
php OAuth2Migrate.php --consumerkey YOUR_CONSUMER_KEY --consumersecret YOUR_CONSUMER_SECRET --accesstoken YOUR_ACCESS_TOKEN --accesstokensecret YOUR_TOKEN_SECRET --scope THE_SCOPE --clientid YOUR_OAUTH2CLIENTID --clientsecret YOUR_OAUTH2CLIENTSECRET

Note:
1. Make sure the redirect URL specified in the "Redirect URI" section of your OAuth2 keys tab is the same that you will pass in the "redirect_uri" parameter of the migration API.