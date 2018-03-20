using System;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using FluidAutomationService.Data;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Cryptography.X509Certificates;
using FluidAutomationService.Common;
using FluidAutomationService.Objects;

namespace FluidAutomationService.Controllers
{

    [Route("fluid/[controller]")]

    public class AuthenticationController : Controller
    {
        [Route("login")]
        [Route("signup")]
        [Route("forgot")]
        [Route("changepassword")]
        [Route("newsession")]
        [HttpPut]
        public RESTRequestStatus Put(string appkey,
                                     string accounttype, 
                                     string accountname, 
                                     string password, 
                                     string newpassword, 
                                     string sessionid)
        {

            RESTRequestStatus response = new RESTRequestStatus( RESTRequestStatusCode.failed );

            if ( Security.Security.ValidRequest( appkey ) == false )
            {

                response.statuscode = RESTRequestStatusCode.invalidappKey;
                response.status = RESTRequestStatusCode.invalidappKey.ToString( );

                return response;

            }

            if ( DataLayer.Connect( ) == false )
            {

                response.statuscode = RESTRequestStatusCode.databaseerror;
                response.status = RESTRequestStatusCode.databaseerror.ToString( );

                DataLayer.CloseConnection( );

                return response;
            }

         

            bool account_exists = DataLayer.AccountExists( accountname );

            string fullpath = Request.Path;
            string op = fullpath.Substring( "/fluid/authentication/".Length , fullpath.Length - "/fluid/authentication/".Length );

            switch ( op )
            {
                case Constants.signup:
                    
                    if ( !account_exists )
                    {

                        DataLayer.CreateAccount( response , accounttype , accountname , password , Request.HttpContext.Connection.RemoteIpAddress.ToString( ) );

                    } else
                    {
                       
                        response.response = RESTRequestStatusCode.accountexists.ToString();

                    }
                    break;

                case Constants.login:
                    if (account_exists)
                    {
                        response = DataLayer.Login(accountname, password, Request.HttpContext.Connection.RemoteIpAddress.ToString());
                    }              
                    break;

                case Constants.forgot:
                    if ( account_exists )
                    {
                        response = ForgotPassword( accountname );
                    }
                    break;

                case Constants.changepassword:
                    if ( account_exists )
                    {
                        response = ChangePassword( accountname , password , newpassword , sessionid );
                    }
                    break;

                case Constants.newsession:
                    if ( account_exists )
                    {

                        Int64 session_id = 0;

                        if ( ( DataLayer.CreateSession( accountname , Request.HttpContext.Connection.RemoteIpAddress.ToString( ) , out session_id ) ) )
                        {

                            response.statuscode = RESTRequestStatusCode.success;
                            response.status = Constants.statusSuccess;
                            response.sessionid = session_id.ToString();
                            response.response = accountname;

                        }
                    }
                    break;

                default:
                    response.statuscode = RESTRequestStatusCode.invalidrequest;
                    response.status = Constants.unknownRestOperation;
                    break;
            }

            DataLayer.CloseConnection( );

            return response;
        }


        #region Implementations

    private RESTRequestStatus ForgotPassword( string accountname )
        {
            RESTRequestStatus result = new RESTRequestStatus( RESTRequestStatusCode.failed );

            string newpassword = "";

            if ( DataLayer.ResetAccountPassword( accountname , out newpassword ) )
            {
                //Task email = 
                SendEmail( accountname ,
                          Constants.resetPasswordMessage ,
                          $"Please login to Fluid Automation (Click on Link) and change the provided temporary password: {newpassword}" );

                //email.Wait();

                result.statuscode = RESTRequestStatusCode.success;
                result.status = RESTRequestStatusCode.success.ToString( );
                result.response = accountname;
            } else
            {

                result.statuscode = RESTRequestStatusCode.failed;
                result.status = RESTRequestStatusCode.failed.ToString( );

            }

            return result;
        }

        private RESTRequestStatus ChangePassword( string accountname , string password , string newpassword , string sessionid )
        {
            RESTRequestStatus result = new RESTRequestStatus( RESTRequestStatusCode.failed );

            Hashtable infoDic = new Hashtable( 2 );

            if ( DataLayer.IsValidSession( sessionid , Request.Host.Host ) )
            {
                if ( DataLayer.ChangeAccountPassword( accountname , password , newpassword ) )
                {
                    //Task email = 
                    SendEmail( accountname ,
                              Constants.passwordChangedTitleMessage ,
                              Constants.passwordChangedMessage );

                    infoDic.Add( Constants.keyAccountName , accountname );
                    infoDic.Add( Constants.keyPassword , newpassword );

                    result.info = infoDic;
                    result.statuscode = RESTRequestStatusCode.success;
                    result.status = RESTRequestStatusCode.success.ToString( );

                } else
                {
                    result.statuscode = RESTRequestStatusCode.failed;
                    result.status = RESTRequestStatusCode.failed.ToString( );

                }
            }

            return result;
        }

        #endregion

        #region Helper Functions

        private void SendEmail( string email , string subject , string message )
        {
            try
            {
                var emailMessage = new MimeMessage( );

                emailMessage.From.Add( new MailboxAddress( "FluidAutomation" , "registration@getfluid.io" ) );
                emailMessage.To.Add( new MailboxAddress( "" , email ) );
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart( "plain" ) { Text = message };

                var certificate = new X509Certificate2( @"/Users/mtynas/FluidAutomationService/FluidAutomation-gmailcert.p12" , "notasecret" , X509KeyStorageFlags.Exportable );

                //var initialiser = new ServiceCredential.Initializer("registration@fluidautomation-151815.iam.gserviceaccount.com");

                /*                // Create Gmail API service.
                                var service = new GmailService(new BaseClientService.Initializer()
                                    {
                                        HttpClientInitializer = initialiser,
                                        ApplicationName = "FluidAutomation",
                                    });
                */

                /*                var credential = new ServiceCredential ( initialiser, {
                                    // Note: other scopes can be found here: https://developers.google.com/gmail/api/auth/scopes
                                    Scopes = new[] { "https://mail.google.com/" },
                                    User = "user@getfluid.io"
                                }.FromCertificate (certificate));
                */

                //You can also use FromPrivateKey(privateKey) where privateKey
                // is the value of the field 'private_key' in your serviceName.json file

                //                var credential = new ServiceCredential.Initializer("");
                //                bool result = credential.RequestAccessToken();

                using ( var client = new SmtpClient( ) )
                {
                    client.ClientCertificates.Add( certificate );
                    client.Connect( "smtp.gmail.com" , 587 , true );

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    //client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate( "user@getfluid.io" , "TpxWj6Ydch#34" );          // TpxWj6Ydch

                    client.Send( FormatOptions.Default , emailMessage );

                    //                client.LocalDomain = "gmail.com";
                    //                client.ConnectAsync("smtp.relay.uri", 25, SecureSocketOptions.None).ConfigureAwait(false);
                    //                client.SendAsync(emailMessage).ConfigureAwait(false);
                    //                client.DisconnectAsync(true).ConfigureAwait(false);

                    client.Disconnect( true );
                }
            }
            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!! SendEmail \n!!!!!!!!!!!!!!! : {0}" , ex );


            }
        }

        #endregion
    }
}
