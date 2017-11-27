package com.intuit.oauthmigration;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.Properties;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClientBuilder;
import org.json.JSONObject;

import oauth.signpost.OAuthConsumer;
import oauth.signpost.commonshttp.CommonsHttpOAuthConsumer;

public class OAuth2Migration {
    
    private static final CloseableHttpClient CLIENT = HttpClientBuilder.create().build();
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
		    	
			//create oauth consumer using tokens
		    OAuthConsumer consumer = new CommonsHttpOAuthConsumer(prop.getProperty("consumerKey"), prop.getProperty("consumerSecret"));
			consumer.setTokenWithSecret(prop.getProperty("accessToken"), prop.getProperty("accessTokenSecret"));
	
		    HttpPost post = new HttpPost(prop.getProperty("hostUrl"));
			    
			//sign
		    consumer.sign(post);
		    post.setHeader("Accept", "application/json");
		    post.setHeader("Content-Type", "application/json");
		    
		    // add post data
		    String json =  new JSONObject().put("scope","com.intuit.quickbooks.accounting").put("redirect_uri", prop.getProperty("redirectUrl")).toString();
		    HttpEntity entity = new StringEntity(json, "UTF-8");
		    post.setEntity(entity);
		    
		    HttpResponse response = CLIENT.execute(post);
		    
		    //print response
		    System.out.println(response.getStatusLine().toString());
		    System.out.println(getResult(response));
	        
		} catch (Exception ex) {
			ex.printStackTrace();
		} 
 	         
    }
    
    public static StringBuffer getResult(HttpResponse response) throws IOException {
		BufferedReader rd = new BufferedReader(new InputStreamReader(response.getEntity().getContent()));
		StringBuffer result = new StringBuffer();
		String line = "";
		while ((line = rd.readLine()) != null) {
		    result.append(line);
		}
		return result;
	}
    
    private static void loadProperties() {

		InputStream input = null;
		try {

			input = OAuth2Migration.class.getClassLoader().getResourceAsStream("config.properties");
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
