using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using FluidAutomationService.Data;
using FluidAutomationService.Objects;
using FluidAutomationService.Common;
using PushSharp.Apple;
using Newtonsoft.Json;
using FluidAutomationService.Objects.Notifications;

namespace FluidAutomationService.Controllers
{

    [Route( "fluid/[controller]" )]

    public class SyncController : Controller
    {

        [HttpPost( "{appkey}" )]

        public RESTRequestStatus Post( string appkey ,
                                       string sessionid ,
                                       Int64 cloudId ,
                                       Int64 totalTimeStampSync
                                      )
        {

            RESTRequestStatus response = new RESTRequestStatus( RESTRequestStatusCode.unknown );

            Account account = null;
            Int64 accountNo = 0;


            if ( AppController.ProceedWithRequest( response , appkey , sessionid , Request ) == false )
            {

                return response;

            }

            string fullpath = Request.Path;
            string op = fullpath.Substring( "/fluid/sync/".Length , fullpath.Length - "/fluid/sync/".Length );

            switch ( op )
            {


                case Constants.basestation:

                    accountNo = DataLayer.GetAccountNoUsingSessionId( sessionid );

                    account = DataLayer.GetAccount( accountNo );

                    BaseStation baseStation = DataLayer.GetBaseStation( response , cloudId , account );

                    if ( baseStation.CloudId == cloudId )
                    {

                        Int64 totalTimeStamps;

                        response.statuscode = RESTRequestStatusCode.success;
                        response.status = RESTRequestStatusCode.success.ToString( );

                        totalTimeStamps = baseStation.NameTimeStamp +
                                                     baseStation.TempUnitTimeStamp +
                                                     baseStation.UserDescriptionTimeStamp;

                        if ( totalTimeStampSync != totalTimeStamps )
                        {

                            response.response = Constants.needsSyncing;

                        } else
                        {

                            response.response = Constants.synced;

                        }

                    }

                    break;

            }

            return response;

        }


        [HttpPut( "{appkey}" )]

        public RESTRequestStatus Put( string appkey ,
                                            string sessionid ,
                                            Int64 cloudid ,
                                            string syncitems ,
                                            string devicetoken

                                     )
        {

            RESTRequestStatus response = new RESTRequestStatus( RESTRequestStatusCode.unknown );

            Account account = null;
            Int64 accountNo = 0;

            if ( AppController.ProceedWithRequest( response , appkey , sessionid , Request ) == false )
            {

                return response;

            }

            string fullpath = Request.Path;
            string op = fullpath.Substring( "/fluid/sync/".Length , fullpath.Length - "/fluid/sync/".Length );

            switch ( op )
            {

                case Constants.basestation:


                    //IDictionary<string , string> baseStationSyncItemsDic = JsonConvert.DeserializeObject<Dictionary<string , string>>( syncitems );


                    accountNo = DataLayer.GetAccountNoUsingSessionId( sessionid );

                    account = DataLayer.GetAccount( accountNo );

                    BaseStation baseStation = DataLayer.GetBaseStation( response , cloudid , account );

                    if ( baseStation.CloudId == cloudid )
                    {

                        SyncBaseStation( response , baseStation , syncitems , devicetoken );

                    }

                    break;

                case Constants.sensor:

                    Sensor sensor = DataLayer.GetSensor( response , cloudid );


                    if ( sensor.CloudId.Equals( cloudid ) )
                    {

                        SyncSensor( response , sensor , syncitems , devicetoken );

                    }

                    break;


            }

            return response;

        }

        static public void SyncBaseStation( RESTRequestStatus response , BaseStation baseStation , string syncitems , string devicetoken )
        {

            IDictionary<string , string> baseStationSyncItemsDic = JsonConvert.DeserializeObject<Dictionary<string , string>>( syncitems );


            Notification notification;
            List<string> devicesTokens;


            Int64 totalTimeStamps;
            Int64 totalTimeStampSync;

            Int64 nameTimeStamp = 0;
            Int64 tempUnitTimeStamp = 0;
            Int64 descriptionTimeStamp = 0;

            string value;

            if ( baseStationSyncItemsDic.TryGetValue( Constants.keyNameTimeStamp , out value ) )
            {

                nameTimeStamp = Convert.ToInt64( value );

            }

            if ( baseStationSyncItemsDic.TryGetValue( Constants.keyTempUnitTimeStamp , out value ) )
            {

                tempUnitTimeStamp = Convert.ToInt64( value );

            }

            if ( baseStationSyncItemsDic.TryGetValue( Constants.keyUserDescriptionTimeStamp , out value ) )
            {

                descriptionTimeStamp = Convert.ToInt64( value );

            }


            totalTimeStamps = baseStation.NameTimeStamp +
                                                 baseStation.TempUnitTimeStamp +
                                                 baseStation.UserDescriptionTimeStamp;

            totalTimeStampSync = nameTimeStamp + tempUnitTimeStamp + descriptionTimeStamp;

            if ( totalTimeStampSync == totalTimeStamps )
            {

                response.response = Constants.synced;

                response.statuscode = RESTRequestStatusCode.success;
                response.status = RESTRequestStatusCode.success.ToString( );

                return;

            }


            if ( totalTimeStampSync != totalTimeStamps )
            {

                foreach ( KeyValuePair<string , string> pair in baseStationSyncItemsDic )
                {
                    if ( pair.Key.Equals( Constants.keyName ) )
                    {

                        if ( nameTimeStamp > baseStation.NameTimeStamp )
                        {

                            baseStation.Name = Convert.ToString( pair.Value );
                            baseStation.NameTimeStamp = nameTimeStamp;

                        }

                    } else if ( pair.Key.Equals( Constants.keyUserDescription ) )
                    {

                        if ( descriptionTimeStamp > baseStation.UserDescriptionTimeStamp )
                        {

                            baseStation.UserDescription = Convert.ToString( pair.Value );
                            baseStation.UserDescriptionTimeStamp = descriptionTimeStamp;

                        }

                    } else if ( pair.Key.Equals( Constants.keyTempUnit ) )
                    {

                        if ( tempUnitTimeStamp > baseStation.TempUnitTimeStamp )
                        {

                            baseStation.TempUnit = Convert.ToString( pair.Value );
                            baseStation.TempUnitTimeStamp = tempUnitTimeStamp;

                        }

                    }

                }

                totalTimeStamps = baseStation.NameTimeStamp +
                                    baseStation.TempUnitTimeStamp +
                                    baseStation.UserDescriptionTimeStamp;

                if ( totalTimeStampSync != totalTimeStamps )
                {
                    response.info.Add( Constants.totalTimeStamps , totalTimeStamps.ToString( ) );
                    response.info.Add( Constants.keyName , baseStation.Name );
                    response.info.Add( Constants.keyUserDescription , baseStation.UserDescription );
                    response.info.Add( Constants.keyTempUnit , baseStation.TempUnit );
                    response.info.Add( Constants.keyNameTimeStamp , baseStation.NameTimeStamp.ToString( ) );
                    response.info.Add( Constants.keyTempUnitTimeStamp , baseStation.TempUnitTimeStamp.ToString( ) );
                    response.info.Add( Constants.keyUserDescriptionTimeStamp , baseStation.UserDescriptionTimeStamp.ToString( ) );

                    if ( DataLayer.UpdateBaseStation( response , baseStation ) )
                    {

                        response.response = Constants.needsSyncing;

                    }

                } else {

                    if ( DataLayer.UpdateBaseStation( response , baseStation ) )
                    {

                        response.statuscode = RESTRequestStatusCode.success;
                        response.status = RESTRequestStatusCode.success.ToString( );

                        response.response = Constants.synced;

                        notification = new SilentNotification( baseStation );

                        notification.NotificationType = Constants.needsSyncing;

                        DataLayer.GetDeviceTokensForAccount( baseStation.AccountNo , out devicesTokens );

                        devicesTokens.Remove( devicetoken );

                        if ( devicesTokens.Count > 0 )
                        {

                            Task.Factory.StartNew(() => { PushNotification( notification , devicesTokens ); });

                        }

                    }

                }

            }

        }

        static public void SyncSensor( RESTRequestStatus response , Sensor sensor , string syncitems , string devicetoken )
        {

            IDictionary<string , string> baseStationSyncItemsDic = JsonConvert.DeserializeObject<Dictionary<string , string>>( syncitems );


            Notification notification;
            List<string> devicesTokens;


            Int64 totalTimeStamps;
            Int64 totalTimeStampSync;

            Int64 nameTimeStamp = 0;
            Int64 tempUnitTimeStamp = 0;
            Int64 descriptionTimeStamp = 0;

            string value;

            if ( baseStationSyncItemsDic.TryGetValue( Constants.keyNameTimeStamp , out value ) )
            {

                nameTimeStamp = Convert.ToInt64( value );

            }

            if ( baseStationSyncItemsDic.TryGetValue( Constants.keyTempUnitTimeStamp , out value ) )
            {

                tempUnitTimeStamp = Convert.ToInt64( value );

            }

            if ( baseStationSyncItemsDic.TryGetValue( Constants.keyUserDescriptionTimeStamp , out value ) )
            {

                descriptionTimeStamp = Convert.ToInt64( value );

            }


            totalTimeStamps = sensor.NameTimeStamp + sensor.UserDescriptionTimeStamp;

            totalTimeStampSync = nameTimeStamp + descriptionTimeStamp;

            if ( totalTimeStampSync == totalTimeStamps )
            {

                response.response = Constants.synced;

                response.statuscode = RESTRequestStatusCode.success;
                response.status = RESTRequestStatusCode.success.ToString( );

                return;

            }


            if ( totalTimeStampSync != totalTimeStamps )
            {

                foreach ( KeyValuePair<string , string> pair in baseStationSyncItemsDic )
                {
                    if ( pair.Key.Equals( Constants.keyName ) )
                    {

                        if ( nameTimeStamp > sensor.NameTimeStamp )
                        {

                            sensor.Name = Convert.ToString( pair.Value );
                            sensor.NameTimeStamp = nameTimeStamp;

                        }

                    } else if ( pair.Key.Equals( Constants.keyUserDescription ) )
                    {

                        if ( descriptionTimeStamp > sensor.UserDescriptionTimeStamp )
                        {

                            sensor.UserDescription = Convert.ToString( pair.Value );
                            sensor.UserDescriptionTimeStamp = descriptionTimeStamp;

                        }

                    }

                }

                totalTimeStamps = sensor.NameTimeStamp + sensor.UserDescriptionTimeStamp;

                if ( totalTimeStampSync != totalTimeStamps )
                {
                    response.info.Add( Constants.totalTimeStamps , totalTimeStamps.ToString( ) );
                    response.info.Add( Constants.keyName , sensor.Name );
                    response.info.Add( Constants.keyUserDescription , sensor.UserDescription );
                    response.info.Add( Constants.keyNameTimeStamp , sensor.NameTimeStamp.ToString( ) );
                    response.info.Add( Constants.keyUserDescriptionTimeStamp , sensor.UserDescriptionTimeStamp.ToString( ) );

                    if ( DataLayer.UpdateSensor( response , sensor ) )
                    {

                        response.response = Constants.needsSyncing;

                } else
                {

                    if ( DataLayer.UpdateSensor( response , sensor ) )
                    {

                            response.statuscode = RESTRequestStatusCode.success;
                            response.status = RESTRequestStatusCode.success.ToString( );

                            response.response = Constants.synced;

                            notification = new SilentNotification( sensor );

                            notification.NotificationType = Constants.needsSyncing;


                            DataLayer.GetDeviceTokensForAccount( sensor.AccountNo , out devicesTokens );

                            devicesTokens.Remove( devicetoken );

                            if ( devicesTokens.Count > 0 )
                            {

                                //Task.Factory.StartNew(() => { PushNotification( notification , devicesTokens ); });

                            }

                        }

                    }

                }

            }

        }


        static public void SyncRule( RESTRequestStatus response , Int64 ruleId )
        {



        }

         static public void PushNotification( Notification pushNotification , List<string> tokens )
        {

            String certificate = System.IO.Directory.GetCurrentDirectory( );

            certificate = certificate + "/Security/Certificates.p12";


            var config = new ApnsConfiguration( ApnsConfiguration.ApnsServerEnvironment.Sandbox , certificate , "marcmicha" );

            var apnsBroker = new ApnsServiceBroker( config );

            apnsBroker.Start( );

            foreach ( var deviceToken in tokens )
            {

                 apnsBroker.QueueNotification( new ApnsNotification
                {

                    DeviceToken = deviceToken ,

                    Payload = pushNotification.Payload( )
         
                } );

            }

            apnsBroker.OnNotificationFailed += ( notification , aggregateEx ) =>
            {

                aggregateEx.Handle( ex =>
                {

                    if ( ex is ApnsNotificationException )
                    {
                        var notificationException = ( ApnsNotificationException )ex;

                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;

                        Console.WriteLine( $"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}" );

                    } else
                    {
                        Console.WriteLine( $"Apple Notification Failed for some unknown reason : {ex.InnerException}" );
                    }

                    return true;

                } );
            };

            apnsBroker.OnNotificationSucceeded += ( notification ) =>
            {

                Console.WriteLine( $"\n!!!!!!!!!!!!!! Apple Notification Sent! {notification.DeviceToken}" );
           
            };


            apnsBroker.Stop( );

        }

       

    }

}
