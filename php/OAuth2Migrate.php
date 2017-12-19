<?php

require ("./CurlClient.php");
require ("./OAuth1.php");

$opts = "Empty";
$longOpts = [
  "consumerkey:",
  "consumersecret:",
  "accesstoken:",
  "accesstokensecret:",
  "scope:",
  "clientid:",
  "clientsecret:"
];
$baseURL = "https://developer.api.intuit.com/v2/oauth2/tokens/migrate";
$options = getopt($opts, $longOpts);
if(!isset($options["consumerkey"]) || !isset($options["consumersecret"]) || !isset($options["accesstoken"]) || !isset($options["accesstokensecret"]) || !isset($options["scope"]) || !isset($options["clientid"]) || !isset($options["clientsecret"])){
    throw new \Exception("Required value missing.");
}
$oauth1 = new OAuth1($options["consumerkey"], $options["consumersecret"], $options["accesstoken"], $options["accesstokensecret"]);
$CurlHttpClient = new CurlClient();

$parameters = array(
    'scope' => $options["scope"],
    'redirect_uri' => "https://developer.intuit.com/v2/OAuth2Playground/RedirectUrl",
    'client_id' => $options["clientid"],
    'client_secret' => $options["clientsecret"]
);

$authorizationHeaderInfo = $oauth1->getOAuthHeader($baseURL, array(), "POST");
$http_header = array(
      'Accept' => 'application/json',
      'Authorization' => $authorizationHeaderInfo,
      'Content-Type' => 'application/json'
);

$intuitResponse = $CurlHttpClient->makeAPICall($baseURL, "POST", $http_header, json_encode($parameters), null, false);
var_dump($intuitResponse);
//$json_string = json_encode($intuitResponse, JSON_PRETTY_PRINT);
//echo $json_string;


?>
