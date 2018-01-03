import requests
from requests_oauthlib import OAuth1
import json

with open('config.json', 'r') as f:
    config = json.load(f)

def migrate_tokens():
    headers = { 'Content-Type': 'application/json' }
    auth = OAuth1 (
        config['OAuth1']['consumer_key'], 
        config['OAuth1']['consumer_secret'],
        config['OAuth1']['access_token'], 
        config['OAuth1']['access_secret']
    )
    body = {
        'scope':'com.intuit.quickbooks.accounting',
        'redirect_uri':'https://developer-qa.intuit.com/v2/OAuth2Playground/RedirectUrl',
        'client_id': config['OAuth2ClientId'],
        'client_secret': config['OAuth2ClientSecret']
    }
    r = requests.post(config['Migration_url'], headers=headers, auth=auth, data=json.dumps(body))
    print ('Migration API call:')
    print (r.status_code)
    print (r.json())
    return r.json()

############## OPTIONAL ################
# Test API call with OAuth2 access token
def api_call(access_token, realmId):
    url = config['Accounting_base_url'] + 'v3/company/' + realmId + '/companyinfo/' + realmId+'?minorversion=12'
    headers = { 
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer '+ access_token 
    }
    r = requests.get(url, headers=headers)
    print ('Test API call:')
    print (r.status_code)
    #print (r.headers)
    print (r.json())
    return r.json()

oauth2_tokens = migrate_tokens()
company_info = api_call(oauth2_tokens['access_token'], oauth2_tokens['realmId'], )

