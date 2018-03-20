using System;
using System.Collections.Generic;
using FluidAutomationService.Interfaces;

namespace FluidAutomationService.Data
{
    public class BaseStation : CloudObject
    {
        public string MACAddress { get; set; }
        public string HWVersion { get; set; }
        public string FWVersion { get; set; }
        public string AvailableFWVersion { get; set; }
        public string AccountName { get; set; }
        public string Status { get; set; }
        public Int64 CloudId { get; set; }
        public Int64 AccountNo { get; set; }
        public DateTime LastCommunicated { get; set; }

        public string Name { get; set; }
        public Int64 NameTimeStamp { get; set; }

        public string UserDescription { get; set; }
        public Int64 UserDescriptionTimeStamp { get; set; }

        public string TempUnit { get; set; }
        public Int64 TempUnitTimeStamp { get; set; }

        public List<Sensor> Sensors { get; set; }

        public BaseStation( )
        {

        }

        public BaseStation( string  MACAddress ,
                           string   HWVersion , 
                           string   FWVersion , 
                           string   Name , 
                           string   Status , 
                           Int64    NameTimeStamp , 
                           string   Description , 
                           Int64    DescriptionTimeStamp,
                           string   TempUnit,  
                           Int64    TempUnitTimeStamp
                          )
        {

            this.MACAddress = MACAddress;
            this.LastCommunicated = LastCommunicated;
            this.HWVersion = HWVersion;
            this.FWVersion = FWVersion;
            this.Name = Name;
            this.Status = Status;
            this.NameTimeStamp = NameTimeStamp;
            this.UserDescription = Description;
            this.UserDescriptionTimeStamp = DescriptionTimeStamp;
            this.TempUnit = TempUnit;
            this.TempUnitTimeStamp = TempUnitTimeStamp;
            this.CloudId = CloudId;

        }

        public Int64 CloudIdentifier()
        {

            return CloudId;

        }

    }
}
