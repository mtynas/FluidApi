using System;

using System.Collections.Generic;
using FluidAutomationService.Objects;
using PushSharp.Apple;




namespace FluidAutomationService.Controllers
{
    public class NotificationController
    {
        public NotificationController( )
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

                Console.WriteLine( "\n!!!!!!!!!!!!!! Apple Notification Sent! \n!!!!!!!!!!!!!!" );

            };


            apnsBroker.Stop( );

        }
    }
}
