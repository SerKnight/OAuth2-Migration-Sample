import requests
from requests_oauthlib import OAuth1
import json

MIGRATION_API = 'https://developer-qal.api.intuit.com/v2/oauth2/tokens/migrate'
ACCOUNTING_BASE_URL = 'https://quickbooks2-qal.api.intuit.com/'

oauth1_tokens = {
    'consumer_key': 'qye2essx8EYKufegczOgby02Dstn7l',
    'consumer_secret': 'D0MXq6GvSN1ZYzGTessdxkJBxvNXluZ258N1eMic',
    'access_token': 'qye2e7pvykSYHZXuQO0oNd5hjaRLwyYjMiuIPic1xm1nmX4n',
    'access_secret': 'Y18v9NbsdovBU8HgWBnA5HtL8bydxqNeCjDV8o5h',
    'realm_id': 123146599700169
}

def migrate_tokens(tokens):
    url = MIGRATION_API
    headers = { 'Content-Type': 'application/json' }
    auth = OAuth1 (
                tokens['consumer_key'], 
                tokens['consumer_secret'],
                tokens['access_token'], 
                tokens['access_secret']
            )
    body = {
            'scope':'com.intuit.quickbooks.accounting',
            'redirect_uri':'https://developer-qa.intuit.com/v2/OAuth2Playground/RedirectUrl'
        }
    r = requests.post(url, headers=headers, auth=auth, data=json.dumps(body))
    print ('Migration API call:')
    print (r.status_code)
    print (r.json())
    return r.json()

############## OPTIONAL ################
# Test API call with OAuth2 access token
def api_call(oauth2_tokens):
    url = ACCOUNTING_BASE_URL + 'v3/company/' + oauth2_tokens['realmId'] + '/companyinfo/' + oauth2_tokens['realmId']
    headers = { 
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': 'Bearer '+ oauth2_tokens['access_token'] 
            }
    r = requests.get(url, headers=headers)
    print ('Test API call:')
    print (r.status_code)
    #print (r.headers)
    print (r.json())

    return r.json()

oauth2_tokens = migrate_tokens(oauth1_tokens)
oauth2_refresh_token = oauth2_tokens['refresh_token']
oauth2_access_token = oauth2_tokens['access_token']

company_info = api_call(oauth2_tokens)

