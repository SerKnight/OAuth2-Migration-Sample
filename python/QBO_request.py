import requests
import json

with open('config.json', 'r') as f:
        config = json.load(f)

def api_call(access_token, realmId):
    '''Test a GET API request with OAuth2 access token'''
    url = config['Accounting_base_url'] + 'v3/company/' + realmId + '/companyinfo/' + realmId+'?minorversion=12'
    headers = { 
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'Authorization': 'Bearer '+ access_token 
    }
    r = requests.get(url, headers=headers)
    print ('Test API call:')
    print ('Status code: '+str(r.status_code))
    #print (r.headers)
    print (r.json())
    return r.json()