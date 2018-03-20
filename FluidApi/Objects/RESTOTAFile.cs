using System;

namespace FluidAutomationService.Objects
{
    public class RESTOTAFile : RESTRequestStatus
    {
        public int blocksize {get; set;}
        public byte[] block {get; set;}

        public RESTOTAFile(RESTRequestStatusCode initial_statuscode) : base(initial_statuscode)
        {
        }
    }
}