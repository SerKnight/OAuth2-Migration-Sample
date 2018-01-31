const oauthSignature = require('oauth-signature');
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

// Generate OAuth1 signature
module.exports = function (httpMethod, url) {
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