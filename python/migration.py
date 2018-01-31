import requests
from requests_oauthlib import OAuth1
import json
from QBO_request import api_call

with open('config.json', 'r') as f:
    config = json.load(f)

def migrate_tokens():
    '''Migrate tokens from OAuth1 to OAuth2'''
    headers = { 'Content-Type': 'application/json' }
    auth = OAuth1 (
        config['OAuth1']['consumer_key'], 
        config['OAuth1']['consumer_secret'],
        config['OAuth1']['access_token'], 
        config['OAuth1']['access_secret']
    )
    body = {
        'scope':'com.intuit.quickbooks.accounting',
        'redirect_uri': config['Redirect_url'],
        'client_id': config['OAuth2ClientId'],
        'client_secret': config['OAuth2ClientSecret']
    }
    r = requests.post(config['Migration_url'], headers=headers, auth=auth, data=json.dumps(body))
    print ('Migration API call:')
    print ('Status code: '+str(r.status_code))
    print (r.json())
    return r.json()

oauth2_tokens = migrate_tokens()

# Optionally, make a sample QBO API request
company_info = api_call(oauth2_tokens['access_token'], oauth2_tokens['realmId'])

