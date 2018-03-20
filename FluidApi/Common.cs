/*
 * Constants.h
 *
 *  Created on:  3 Dec 2016
 *  Updated on: 15 Apr 2017
 *      Author: Micha & Mike
 */

//*****************************************************************************
// Keys
//*****************************************************************************

namespace FluidAutomationService.Common
{
    public class Constants
    {
        public const string keyMode = "mode";
        public const string keySSID = "ssid";
        public const string keyStatus = "status";
        public const string keyState = "state";

        public const string keyContext = "context";
        public const string keyStatusCode = "statuscode";
        public const string keyStatusMessage = "statusmessage";
        public const string keyName = "name";
        public const string keyApSSID = "apssid";
        public const string keyRSSI = "rssi";
        public const string keySecurityType = "securitytype";
        public const string keyMacAddress = "macaddress";
        public const string keyMasterMacAddress = "mastermacaddress";
        public const string keyShortAddress = "shortaddress";
        public const string keyCluster = "cluster";
        public const string keyAttribute = "attribute";
        public const string keyCommand = "command";
        public const string keyEmail = "email";
        public const string keyPassword = "password";
        public const string keyAccountNo = "accountno";
        public const string keyAccountName = "accountname";
        public const string keyAccountType = "accounttype";
        public const string keyAccountStatus = "accountstatus";
        public const string KeyVerifyTimeout = "verifytimeout";
        public const string keyAccountSessionId = "sessionid";
        public const string keyNetworks = "networks";
        public const string keyEndpoints = "endpoints";
        public const string keyNTPSources = "ntpsources";
        public const string keyThreads = "threads";
        public const string keyBaseStationStatus = "basestationstatus";
        public const string keyDate = "date";
        public const string keyTime = "time";
        public const string keyTimeStatus = "timestatus";
        public const string keyRAMFree = "ramfree";
        public const string keyRAMFreeMax = "ramfreemax";
        public const string keyRAMUsed = "ramused";
        public const string keyRAMUsedMax = "ramusedmax";
        public const string keyCoordinatorSeqNo = "coordseq";
        public const string keyTxPower = "txpower";
        public const string keyEnabled = "enabled";
        public const string keyRequest = "request";
        public const string keyScanTime = "scantime";
        public const string keyHWDriver = "hwdriver";
        public const string keyHWVersion = "hwversion";
        public const string keyReboot = "reboot";
        public const string keyApprove = "approve";
        public const string keyUnApprove = "unapprove";
        public const string keyApproval = "approval";
        public const string keyDiscovery = "discovery";
        public const string keyAppServer = "appserver";
        public const string keySpiStatus = "spistatus";
        public const string keyLedStatus = "ledstatus";
        public const string keyScanPeriod = "scanperiod";
        public const string keyLQI = "lqi";
        public const string keyWatchdog = "wdt";
        public const string keyFWStatus = "fwstatus";
        public const string keyCoordStatus = "coordstatus";
        public const string keyDebug = "debug";
        public const string keyReset = "reset";
        public const string keyNotReset = "notreset";
        public const string keySent = "sent";
        public const string keyNotSent = "notsent";


        public const string keyUrl = "url";
        public const string keyOTAType = "ota";
        public const string keyClockSource = "clocksource";
        public const string keyTimezone = "timezone";
        public const string keyUTCOffsetMins = "utcoffsetmins";
        public const string keyPowerSource = "powersource";
        public const string keyBootCount = "bootcount";
        public const string keyStackSize = "stacksize";
        public const string keyServiceStatus = "servicestatus";
        public const string keyData = "data";
        public const string keyVersionMajor = "major";
        public const string keyVersionMinor = "minor";
        public const string keyVersionBuild = "build";
        public const string keyAsyncQueueDepth = "asyncqueuedepth";
        public const string keyRxQueueDepth = "rxqueuedepth";
        public const string keyTxQueueDepth = "txqueuedepth";

        public const string keyTotalTimeStampSync = "totaltimestampsync";
        public const string keyDeviceToken = "devicetoken";
        public const string keyPlatform = "platform";
        public const string KeyAppKey = "appkey";
        public const string KeyHwType = "hwtype";
        public const string keyFwVersion = "fwversion";
        public const string keyCloudId = "cloudid";
        public const string keyBaseStations = "basestations";
        public const string keyBaseStationId = "basestationid";

        public const string keyBaseSensors = "sensors";
        public const string keyBaseStationSyncItems = "basestationsyncstems";
        public const string keyLastCommunicated = "lastcommunicated";
        public const string keyLastUpdated = "lastupdated";
        public const string keyIpAddress = "ipaddress";
        public const string keyCoordMacAddress = "coordmacaddress";


        public const string keyBaseStationName = "basestationname";
        public const string keyBaseStation = "basestation";


        public const string keySensorId = "sensorid";
        public const string keySensorType = "sensortype";
        public const string keySourceIp = "sourceip";
        public const string keyCreatedDT = "createddt";
        public const string keyCreationDate = "creationdate";

        public const string keyLoginMethod = "loginmethod";
        public const string keyDeviceType = "devicetype";

        public const string keyUserDescription = "userdescription";
        public const string keyTempUnit = "tempunit";
        public const string keyTempUnitTimeStamp = "tempunittimestamp";
        public const string keyUserDescriptionTimeStamp = "userdescriptiontimestamp";
        public const string keyNameTimeStamp = "nametimestamp";
        public const string keyTimeStamp = "timestamp";
        public const string keyAccount = "account";


        //*****************************************************************************
        // Key Values
        //*****************************************************************************

        public const string baseStationModeAP = "ap";
        public const string baseStationModeStation = "station";
        public const string baseStationModeUnavailable = "unavailable";

        public const string statusUnknown = "unknown";
        public const string statusUnimplemented = "unimplemented";
        public const string statusFailed = "failed";
        public const string statusSuccess = "success";
        public const string statusInProgress = "in progress";
        public const string statusRestarting = "restarting";
        public const string statusRebooting = "rebooting";

        public const string statusChangingSSID = "changing ssid";
        public const string statusDiscoverInProgress = "discover in progress";
        public const string statusDiscoverAlreadyInProgress = "discover already in progress";

        public const string statusMissing = "parameter missing";
        public const string statusRunning = "running";
        public const string statusUnavailable = "unavailable";
        public const string statusAppServerNotSet = "appserver not set";
        public const string statusAppServerFailedToConnect = "appserver failed to connect";
        public const string statusNoInternetConnection = "no internet connection";
        public const string statusFactoryReset = "factory reset";
        public const string statusUnsupported = "unsupported";
        public const string statusDiscovered = "discovered";
        public const string statusApproved = "approved";
        public const string statusReset = "reset";
        public const string statusDeleted = "deleted";
        public const string statusLive = "live";

        public const string statusOn = "on";
        public const string statusOff = "off";
        public const string statusDone = "done";
        public const string statusUSB = "usb";
        public const string statusSynchronised = "synchronised";
        public const string statusLocal = "local";
        public const string statusAccountCreated = @"account created";
        public const string statusAccountLogin = @"account login";


        public const string securityOpen = "open";
        public const string securityWep = "wep";
        public const string securityWPA = "wpa";
        public const string securityWPA2 = "wpa2";
        public const string securityUnavailable = "unavailable";

        public const string statusMessageMissingEmailOrPassword = "missing email or password";


        // Added April 17

        public const string invalidAccount = "invalid account";
        public const string unknownRequest = "unknown request";
        public const string keyLogin = "login";
        public const string associateBasestation = "associatebasestation";
        public const string associateSensor = "associatesensor";
        public const string failedToConnectToDB = "failed to connect to db";
        public const string failedToCreateAccount = "failed to connect to db";
        public const string verificationPending = "Verification Pending";
        public const string pendingValidation = "pendingvalidation";

        public const string emailExists = "email exists";
        public const string confirmEmailAccountMessage = "Please confirm email for you Fluid Automation Account";
        public const string forgot = "forgot";
        public const string signup = "signup";
        public const string login = "login";
        public const string delete = "delete";
        public const string add = "add";

        public const string created = "created";

        public const string session = "session";

        public const string resetPasswordMessage = "Fluid Automation - Reset Password";
        public const string emailSentMessage = "Password Reset Email Sent";
        public const string failedResetPasswordMessage = "Failed to reset password";
        public const string accountDontExistsMessage = "Account does not exist";
        public const string changepassword = "changepassword";
        public const string newsession = "newsession";
        public const string passwordChangedTitleMessage = "Fluid Automation - Password changed";
        public const string passwordChangedMessage = "Your Fluid Automation password has been successfully changed";
        public const string invalidSession = "Invalid Session";
        public const string unknownRestOperation = "Unknown REST operation";
        public const string bad = "bad";
        public const string basestations = "basestations";
        public const string sensors = "sensors";
        public const string missingBasestationMacAddress = "missing basestation mac address";
        public const string missingSessionId = "missing session id";
        public const string notify = "notify";
        public const string error = "error";
        public const string basestation = "basestation";
        public const string sensor = "sensor";
        public const string push = "push";
        public const string device = "device";
        public const string totalTimeStamps = "totaltimestamps";
        public const string synced = "synced";
        public const string needsSyncing = "needssyncing";


    }
}
