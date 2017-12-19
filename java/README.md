# OAuth2-Migration-Sample

## Running the code
1. Import the gradle project in a IDE of your choice
2. Update [`config.properties`](src/main/resources/config.properties) to add OAuth1 consumerkey, consumersecret, accesstoken and accesssecret and OAuth2 clientId, clientSecret
3. Verify the post body in [`OAuth2Migration.java`](src/main/java/com/intuit/oauthmigration/OAuth2Migration.java) to make sure the correct scope is added.
4. Run the main method of OAuth2Migration.java
