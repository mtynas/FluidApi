using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using FluidAutomationService.Interfaces;

namespace FluidAutomationService.Objects.Notifications
{
    public class SilentNotification : Notification
    {


        public SilentNotification ( CloudObject notificationObject ) : base (  notificationObject )
        {


        }


        public override JObject Payload( )
        {

            Dictionary<string , object> aps = new Dictionary<string , object>( );

            Dictionary<string , object> apsDetails = new Dictionary<string , object>( );

            apsDetails.Add( "content-available" , 1 );

            aps.Add( "aps" , apsDetails );
            aps.Add( "acmkind" , NotificationObject.GetType().Name );
            aps.Add( "acmcloudid" , NotificationObject.CloudIdentifier() );
            aps.Add( "acmtype" , NotificationType );



            //return JObject.Parse(JsonConvert.SerializeObject(aps));

            return JObject.FromObject(aps);

        }

    }
}
