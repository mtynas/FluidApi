using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
namespace FluidAutomationService.Objects
{
    public enum RESTRequestStatusCode
    {
        unknown,
        ok,
        failed,
        notfound,
        invalidrequest,
        missingsessionid,
        invalidsessionid,
        invalidaccount,
        accountexists,
        wrongpassword,
        invalidappKey,
        databaseerror,
        failedToCreate,
        eof,
        readerror,
        unknownoperation,
        securityerror,
        success,
        created,
        loggedin
    };

    public class RESTRequestStatus
    {
		public RESTRequestStatusCode statuscode;

		public string sessionid { get; set; }
        public string status { get; set; }
        public string response { get; set; }
        public IDictionary info { get; set; }


        public RESTRequestStatus(RESTRequestStatusCode initial_statuscode)
        {
            statuscode = initial_statuscode;
            status = statuscode.ToString();
            info = new Dictionary < string , string >();
        }

        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.StringEscapeHandling = StringEscapeHandling.Default;

            return JsonConvert.SerializeObject(this, settings);
        }

    }

}