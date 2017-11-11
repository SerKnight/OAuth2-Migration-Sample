using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using System.IO;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Configuration;

namespace OAuth1toOAuth2Migration
{
    public partial class MigrateTokens : System.Web.UI.Page
    {
        public string migrateUrl = ConfigurationManager.AppSettings["migrateUrl"];
        public string qboBaseUrl = ConfigurationManager.AppSettings["qboBaseUrl"];
        //public string clientID = ConfigurationManager.AppSettings["clientID"];
        //public string clientSecret = ConfigurationManager.AppSettings["clientSecret"];

        public string logPath = ConfigurationManager.AppSettings["logPath"];
        string accessTokenOAuth2 = "";
        string refreshTokenOAuth2 = "";
        string realmId = "";


        protected void Page_Load(object sender, EventArgs e)
        {
            realmId = HttpContext.Current.Session["realm"].ToString();
            string accessToken = HttpContext.Current.Session["accessToken"].ToString();
            string accessTokenSecret = HttpContext.Current.Session["accessTokenSecret"].ToString();
            string consumerKey = HttpContext.Current.Session["consumerKey"].ToString();
            string consumerSecret = HttpContext.Current.Session["consumerSecret"].ToString();

            MigrateTokensOAuth1ToOAuth2(consumerKey, consumerSecret, accessToken, accessTokenSecret, realmId);
           
        }

        /// <summary>
        /// Migrate OAuth1 tokens to OAuth2
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        /// <param name="realmId"></param>
        public void MigrateTokensOAuth1ToOAuth2(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string realmId)
        {
            try
            {
                string uri = migrateUrl;
                HttpWebRequest httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentType = "application/json";
                string oAuth1header = GetDevDefinedOAuthHeader(consumerKey, consumerSecret, accessToken, accessTokenSecret, httpWebRequest, null);
                
                //httpWebRequest.Headers.Add("Authorization", oAuth1header);
                httpWebRequest.Headers[HttpRequestHeader.Authorization] = oAuth1header;
                
                string requestBody ="{\"scope\":\"com.intuit.quickbooks.accounting\",\"redirect_uri\":\"https://developer-qa.intuit.com/v2/OAuth2Playground/RedirectUrl\"}";
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] content = encoding.GetBytes(requestBody.ToString());
                using (var stream = httpWebRequest.GetRequestStream())
                {
                    stream.Write(content, 0, content.Length);
                }


                HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;

                using (Stream data = httpWebResponse.GetResponseStream())
                {
                    if (httpWebResponse.StatusCode == HttpStatusCode.OK)//200
                    {

                        output("OAuth1 tokens successfully migrated to OAuth2");
                        //return response
                        string OAuth2Tokens= new StreamReader(data).ReadToEnd();

                        Dictionary<string, string> oAuth2TokenDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(OAuth2Tokens);

                        if (oAuth2TokenDecoded.ContainsKey("refresh_token"))
                        {
                            //save the refresh token in persistent store so that it can be used to refresh short lived access tokens
                            output("Refresh token for OAuth2 obtained.");
                            refreshTokenOAuth2 = oAuth2TokenDecoded["refresh_token"];
                            Session["refreshTokenOAuth2"] = refreshTokenOAuth2;


                            if (oAuth2TokenDecoded.ContainsKey("access_token"))
                            {
                                output("Access token for OAuth2 obtained.");
                                accessTokenOAuth2 = oAuth2TokenDecoded["access_token"];
                                Session["accessTokenOAuth2"] = accessTokenOAuth2;

                                //Deserialize json and get OAuth2 tokens
                                TestQBOCallUsingOAuth2Token(accessTokenOAuth2, refreshTokenOAuth2, realmId);
                            }
                        }

                    }
                    else if (httpWebResponse.StatusCode == HttpStatusCode.BadRequest)//400
                    {
                        output("Migration unsuccessful");
                        string OAuth2TokensError = new StreamReader(data).ReadToEnd(); new StreamReader(data).ReadToEnd();
                        Dictionary<string, string> oAuth2TokenDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(OAuth2TokensError);

                    }
                }
                
            }
            catch (WebException ex)
            {
                Session.Clear();
                Session.Abandon();
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {

                        output("HTTP Status: " + response.StatusCode);
                        var exceptionDetail = response.GetResponseHeader("WWW-Authenticate");
                        if (exceptionDetail != null && exceptionDetail != "")
                        {
                            output(exceptionDetail);
                            
                        }
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // read response body
                            string responseText = reader.ReadToEnd();
                            if (responseText != null && responseText != "")
                            {
                                output(responseText);
                                
                            }
                        }
                    }

                }
                
            }

        }

        /// <summary>
        /// Get OAuth1 header
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        /// <param name="webRequest"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        private string GetDevDefinedOAuthHeader(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, HttpWebRequest webRequest, string requestBody)
        {
            OAuthConsumerContext consumerContext = new OAuthConsumerContext
            {
                ConsumerKey = consumerKey,
                SignatureMethod = SignatureMethod.HmacSha1,
                ConsumerSecret = consumerSecret,
                UseHeaderForOAuthParameters = true
            };
            //We already have OAuth tokens, so OAuth URIs below are not used - set to example.com
            OAuthSession oSession = new OAuthSession(consumerContext, "https://www.example.com",
                                    "https://www.example.com",
                                    "https://www.example.com");
            oSession.AccessToken = new TokenBase
            {
                Token = accessToken,
                ConsumerKey = consumerKey,
                TokenSecret = accessTokenSecret
            };
            IConsumerRequest consumerRequest = oSession.Request();
            consumerRequest = ConsumerRequestExtensions.ForMethod(consumerRequest, webRequest.Method);
            if (requestBody != null)
            {
                consumerRequest = consumerRequest.Post().WithRawContentType(webRequest.ContentType).WithRawContent(System.Text.Encoding.ASCII.GetBytes(requestBody));
            }
            consumerRequest = ConsumerRequestExtensions.ForUri(consumerRequest, webRequest.RequestUri);
            consumerRequest = consumerRequest.SignWithToken();
            return consumerRequest.Context.GenerateOAuthParametersForHeader();
        }

        /// <summary>
        /// Test QBO call using migrated Oauth2 tokens
        /// </summary>
        /// <param name="accessTokenOAuth2"></param>
        /// <param name="refreshTokenOAuth2"></param>
        /// <param name="realmId"></param>
        public void TestQBOCallUsingOAuth2Token(string accessTokenOAuth2, string refreshTokenOAuth2, string realmId)
        {
            try
            {
                if (realmId != "")
                {
                    string query = "select * from CompanyInfo";
                    // build the  request
                    string encodedQuery = WebUtility.UrlEncode(query);

                    //add qbobase url and query
                    //string uri = string.Format("https://{0}/v3/company/{1}/query?query={2}", qboBaseUrl, realmId, encodedQuery);
                    string uri = string.Format("https://{0}/v3/company/{1}/reports/JournalReportFR?journal_code=VT", qboBaseUrl, realmId);

                    // send the request
                    HttpWebRequest qboApiRequest = (HttpWebRequest)WebRequest.Create(uri);
                    qboApiRequest.Method = "GET";
                    qboApiRequest.Headers.Add(string.Format("Authorization: Bearer {0}", accessTokenOAuth2));
                    qboApiRequest.ContentType = "application/json;charset=UTF-8";
                    qboApiRequest.Accept = "*/*";


                    // get the response
                    HttpWebResponse qboApiResponse = (HttpWebResponse)qboApiRequest.GetResponse();
                    if (qboApiResponse.StatusCode == HttpStatusCode.Unauthorized)//401
                    {

                        //if you get a 401 token expiry then perform token refresh as token is expired/ivnalid
                        //performRefreshToken(refreshTokenOAuth2);

                        //Retry QBO API call again with new tokens
                        if (Session["accessTokenOAuth2"] != null && Session["refreshTokenOauth2"] != null && Session["realmId"] != null)
                        {
                            TestQBOCallUsingOAuth2Token(Session["accessTokenOAuth2"].ToString(), Session["accessTokenOAuth2"].ToString(), Session["realmId"].ToString());
                        }


                    }
                    else
                    {
                        //read qbo api response
                        using (var qboApiReader = new StreamReader(qboApiResponse.GetResponseStream()))
                        {
                            //QBO call success
                            string responseText = qboApiReader.ReadToEnd();



                        }

                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {

                        output("HTTP Status: " + response.StatusCode);
                        var exceptionDetail = response.GetResponseHeader("WWW-Authenticate");
                        if (exceptionDetail != null && exceptionDetail != "")
                        {
                            output(exceptionDetail);
                        }
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // read response body
                            string responseText = reader.ReadToEnd();
                            if (responseText != null && responseText != "")
                            {
                                output(responseText);
                            }
                        }
                    }

                }
            }

        }


        /// <summary>
        /// Function to log app related logs
        /// </summary>
        /// <param name="logMsg"></param>
        public void output(string logMsg)
        {
            //Console.WriteLine(logMsg);

            System.IO.StreamWriter sw = System.IO.File.AppendText(GetLogPath() + "OAuth2SampleAppLogs.txt");
            try
            {
                string logLine = System.String.Format(
                    "{0:G}: {1}.", System.DateTime.Now, logMsg);
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }


        public string GetLogPath()
        {


            try
            {
                if (logPath == "")
                {
                    logPath = System.Environment.GetEnvironmentVariable("TEMP");
                    if (!logPath.EndsWith("\\")) logPath += "\\";
                }
            }
            catch
            {
                output("Log error path not found.");
            }

            return logPath;



        }

        #region code to be used if Refresh token logic needs to be implemented
        //private void performRefreshToken(string refreshTokenOAuth2)
        //{
        //    output("Exchanging refresh token for access token.");//refresh token is valid for 100days but refreshes every 24hrs and access token for 1hr
        //    string tokenEndpoint = "";


        //    string accessTokenOAuth2 = "";
        //    string cred = string.Format("{0}:{1}", clientID, clientSecret);
        //    string enc = Convert.ToBase64String(Encoding.ASCII.GetBytes(cred));
        //    string basicAuth = string.Format("{0} {1}", "Basic", enc);

        //    // build the  request
        //    string refreshtokenRequestBody = string.Format("grant_type=refresh_token&refresh_token={0}",
        //        refreshTokenOAuth2
        //        );

        //    // send the Refresh Token request
        //    HttpWebRequest refreshtokenRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
        //    refreshtokenRequest.Method = "POST";
        //    refreshtokenRequest.ContentType = "application/x-www-form-urlencoded";
        //    refreshtokenRequest.Accept = "application/json";
        //    //Adding Authorization header
        //    refreshtokenRequest.Headers[HttpRequestHeader.Authorization] = basicAuth;

        //    byte[] _byteVersion = Encoding.ASCII.GetBytes(refreshtokenRequestBody);
        //    refreshtokenRequest.ContentLength = _byteVersion.Length;
        //    Stream stream = refreshtokenRequest.GetRequestStream();
        //    stream.Write(_byteVersion, 0, _byteVersion.Length);
        //    stream.Close();

        //    try
        //    {
        //        //get response
        //        HttpWebResponse refreshtokenResponse = (HttpWebResponse)refreshtokenRequest.GetResponse();
        //        using (var refreshTokenReader = new StreamReader(refreshtokenResponse.GetResponseStream()))
        //        {
        //            //read response
        //            string responseText = refreshTokenReader.ReadToEnd();

        //            // decode response
        //            Dictionary<string, string> refreshtokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

        //            if (refreshtokenEndpointDecoded.ContainsKey("error"))
        //            {
        //                // Check for errors.
        //                if (refreshtokenEndpointDecoded["error"] != null)
        //                {
        //                    output(String.Format("OAuth token refresh error: {0}.", refreshtokenEndpointDecoded["error"]));
        //                    return;
        //                }
        //            }
        //            else
        //            {
        //                //if no error
        //                if (refreshtokenEndpointDecoded.ContainsKey("refresh_token"))
        //                {

        //                    refreshTokenOAuth2 = refreshtokenEndpointDecoded["refresh_token"];
        //                    Session["refreshTokenOAuth2"] = refreshTokenOAuth2;


        //                    if (refreshtokenEndpointDecoded.ContainsKey("accessTokenOAuth2"))
        //                    {
        //                        //save both refresh token and new access token in permanent store
        //                        accessTokenOAuth2 = refreshtokenEndpointDecoded["accessTokenOAuth2"];
        //                        Session["accessTokenOAuth2"] = accessTokenOAuth2;



        //                    }
        //                }
        //            }



        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        if (ex.Status == WebExceptionStatus.ProtocolError)
        //        {
        //            var response = ex.Response as HttpWebResponse;
        //            if (response != null)
        //            {

        //                output("HTTP Status: " + response.StatusCode);
        //                var exceptionDetail = response.GetResponseHeader("WWW-Authenticate");
        //                if (exceptionDetail != null && exceptionDetail != "")
        //                {
        //                    output(exceptionDetail);
        //                }
        //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //                {
        //                    // read response body
        //                    string responseText = reader.ReadToEnd();
        //                    if (responseText != null && responseText != "")
        //                    {
        //                        output(responseText);
        //                    }
        //                }
        //            }

        //        }
        //    }

        //    output("Access token refreshed.");
        //}
        #endregion
    }
}