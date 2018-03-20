using System;
using FluidAutomationService.Interfaces;


namespace FluidAutomationService.Data
{
    public class Sensor: CloudObject
    {
        public Int64 CloudId { get; set; }
        public Int64 BaseStationCloudId {get; set;}
        public Int64 AccountNo { get; set; } 
        public string AccountName { get; set; }
        public string MACAddress {get; set;}
        public string MasterMacAddress {get; set;}
        public string SensorType {get; set;}
		public string Name { get; set; }
        public string Status { get; set; }
        public Int64 NameTimeStamp { get; set; }
        public string UserDescription { get; set; }
        public Int64 UserDescriptionTimeStamp { get; set; }
        public string HWVersion {get; set;}
        public string FWVersion {get; set;}
		DateTime LastCommunicated { get; set; }

		public Sensor()
        {            
        }

        public Int64 CloudIdentifier()
        {

            return CloudId;

        }


    }
}