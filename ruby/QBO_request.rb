require 'oauth/request_proxy/typhoeus_request'
require 'json'
require 'yaml'

CONFIG = YAML.load_file("config.yml")

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