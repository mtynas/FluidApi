using System;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using FluidAutomationService.Interfaces;

namespace FluidAutomationService.Objects
{
    public class Notification
    {

        public CloudObject NotificationObject { get; set; }
        public string NotificationType { get; set; }


        public Notification ( CloudObject notificationObject )
        {

            this.NotificationObject = notificationObject;

        }

        public virtual JObject Payload( )
        {

            //Must be implemeted by derived classes

            return null;

        }


    }
}
