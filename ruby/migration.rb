require 'oauth/request_proxy/typhoeus_request'
require 'oauth'
require 'json'
require 'yaml'

CONFIG = YAML.load_file("config.yml")

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
        	redirect_uri:'https://developer-qa.intuit.com/v2/OAuth2Playground/RedirectUrl'
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

def api_call(access_token, realm_id)
	url =  CONFIG['accounting_base_url'] + 'v3/company/' + realm_id + '/companyinfo/' + realm_id+'?minorversion=12'
	options = {
		method: :get,
		headers: { 
			'Content-Type' => 'application/json',
			'Accept' => 'application/json',
			'Authorization' => 'Bearer ' + access_token
		}
	}
	req = Typhoeus::Request.new(url, options)
	req.run
	response = req.response
	puts 'Test API call'
	puts response.code 
	puts response.headers 
	puts response.body 
	JSON.parse(response.body)
end

oauth2_tokens = migrate_tokens()
api_call(oauth2_tokens['access_token'], oauth2_tokens['realmId'])