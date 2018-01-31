const axios = require('axios');
const generateOauth1Sign = require('./OAuth1Helper.js');
const makeAPICall = require('./QBORequest.js');
const config = require('./config');

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
        'redirect_uri':config.Redirect_url,
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
