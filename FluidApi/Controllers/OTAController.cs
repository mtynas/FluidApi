using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using FluidAutomationService.Data;
using FluidAutomationService.Objects;

namespace FluidAutomationService.Controllers
{
    [Route("fluid/[controller]")]


    public class OTAController : Controller
    {


        [HttpGet("{op}")]

        public string Get(string op,
                           [FromQueryAttribute]string sessionid,
                           [FromQueryAttribute]string filename,
                           [FromQueryAttribute]int offset,
                           [FromQueryAttribute]int length)
        {
            RESTOTAFile result = new RESTOTAFile(RESTRequestStatusCode.failed);

            if (!string.IsNullOrEmpty(sessionid))
            {
                if (DataLayer.Connect())
                {
                    if (DataLayer.IsValidSession(sessionid, Request.HttpContext.Connection.RemoteIpAddress.ToString(), false))
                    {
                        try
                        {
                            // Open the Firmware file for reading
                            using (FileStream fs = new FileStream("Firmware/" + filename, FileMode.Open))
                            {
                                result.block = new byte[length];

                                // Go to the offset requested
                                fs.Seek(offset, SeekOrigin.Begin);

                                // Read a block of the requested length
                                int read_blocksize = fs.Read(result.block, 0, length);

                                if (read_blocksize < length)
                                {
                                    // Release the existing block allocation
                                    result.block = null;

                                    // Allocate a new block of the remaining size
                                    result.block = new byte[read_blocksize];

                                    // Return to the offset location
                                    fs.Seek(offset, SeekOrigin.Begin);
                                    // Re-read the final block again
                                    read_blocksize = fs.Read(result.block, 0, read_blocksize);
                                    // Return the final block size
                                    result.blocksize = read_blocksize;
                                }
                                else
                                {
                                    result.blocksize = read_blocksize;
                                }

                                result.sessionid = sessionid;

                                if (result.blocksize == length)
                                {
                                    result.statuscode = RESTRequestStatusCode.ok;
                                    result.status = RESTRequestStatusCode.ok.ToString();
                                }
                                else
                                {
                                    result.statuscode = RESTRequestStatusCode.eof;
                                    result.status = RESTRequestStatusCode.eof.ToString();
                                }
                            }
                        }
                        catch (FileNotFoundException ex)
                        {
                            Console.WriteLine("O\n!!!!!!!!!!!!!! TAController \n!!!!!!!!!!!!!! : Get {0}", ex);

                            result.info = ex.Data;
                            result.statuscode = RESTRequestStatusCode.failed;
                            result.status = RESTRequestStatusCode.failed.ToString();

                            result.status = RESTRequestStatusCode.notfound.ToString();
                            result.statuscode = RESTRequestStatusCode.notfound;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\n!!!!!!!!!!!!!! OTAController \n!!!!!!!!!!!!!! : Get {0}", ex);

                            result.info = ex.Data;
                            result.statuscode = RESTRequestStatusCode.failed;
                            result.status = RESTRequestStatusCode.failed.ToString();

                            result.status = RESTRequestStatusCode.readerror.ToString();
                            result.statuscode = RESTRequestStatusCode.readerror;
                        }
                    } // IsValidSession
                    else
                    {
                        result.status = RESTRequestStatusCode.invalidsessionid.ToString();
                        result.statuscode = RESTRequestStatusCode.invalidsessionid;
                    }
                } // Connect
                else
                {
                    result.status = RESTRequestStatusCode.databaseerror.ToString();
                    result.statuscode = RESTRequestStatusCode.databaseerror;
                }
            } // IsNullOrEmpty
            else
            {
                result.status = RESTRequestStatusCode.missingsessionid.ToString();
                result.statuscode = RESTRequestStatusCode.missingsessionid;
            }

            return result.ToJson();
        }
    }
}
