require 'oauth/request_proxy/typhoeus_request'
require 'oauth'
require 'json'
require 'yaml'
require_relative 'QBO_request'

def migrate_tokens()
	@consumer = OAuth::Consumer.new(
		CONFIG['consumer_key'],
		CONFIG['consumer_secret'],
	)
	@token = OAuth::Token.new(
		CONFIG['access_token'], 
		CONFIG['access_secret']
	)
	
	options = {
		method: :post,
		headers: { 'Content-Type' => 'application/json' },
		body: {
        	scope:'com.intuit.quickbooks.accounting',
        	redirect_uri: CONFIG['redirect_uri'],
        	client_id: CONFIG['oauth2_client_id'],
        	client_secret: CONFIG['oauth2_client_secret']

    	}.to_json
	}

	oauth_params = {:consumer => @consumer, :token => @token}

	req = Typhoeus::Request.new(CONFIG['migration_url'], options)
	oauth_helper = OAuth::Client::Helper.new(req, oauth_params.merge(:request_uri => CONFIG['migration_url']))
	req.options[:headers].merge!({
		'Authorization' => oauth_helper.header,
    })
	req.run
	response = req.response
	puts 'Migrate API call'
	puts response.code 
	puts response.headers 
	puts response.body 
	JSON.parse(response.body)
end

oauth2_tokens = migrate_tokens()

# Optionally, make a sample QBO API request
api_call(oauth2_tokens['access_token'], oauth2_tokens['realmId'])