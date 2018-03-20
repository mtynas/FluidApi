using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using FluidAutomationService.Data;
using FluidAutomationService.Objects;
using FluidAutomationService.Common;
using FluidAutomationService.Objects.Notifications;
using System.Threading.Tasks;

namespace FluidAutomationService.Controllers
{

    [Route( "fluid/[controller]" )]

    public class BasestationController : Controller
    {

        //[HttpPost]
        [HttpPost( "{appkey}" )]


        public RESTRequestStatus Post(
                            string sessionid ,
                            string appkey ,
                            string macaddress ,
                            string status ,
                            string hwversion ,
                            string fwversion ,
                            string name ,
                            string description ,
                            string tempunit ,
                            string devicetoken ,

                            Int64 cloudId ,
                            Int64 descriptiontimestamp ,
                            Int64 nametimestamp ,
                            Int64 tempunittimestamp

                            )

        {

            RESTRequestStatus response = new RESTRequestStatus( RESTRequestStatusCode.unknown );
           
            BaseStation baseStation = null;
            Account account = null;
            Int64 accountNo;

            if ( AppController.ProceedWithRequest( response , appkey , sessionid , Request ) == false )
            {

                return response;

            }

            string fullpath = Request.Path;

            string op = fullpath.Substring( "/fluid/basestation/".Length , fullpath.Length - "/fluid/basestation/".Length );

            switch ( op )
            {


                case Constants.basestations:

                    GetBaseStationsForAccount( response , sessionid );

                    break;


                case Constants.associateBasestation:


                    baseStation = new BaseStation( macaddress , 
                                                              hwversion , 
                                                              fwversion , 
                                                              name , 
                                                              status , 
                                                              nametimestamp , 
                                                              description , 
                                                              descriptiontimestamp ,
                                                              tempunit ,
                                                              tempunittimestamp );

                    AssociateBaseStationWithAccount( response , sessionid , baseStation , devicetoken );

                    break;

                case Constants.delete:

                    accountNo = DataLayer.GetAccountNoUsingSessionId( sessionid );

                    account = DataLayer.GetAccount( accountNo );

                    baseStation = DataLayer.GetBaseStation( response , cloudId,  account );

                    baseStation.Status = status;

                    DeleteBaseStationFromAccount( response , sessionid , baseStation , devicetoken );

                    break;

                    

            }

            DataLayer.CloseConnection( );

            return response;

        }


        [HttpPut( "{op}" )]

        public RESTRequestStatus Put( string op ,
                                    string appkey ,
                                    string sessionid ,
                                    string macaddress ,
                                    string hwtype ,
                                    string hwversion ,
                                    string fwversion )
        {

            RESTRequestStatus response = new RESTRequestStatus( RESTRequestStatusCode.unknown );

            if ( AppController.ProceedWithRequest( response , appkey , sessionid , Request ) == false )
            {

                return response;

            }
            switch ( op )
            {
                case Constants.notify:

                    Int64 cloudId = DataLayer.GetBaseStationId( response , sessionid , macaddress );

                    if ( cloudId != 0 )
                    {
                        response.info.Add( Constants.keyCloudId , cloudId );
                        response.statuscode = RESTRequestStatusCode.ok;
                        response.status = RESTRequestStatusCode.ok.ToString( );

                    } else
                    {
                        response.statuscode = RESTRequestStatusCode.notfound;
                        response.status = RESTRequestStatusCode.notfound.ToString( );
                    }
                    break;


            }

            return response;

        }

        // DELETE fluid/interface/5
        [HttpDelete( "{id}" )]
        public void Delete( int id )
        {
        }

        private void GetBaseStationsForAccount( RESTRequestStatus response , String sessionid )
        {

            if ( DataLayer.GetBaseStationsWithSessionId( response , sessionid ) )
            {

                response.statuscode = RESTRequestStatusCode.success;
                response.status = RESTRequestStatusCode.success.ToString( );

            }

        }

        private void AssociateBaseStationWithAccount( RESTRequestStatus response , String sessionid , BaseStation baseStation , String devicetoken )
        {

            SilentNotification notification = null;
            Int64 accountNo = 0;
            List<string> devicesTokens;


            if ( DataLayer.AssociateBaseStationWithAccount( response , sessionid , baseStation ) )
            {


                if ( DataLayer.GetBaseStationsWithSessionId( response , sessionid ) )
                {

                    accountNo = DataLayer.GetAccountNoUsingSessionId(sessionid);

                    response.statuscode = RESTRequestStatusCode.success;
                    response.status = RESTRequestStatusCode.success.ToString( );

                    notification = new SilentNotification( baseStation );

                    notification.NotificationType = Constants.add;

                    DataLayer.GetDeviceTokensForAccount( accountNo , out devicesTokens );

                    if ( devicesTokens == null )
                    {

                        return;

                    }

                    devicesTokens.Remove( devicetoken );

                    if ( devicesTokens.Count > 0 )
                    {

                        Task.Factory.StartNew(() => { NotificationController.PushNotification( notification , devicesTokens ); });

                    }

                }

            }

        }

        private void DeleteBaseStationFromAccount( RESTRequestStatus response , String sessionid , BaseStation baseStation , string devicetoken )
        {

            SilentNotification notification = null;

            List<string> devicesTokens;

            if ( DataLayer.DeleteBaseStation( response , baseStation ) )
            {

                if ( DataLayer.GetBaseStationsWithSessionId( response , sessionid ) )
                {

                    response.statuscode = RESTRequestStatusCode.success;
                    response.status = RESTRequestStatusCode.success.ToString( );

                    notification = new SilentNotification( baseStation );

                    notification.NotificationType = Constants.delete;

                    DataLayer.GetDeviceTokensForAccount( baseStation.AccountNo , out devicesTokens );

                    devicesTokens.Remove( devicetoken );

                    if ( devicesTokens.Count > 0 )
                    {

                        Task.Factory.StartNew(() => { NotificationController.PushNotification( notification , devicesTokens ); });

                    }

                }

            }

        }

    }

}
