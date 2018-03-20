using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using FluidAutomationService.Data;
using FluidAutomationService.Objects;
using FluidAutomationService.Common;
using Newtonsoft.Json;

namespace FluidAutomationService.Controllers
{

    [Route( "fluid/[controller]" )]

    public class AccountController : Controller
    {

        [HttpPost( "{appkey}" )]
        public RESTRequestStatus Post( string appkey ,
                                    string sessionid ,
                                    string deviceToken ,
                                    string platform ,
                                    Int64 accountno
                                            )
        {

            RESTRequestStatus response = new RESTRequestStatus( RESTRequestStatusCode.unknown );

            if ( AppController.ProceedWithRequest( response , appkey , sessionid , Request ) == false )
            {
                return response;
            }

            string fullpath = Request.Path;
            string op = fullpath.Substring( "/fluid/account/".Length , fullpath.Length - "/fluid/account/".Length );

            switch ( op )
            {
                case Constants.device:
                    if ( DataLayer.AddDeviceToAccount( response , accountno , deviceToken , platform ) )
                    {
                        response.statuscode = RESTRequestStatusCode.success;
                        response.status = RESTRequestStatusCode.success.ToString( );

                    }
                    break;
            }

            return response;
        }

        [HttpPut( "{appkey}" )]
        public RESTRequestStatus Put( string appkey ,
                                            string sessionid ,
                                            Int64 cloudId ,
                                            Int64 totalTimeStampSync ,
                                            string baseStationSyncItems

                                           )
        {

            RESTRequestStatus response = new RESTRequestStatus( RESTRequestStatusCode.unknown );

            if ( AppController.ProceedWithRequest( response , appkey , sessionid , Request ) == false )
            {

                return response;

            }

            return response;
        }
    }

}
