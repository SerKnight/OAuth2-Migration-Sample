package com.intuit.oauthmigration;

import java.io.IOException;
import java.io.InputStream;
import java.util.Properties;

import org.slf4j.LoggerFactory;

import com.intuit.oauth2.client.OAuthMigrationClient;
import com.intuit.oauth2.config.Environment;
import com.intuit.oauth2.config.OAuth2Config;
import com.intuit.oauth2.config.Scope;
import com.intuit.oauth2.data.OAuthMigrationRequest;
import com.intuit.oauth2.data.OAuthMigrationResponse;

public class OAuth2MigrationUsingSDK {
    
	private static final org.slf4j.Logger LOG = LoggerFactory.getLogger(OAuth2MigrationUsingSDK.class);
	
   
    static Properties prop = new Properties();
	
    public static void main(String[] args) {
		try {
			//read tokens and urls from config file
			loadProperties();
			
			//call migrate API
			migrate();
		} catch (IOException e) {
			e.printStackTrace();
		}
    }
 
    public static void migrate() throws IOException {           
	    
		try {
		    
		    //build oauth2config to get client id secret
			OAuth2Config oAuth2Config = new OAuth2Config.OAuth2ConfigBuilder(prop.getProperty("clientId"), prop.getProperty("clientSecret")).buildConfig();
			
			//build migration request 
			OAuthMigrationRequest req = new OAuthMigrationRequest.OAuthMigrationRequestBuilder(Environment.SANDBOX, Scope.Accounting)
					.consumerKey(prop.getProperty("consumerKey"))
					.consumerSecret(prop.getProperty("consumerSecret"))
					.accessToken(prop.getProperty("accessToken"))
					.accessSecret(prop.getProperty("accessTokenSecret"))
					.oAuth2Config(oAuth2Config)
					.build();
			
			
			// create migration service
			OAuthMigrationClient oAuthMigrationService = new OAuthMigrationClient(req);
			OAuthMigrationResponse result = oAuthMigrationService.migrate();
			LOG.info("AccessToken" +result.getAccessToken());
		   
	        
		} catch (Exception ex) {
			LOG.error("Exception calling migrate API" + ex.getMessage());
		} 
 	         
    }

    
    private static void loadProperties() {

		InputStream input = null;
		try {

			input = OAuth2MigrationUsingSDK.class.getClassLoader().getResourceAsStream("config.properties");
			prop.load(input);

		} catch (Exception e) {
			e.printStackTrace();
		} finally {
			if (input != null) {
				try {
					input.close();
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		}

	}

   
}
