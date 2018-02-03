using System;
using System.Configuration;
using System.Web;
using System.Web.UI;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using System.Collections.Generic;

namespace OAuth1toOAuth2Migration
{
    public partial class Default : System.Web.UI.Page
    {
        #region <<App Properties >>
        public static string REQUEST_TOKEN_URL = ConfigurationManager.AppSettings["GET_REQUEST_TOKEN"];
        public static string ACCESS_TOKEN_URL = ConfigurationManager.AppSettings["GET_ACCESS_TOKEN"];
        public static string AUTHORIZE_URL = ConfigurationManager.AppSettings["AuthorizeUrl"];
        public static string OAUTH_URL = ConfigurationManager.AppSettings["OauthLink"];
        public string consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public string consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public string strrequestToken = string.Empty;
        public string tokenSecret = string.Empty;
        public string oauth_callback_url = "http://localhost:54703/Default.aspx?";
        public string GrantUrl = "http://localhost:54703/Default.aspx?connect=true";
  
        //public string GrantUrl = string.Empty;
        #endregion
        /// <summary>
        /// Page Load with initialization of properties.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString.Count > 0)
            {
                List<string> queryKeys = new List<string>(Request.QueryString.AllKeys);
                if (queryKeys.Contains("connect"))
                {
                    FireAuth();
                }
                if (queryKeys.Contains("oauth_token"))
                {
                    ReadToken();
                }
            }
            else
            {
                if (HttpContext.Current.Session["accessToken"] == null && HttpContext.Current.Session["accessTokenSecret"] == null)
                {
                    c2qb.Visible = true;
                    disconnect.Visible = false;
                   // lblDisconnect.Visible = false;
                }
                else
                {
                    c2qb.Visible = false;
                    disconnect.Visible = true;
                    //Disconnect();
                }
            }
        }
        /// <summary>
        /// Initiate the ouath screen.
        /// </summary>
        private void FireAuth()
        {
            HttpContext.Current.Session["consumerKey"] = consumerKey;
            HttpContext.Current.Session["consumerSecret"] = consumerSecret;
            CreateAuthorization();
            IToken token = (IToken)HttpContext.Current.Session["requestToken"];
            tokenSecret = token.TokenSecret;
            strrequestToken = token.Token;
        }
        /// <summary>
        /// Read the values from the query string.
        /// </summary>
        private void ReadToken()
        {
            HttpContext.Current.Session["oauthToken"] = Request.QueryString["oauth_token"].ToString(); ;
            HttpContext.Current.Session["oauthVerifyer"] = Request.QueryString["oauth_verifier"].ToString();
            HttpContext.Current.Session["realm"] = Request.QueryString["realmId"].ToString();
            HttpContext.Current.Session["dataSource"] = Request.QueryString["dataSource"].ToString();
            //Stored in a session for demo purposes.
            //Production applications should securely store the Access Token
            getAccessToken();
        }
        //
        #region <<Routines>>

        /// <summary>
        /// Create a session.
        /// </summary>
        /// <returns></returns>
        protected IOAuthSession CreateSession()
        {
            var consumerContext = new OAuthConsumerContext
            {
                ConsumerKey = HttpContext.Current.Session["consumerKey"].ToString(),
                ConsumerSecret = HttpContext.Current.Session["consumerSecret"].ToString(),
                SignatureMethod = SignatureMethod.HmacSha1
            };
            return new OAuthSession(consumerContext,
                                    REQUEST_TOKEN_URL,
                                    HttpContext.Current.Session["oauthLink"].ToString(),
                                    ACCESS_TOKEN_URL);
        }
        /// <summary>
        /// Get Access token.
        /// </summary>
        private void getAccessToken()
        {
            IOAuthSession clientSession = CreateSession();
            IToken accessToken = clientSession.ExchangeRequestTokenForAccessToken((IToken)HttpContext.Current.Session["requestToken"], HttpContext.Current.Session["oauthVerifyer"].ToString());
            HttpContext.Current.Session["accessToken"] = accessToken.Token;
            HttpContext.Current.Session["accessTokenSecret"] = accessToken.TokenSecret;
        }
        /// <summary>
        /// Do Authorization
        /// </summary>
        protected void CreateAuthorization()
        {
            //Remember these for later.
            HttpContext.Current.Session["consumerKey"] = consumerKey;
            HttpContext.Current.Session["consumerSecret"] = consumerSecret;
            HttpContext.Current.Session["oauthLink"] = OAUTH_URL;
            //
            IOAuthSession session = CreateSession();
            IToken requestToken = session.GetRequestToken();
            HttpContext.Current.Session["requestToken"] = requestToken;
            tokenSecret = requestToken.TokenSecret;
            var authUrl = string.Format("{0}?oauth_token={1}&oauth_callback={2}", AUTHORIZE_URL, requestToken.Token, UriUtility.UrlEncode(oauth_callback_url));
            HttpContext.Current.Session["oauthLink"] = authUrl;
            Response.Redirect(authUrl);
        }
        #endregion
        /// <summary>
        /// Disconnect and call Session_End event of the page life cycle 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnDisconnect_Click(object sender, EventArgs e)
        {
            //Clearing all session data
            Disconnect();
        }

        private void Disconnect()
        {
            try
            {
                Session.Clear();
                Session.Abandon();
                HttpContext.Current.Session["accessToken"] = null;
                HttpContext.Current.Session["accessTokenSecret"] = null;
                HttpContext.Current.Session["realm"] = null;
                HttpContext.Current.Session["dataSource"] = null;
                disconnect.Visible = false;
                //lblDisconnect.Visible = true;
            }
            catch (Exception ex)
            {
                Response.Write(ex.InnerException);
            }
        }
    }
}