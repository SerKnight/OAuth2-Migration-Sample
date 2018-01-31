const config = require('./config');
const axios = require('axios');

// Make a QBO API request
module.exports = function (accessToken, realmId) {
    const httpMethod = 'GET';
    let url = config.Accounting_base_url;
    url +=  'v3/company/'+ realmId +'/companyinfo/'+ realmId +'?minorversion=12';
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
