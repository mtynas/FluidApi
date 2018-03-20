using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using FluidAutomationService.Data;
using FluidAutomationService.Objects;
using FluidAutomationService.Common;
using Microsoft.AspNetCore.Http;

namespace FluidAutomationService.Controllers
{

    [Route( "fluid/[controller]" )]
    public class AppController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{op}")]
        public RESTRequestStatus  Get(string op)
        {
            RESTRequestStatus response = new RESTRequestStatus( RESTRequestStatusCode.unknown );


            if ( Security.Security.ValidRequest( op ) == true )
            {

                response.statuscode = RESTRequestStatusCode.success;
                response.status = RESTRequestStatusCode.success.ToString( );

                return response;

            }

            return response;
        }

        [HttpPost( "{appkey}" )]
        public string Post( string sessionid ,
                           string macaddress ,
                           string sensor_macaddress ,
                           string hwtype ,
                           string hwversion ,
                           string fwversion ,
                           string name ,
                           Int64 timestamp ,
                           string description )
        {
            RESTRequestStatus response = new RESTRequestStatus( RESTRequestStatusCode.unknown );

            if ( !string.IsNullOrEmpty( sessionid ) )
            {
                if ( DataLayer.Connect( ) )
                {
                    if ( DataLayer.IsValidSession( sessionid , Request.HttpContext.Connection.RemoteIpAddress.ToString( ) ) )
                    {

                        string fullpath = Request.Path;
                        string op = fullpath.Substring( "/fluid/app/".Length , fullpath.Length - "/fluid/app/".Length );

                        switch ( op )
                        {

                            case Constants.associateSensor:
                                if ( DataLayer.AssociateSensorWithAccount( response , sessionid , macaddress , sensor_macaddress , hwtype , hwversion , fwversion ) )
                                {
                                    response.statuscode = RESTRequestStatusCode.ok;
                                    response.status = RESTRequestStatusCode.ok.ToString( );
                                } else
                                {
                                    response.statuscode = RESTRequestStatusCode.failed;
                                    response.status = RESTRequestStatusCode.failed.ToString( );
                                }
                                break;
                        }
                    } else
                    {
                        response.statuscode = RESTRequestStatusCode.invalidsessionid;
                        response.status = RESTRequestStatusCode.invalidsessionid.ToString( );
                    }

                    DataLayer.CloseConnection( );
                } //  Connect
                else
                {
                    response.statuscode = RESTRequestStatusCode.databaseerror;
                }
            } else
            {
                response.statuscode = RESTRequestStatusCode.missingsessionid;
                response.status = RESTRequestStatusCode.missingsessionid.ToString( );
            }

            return response.ToJson( );
        }

        [HttpPut( "{id}" )]
        public void Put( int id , [FromBody]string value )
        {
        }

        [HttpDelete( "{id}" )]
        public void Delete( int id )
        {
        }



        static public bool ProceedWithRequest( RESTRequestStatus response , string appkey , string sessionId , HttpRequest request )
        {

            bool ret = false;

            if ( Security.Security.ValidRequest( appkey ) )
            {

                if ( DataLayer.Connect( ) == true )
                {

                    if ( DataLayer.IsValidSession( sessionId , request.HttpContext.Connection.RemoteIpAddress.ToString( ) ) == true )
                    {

                        ret = true;

                    } else {

                        response.statuscode = RESTRequestStatusCode.invalidsessionid;
                        response.status = RESTRequestStatusCode.invalidsessionid.ToString( );

                    }

                } else {

                    response.statuscode = RESTRequestStatusCode.databaseerror;
                    response.status = RESTRequestStatusCode.databaseerror.ToString( );

                }


            } else {

                response.statuscode = RESTRequestStatusCode.invalidappKey;
                response.status = RESTRequestStatusCode.invalidappKey.ToString( );

            }

            return ret;

        }

    }

}
