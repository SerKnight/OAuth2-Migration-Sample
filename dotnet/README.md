# OAuth2-Migration-Sample

##### This sample uses .Net framework

### Configuration
Update [`Web.config`](OAuth1toOAuth2Migration\OAuth1toOAuth2Migration\OAuth1toOAuth2Migration\Web.config) to add app token, OAuth1 consumerkey, consumersecret OAuth2 clientId, clientSecret and a valid filepath for logging.

### Note: This sample has two projects, one that demonstrates migration from OAuth1 to OAuth2 tokens standalone and the other which uses .Net SDK

### Steps
1.Build solution
2.Put a break point on Page_Load method for the project you're running
3.Run solution
4.Go through the OAuth1 flow first to get OAuth1 Access token, Access token secret and realm id
5.Step through the code to get OAuth2 Access and Refresh tokens

