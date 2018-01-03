const oauthSignature = require('oauth-signature');
const axios = require('axios');
const config = require('./config');

const timestamp = Math.round(new Date().getTime()/1000);
const parameters = {
    oauth_consumer_key : config.OAuth1.consumer_key,
    oauth_token : config.OAuth1.access_token,
    oauth_signature_method : 'HMAC-SHA1',
    oauth_timestamp : timestamp,
    oauth_nonce : 'nonce',
    oauth_version : '1.0'
};

function migrateTokens(oauth1Tokens) {
    const httpMethod = 'POST';
    authHeader = generateOauth1Sign(httpMethod, config.Migration_url);
    const headers = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'OAuth ' + authHeader,  
        }
    };
    const body = {
        'scope':'com.intuit.quickbooks.accounting',
        'redirect_uri':'https://developer-qa.intuit.com/v2/OAuth2Playground/RedirectUrl',
        'client_id': config.OAuth2ClientId,
        'client_secret': config.OAuth2ClientSecret
    };
    axios.post(config.Migration_url, body, headers).then ( function (response) {
        console.log(response.status);
        console.log(JSON.stringify(response.headers));
        console.log(JSON.stringify(response.data));
        makeAPICall(response.data.access_token, response.data.realmId);
    }).catch(function (error) {
        console.log(error.response.status);
        console.log(error.response.data);
        console.log(error.response.headers);
    });
}

migrateTokens()

function generateOauth1Sign(httpMethod, url) {
    const encodedSignature = oauthSignature.generate (httpMethod, url, parameters, config.OAuth1.consumer_secret, config.OAuth1.access_secret);
    parameters ['oauth_signature'] = encodedSignature;
    const keys = Object.keys(parameters);
    let authHeader = '';
    for (key in parameters) {
        // Add this for Accounting API minorversion url query parameter
        if (key === 'minorversion') {
            continue;
        }
        if (key === keys[keys.length-1]) {
            authHeader += key + '=' + '"'+parameters[key]+'"';
        } 
        else {
            authHeader += key + '=' + '"'+parameters[key]+'",';
        }
    }
    return authHeader;
}

function makeAPICall(accessToken, realmId) {
    const httpMethod = 'GET';
    let url = config.Accounting_base_url;
    url +=  'v3/company/'+ realmId +'/companyinfo/'+ realmId +'?minorversion=12';
    authHeader = generateOauth1Sign(httpMethod, url);
    const headers = {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + accessToken  
        }
    };
    axios.get(url, headers).then ( function (response) {
        console.log(response.status);
        console.log(JSON.stringify(response.headers));
        console.log(JSON.stringify(response.data));
    }).catch(function (error) {
        console.log(error.response.status);
        console.log(error.response.data);
        console.log(error.response.headers);
    });
}
