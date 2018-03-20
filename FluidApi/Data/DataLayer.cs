using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Data;
using Microsoft.SqlServer;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using FluidAutomationService.Common;
using FluidAutomationService.Objects;
using Newtonsoft.Json;
using FluidAutomationService.Objects.Notifications;


/*
 *
 *
drop table account

CREATE TABLE Account
(
  accountno bigint PRIMARY KEY NOT NULL,
  email varchar(80) NOT NULL,
  password varchar(40) NOT NULL,
  accountstatus varchar(20) NOT NULL,
  verifytimeout varchar(20),
  accounttype varchar(20)  NOT NULL DEFAULT('fl'),
  creationdate integer
);

insert into Account(accountno,email,password,accountstatus,accounttype) values (12345678,'michaf@me.com','aaa','active','fl')


DROP TABLE Session

CREATE TABLE Session
(
  sessionid bigint PRIMARY KEY NOT NULL,
  accountname nvarchar(128) NOT NULL,
  sourceip varchar(20),
  createddt integer,
  lastcommunicated integer,
  loginmethod varchar(40),
  devicetype varchar(40),
  state varchar(20)
);


DROP TABLE BaseStation 

CREATE TABLE BaseStation 
(
  cloudid integer PRIMARY KEY IDENTITY(1,1) NOT NULL,
  status varchar(40),
  accountno integer NOT NULL,
  macaddress varchar(40) NOT NULL,
  hwversion varchar(20) NOT NULL,
  fwversion varchar(20) NOT NULL DEFAULT(' '),
  lastcommunicated integer,
  lastupdated integer,
  ipaddress varchar(40),
  coordmacaddress varchar(40),
  name varchar(80),
  nametimestamp integer,
  tempunit integer,
  tempunittimestamp integer,
  userdescription varchar(40),
  userdescriptiontimestamp integer
);

DROP TABLE Device

CREATE TABLE Device
(
  accountno integer,
  devicetoken varchar(40),
  platform varchar(40)
);


DROP TABLE Firmware 

CREATE TABLE Firmware 
(
  FirmwareId integer PRIMARY KEY IDENTITY(1,1) NOT NULL,
  FWType varchar(40) NOT NULL,
  HWVersion varchar(20) NOT NULL,
  FWVersion varchar(20) NOT NULL,
  FilePath varchar(256) NOT NULL,
  FileSize integer NOT NULL,
  FileCrc integer
);

DROP TABLE Sensor 

CREATE TABLE Sensor 
(
  cloudid integer PRIMARY KEY IDENTITY(1,1) NOT NULL,
  status varchar(20),
  basestationid varchar(20),
  accountno integer NOT NULL,
  macaddress varchar(20) NOT NULL,
  mastermacaddress varchar(20) null,
  sensortype varchar(20) NOT NULL,
  hwversion varchar(20) NOT NULL,
  fwversion varchar(20) NOT NULL,
  lastcommunicated integer,
  name varchar(40),
  nametimestamp integer,
  userdescription varchar(256),
  userdescriptiontimestamp integer
);

DROP TABLE SysConfig 

CREATE TABLE SysConfig 
(
  Domain varchar(40) NOT NULL,
  Parameter varchar(50) NOT NULL DEFAULT(' '),
  Value nvarchar(128),
  BlobValue IMAGE,
  PRIMARY KEY(Domain, Parameter)
);

DROP TABLE SysLog

CREATE TABLE SysLog
(
  SysLogId integer PRIMARY KEY IDENTITY(1,1) NOT NULL,
  LogDT datetime NOT NULL,
  SessionId integer NOT NULL,
  Category varchar(20) NOT NULL DEFAULT(' '),
  LogMessage varchar(512) NOT NULL
)

*/

namespace FluidAutomationService.Data
{
    public class DataLayer
    {
        //private static SqliteConnection connection = null;
        private static DbConnection connection = null;
        private static string dbtype = "";

        private static Random rand = new Random( );

        private static int CurrentTimeStamp( )
        {

            DateTime reference = new DateTime( 2001 , 01 , 01 , 0 , 0 , 0 , DateTimeKind.Utc );

            DateTime now = DateTime.UtcNow;

            TimeSpan difference = now.Subtract( reference );

            return Convert.ToInt32( difference.TotalSeconds );

        }


        #region Database Utils

        static public bool Connect( )
        {
            bool result = false;

            var configbuilder = new ConfigurationBuilder( );
            configbuilder.SetBasePath( Directory.GetCurrentDirectory( ) );
            configbuilder.AddJsonFile( "appsettings.json" );
            var config = configbuilder.Build( );

            string pwd = Directory.GetCurrentDirectory( );
            dbtype = config [ "dbtype" ];
            string connection_string = "";


            if (dbtype == "SqlLite")
            {
                SqliteConnectionStringBuilder connection_builder = new SqliteConnectionStringBuilder();
                string dbPath = config["Database"];
                connection_builder.DataSource = pwd + "/" + dbPath;
                connection_string = connection_builder.ConnectionString;

                Console.WriteLine($"Database Location: {pwd}/{dbPath}");

                connection = ( SqliteConnection )SqliteFactory.Instance.CreateConnection( );
            }
            else
            {
                string dbPath = config["SqlConnection"];

                SqlConnectionStringBuilder connection_builder = new SqlConnectionStringBuilder(dbPath);
                connection_string = connection_builder.ConnectionString;

                connection = (SqlConnection)SqlClientFactory.Instance.CreateConnection();
            }

            connection.ConnectionString = connection_string;

            try
            {
                connection.Open( );

                result = true;
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "\n!!!!!!!!!!!!!! Connect !!!!!!!!!!!!!! : {0} \n" , ex );
            }

            return result;
        }

        static DbCommand GetCommand(string query, DbTransaction transaction = null)
        {
            DbCommand command = null;

            if (dbtype == "SqlLite")
            {
                command = new SqliteCommand();
            }
            else
            {
                command = new SqlCommand();
            }

            command.Connection = connection;
            command.CommandText = query;

            if (transaction != null)
            {
                command.Transaction = transaction;    
            }

            return command;
        }

        static public void CloseConnection( )
        {
            if ( connection != null )
            {
                connection.Close( );
            }
        }


        #endregion

        #region Account Operations

        static public bool AccountExists( string email )
        {
            bool result = false;

            try
            {
                string query = $"select count(*) from Account where {Constants.keyEmail} = '{email}'";

                using ( DbCommand command = GetCommand(query) )
                {
                    object account_found = command.ExecuteScalar( );

                    if ( ( int )account_found == 1 )
                    {
                        result = true;
                    }
                }
            }
            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!! AccountExists !!!!!!!!!!!!!! : {0} \n" , ex );

            }

            return result;
        }

        static public Account GetAccount( Int64 AccountNo )
        {

            Account account = null;

            try
            {
                string sqlQuery = $"select Account.* from Account where {Constants.keyAccountNo}='{AccountNo}' ";

                using ( DbCommand command = GetCommand(sqlQuery) )
                {
                    using ( DbDataReader reader = command.ExecuteReader( ) )
                    {
                        if ( reader.HasRows )
                        {
                            if ( reader.Read( ) )
                            {

                                account = new Account( );

                                account.AccountNo = reader.GetInt64( reader.GetOrdinal( $"{Constants.keyAccountNo}" ) );

                                account.Email = GetDBString( reader , $"{Constants.keyEmail}" );

                                account.Type = GetDBString( reader , $"{Constants.keyAccountType}" );

                                account.Password = GetDBString( reader , $"{Constants.keyPassword}" );
                            }
                        }

                    }

                }

            }

            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!! GetAccount !!!!!!!!!!!!!! : {0} \n" , ex );


            }

            return account;

        }

        static public bool ChangeAccountPassword( string accountname , string password , string newpassword )
        {
            bool result = false;

            string sqlQuery = $"update Account set {Constants.keyPassword}='{newpassword}' where {Constants.keyEmail} = '{accountname}' and Password='{password}'";

            using ( DbTransaction transaction = connection.BeginTransaction( ) )
            {
                result = ExecuteSqlCommandInTransaction( sqlQuery , transaction );
            }

            return result;
        }

        static public bool ResetAccountPassword( string email , out string newpassword )
        {
            bool result = false;

            newpassword = "";
            for ( int i = 0 ; i < 10 ; i++ )
            {
                int ci = rand.Next( 61 );

                char c;

                if ( i <= 9 )
                {
                    c = ( char )( '0' + i );
                } else
                if ( i <= 35 )
                {
                    c = ( char )( 'a' + i - 10 );
                } else
                {
                    c = ( char )( 'A' + i - 35 );
                }

                newpassword += c;
            }

            result = true;

            return result;
        }

        static public void CreateAccount( RESTRequestStatus response ,
                                         string accounttype ,
                                         string accountname ,
                                         string password ,
                                         string source_host )
        {

            Int64 accounNo = 0;
            Int64 session_id = 0;

            int attempt = 50;

            Account account = null;

            using ( DbTransaction transaction = connection.BeginTransaction( ) )
            {

                try
                {
                    accounNo = AllocateAccountNo( transaction );

                    string sqlQuery = $"insert into Account ({Constants.keyAccountNo}, {Constants.keyAccountType}, {Constants.keyEmail}, {Constants.keyPassword}, {Constants.keyAccountStatus} , {Constants.keyCreationDate} ) " +
                        $"values ({accounNo}, '{accounttype}', '{accountname}', '{password}', '{Constants.pendingValidation}' , {DataLayer.CurrentTimeStamp( )})";

                    if ( ExecuteSqlCommandInTransaction( sqlQuery , transaction ) )
                    {

                        account = GetAccount( accounNo );

                        while ( attempt > 0 && !CreateSession( accountname , source_host , out session_id ) )
                        {

                            attempt--;

                        }

                        response.sessionid = session_id.ToString( );

                        account.SessionId = session_id.ToString( );

                        JsonSerializerSettings settings = new JsonSerializerSettings( );

                        string json = JsonConvert.SerializeObject( account , settings );

                        response.info.Add( Constants.keyAccount , json );

                        response.response = RESTRequestStatusCode.created.ToString( );
                        response.statuscode = RESTRequestStatusCode.success;
                        response.status = RESTRequestStatusCode.success.ToString( );

                    }

                }
                catch ( Exception ex )
                {

                    Console.WriteLine( "\n!!!!!!!!!!!!!! CreateAccount !!!!!!!!!!!!!! : {0} \n" , ex );

                }

            }

        }

        private static bool ExecuteSqlCommandInTransaction( string sqlQuery , DbTransaction transaction )
        {
            bool result = false;

            using ( DbCommand command = GetCommand( sqlQuery, transaction ) )
            {
                try
                {
                    int affected_rows = command.ExecuteNonQuery( );

                    if ( affected_rows == 1 )
                    {
                        transaction.Commit( );

                        result = true;
                    }
                }
                catch ( Exception ex )
                {

                    Console.WriteLine( "\n!!!!!!!!!!!!!! ExecuteSqlCommandInTransaction !!!!!!!!!!!!!! : {0} \n" , ex );

                    transaction.Rollback( );
                }
                finally
                {
                    if ( !result )
                    {
                        transaction.Rollback( );
                    }
                }
            }

            return result;
        }

        static private Int64 AllocateAccountNo( DbTransaction transaction )
        {
            Int64 accountNo = 0;

            string sqlQuery = $"select count(*) from Account where {Constants.keyAccountNo}='{accountNo}'";

            using ( DbCommand command = GetCommand( sqlQuery, transaction ) )
            {
                try
                {
                    bool account_no_found = false;

                    // Repeat in a loop to find a free acount number
                    do
                    {
                        accountNo = rand.Next( 143996589 , int.MaxValue );

                        command.CommandText = $"select count(*) from Account where {Constants.keyAccountNo}='{accountNo}'";
                        object account_found = command.ExecuteScalar( );

                        if ( ( Int64 )account_found == 0 )
                        {
                            account_no_found = true;
                        }

                    } while ( !account_no_found );
                }
                catch ( Exception ex )
                {

                    Console.WriteLine( "\n!!!!!!!!!!!!!! AllocateAccountNo !!!!!!!!!!!!!! : {0} \n" , ex );

                }
            }

            return accountNo;
        }
        #endregion

        static private object ExecuteSqlCommandForScalar( string sqlQuery )
        {
            object result = null;

            using ( DbCommand command = GetCommand( sqlQuery ) )
            {
                try
                {
                    result = command.ExecuteScalar( );
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "\n!!!!!!!!!!!!!! ExecuteSqlCommandForScalar !!!!!!!!!!!!!! {0} \n" , ex );

                }
            }

            return result;
        }

        static public RESTRequestStatus Login(string accountname,
                                              string password,
                                              string source_host)
        {
            RESTRequestStatus response = new RESTRequestStatus(RESTRequestStatusCode.unknown);

            Account account = null;

            try
            {
                string sqlQuery = $"select Account.* from Account where {Constants.keyEmail}='{accountname}'";

                using ( DbCommand command = GetCommand( sqlQuery ) )
                {
                    using ( DbDataReader reader = command.ExecuteReader( ) )
                    {
                        if ( reader.HasRows )
                        {
                            if ( reader.Read( ) )
                            {

                                account = new Account( );

                                account.AccountNo = reader.GetInt64( reader.GetOrdinal( $"{Constants.keyAccountNo}" ) );

                                account.Email = GetDBString( reader , $"{Constants.keyEmail}" );

                                account.Type = GetDBString( reader , $"{Constants.keyAccountType}" );

                                account.Password = GetDBString( reader , $"{Constants.keyPassword}" );

                            }

                        }
                    }
                }


            }

            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!! Datalyer:Login !!!!!!!!!!!!!! : {0} \n" , ex );

            }

            if ( account != null )
            {

              
                if ( account.Password.Equals( password ) == false )
                {
                    response.response = RESTRequestStatusCode.wrongpassword.ToString( );
                    response.statuscode = RESTRequestStatusCode.failed;
                    response.status = RESTRequestStatusCode.failed.ToString( );

                } else {


                    Int64 session_id = 0;

                    int attempt = 50;

                    while ( attempt > 0 && !CreateSession( accountname , source_host , out session_id ) )
                    {
                        attempt--;
                    }

                    JsonSerializerSettings settings = new JsonSerializerSettings( );

                    account.SessionId = session_id.ToString( );

                    string json = JsonConvert.SerializeObject( account , settings );

                    response.response = RESTRequestStatusCode.loggedin.ToString( );
                    response.statuscode = RESTRequestStatusCode.success;
                    response.status = RESTRequestStatusCode.success.ToString( );
                    response.sessionid = session_id.ToString( );

                    response.info.Add( Constants.keyPassword , password );
                    response.info.Add( Constants.keyAccountName , accountname );
                    response.info.Add( Constants.keyAccount , json );

                    GetBaseStationsWithSessionId( response , session_id.ToString( ) );

                    GetSensorsWithSessionId( response , session_id.ToString( ) );


                }

            } else {

                response.response = RESTRequestStatusCode.invalidaccount.ToString( );
                response.statuscode = RESTRequestStatusCode.failed;
                response.status = RESTRequestStatusCode.failed.ToString( );

            }

            return response;
        }

        static public bool CreateSession( string accountname , string source_host , out Int64 sessionid )
        {
            bool result = false;

            sessionid = rand.Next( 1234567890 , int.MaxValue );


            string sqlQuery = $"insert into Session ({Constants.keyAccountSessionId}, {Constants.keyAccountName}, {Constants.keySourceIp}, {Constants.keyState}, {Constants.keyCreatedDT}, {Constants.keyLastCommunicated}) " +
                $"values ({sessionid},'{accountname}','{source_host}','{Constants.created}',{DataLayer.CurrentTimeStamp()},{DataLayer.CurrentTimeStamp()})";

            using ( DbTransaction transaction = connection.BeginTransaction( ) )
            {
                try
                {
                    result = ExecuteSqlCommandInTransaction( sqlQuery , transaction );
                }
                catch ( Exception ex )
                {

                    Console.WriteLine( "\n!!!!!!!!!!!!!! CreateSession !!!!!!!!!!!!!! : {0}\n" , ex );

                    transaction.Rollback( );
                }
            }

            return result;

        }

        static public void DeleteExpiredSessions( )
        {

            try
            {
                string query = $"update Session " +
                               $"set {Constants.keyState}='expired' " +
                               $"where {Constants.keyState} in ('created','open') " +
                               $"and {Constants.keyLastCommunicated} < {DataLayer.CurrentTimeStamp() - 86400 }";
                
                using ( DbCommand update_command = GetCommand(query) )
                {
                    update_command.ExecuteNonQuery( );
                }
            }
            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!! DeleteExpiredSessions !!!!!!!!!!!!!! : {0}\n" , ex );

            }
        }

        static public bool IsValidSession( string sessionid , string source_host , bool update_status = true )
        {
            bool result = false;

            DeleteExpiredSessions( );

            string sqlQuery = $"select count(*) from Session where {Constants.keyAccountSessionId}='{sessionid}'";
            object account_found = ExecuteSqlCommandForScalar( sqlQuery );

            if ( account_found != null && ( Int64 )account_found == 1 )
            {
                using ( DbTransaction transaction = connection.BeginTransaction( ) )
                {
                    string updateSessionQuery = $"update Session " +
                        $"set {Constants.keyState} = 'open',{Constants.keyLastCommunicated} = {DataLayer.CurrentTimeStamp()},{Constants.keySourceIp}='{source_host}' " +
                                                $"where {Constants.keyAccountSessionId} = '{sessionid}' and {Constants.keyState} in ('created','open')";

                    result = ExecuteSqlCommandInTransaction( updateSessionQuery , transaction );

                }
            }

            return result;
        }

        static public Int64 GetAccountNoUsingSessionId( string sessionid )
        {
            Int64 result = 0;

            string sqlQuery = $"select {Constants.keyAccountNo} from Account,Session where {Constants.keyAccountSessionId}='{sessionid}' and Account.{Constants.keyEmail}=Session.{Constants.keyAccountName}";
            object account_found = ExecuteSqlCommandForScalar( sqlQuery );

            if ( account_found != null )
            {
                result = ( Int64 )account_found;
            }

            return result;
        }

        static public Int64 GetAccountNoUsingName( string name )
        {
            Int64 result = 0;

            string sqlQuery = $"select {Constants.keyAccountNo} from Account where {Constants.keyEmail}='{name}'";

            object account_found = ExecuteSqlCommandForScalar( sqlQuery );

            if ( account_found != null )
            {
                result = ( Int64 )account_found;
            }

            return result;
        }

        static public bool BaseStationExists( string basestation_mac , out Int64 bsAccountNo )
        {
            bool result = false;

            bsAccountNo = 0;

            try
            {
                string sqlQuery = $"select BaseStation.* from BaseStation where {Constants.keyMacAddress}='{basestation_mac}'";

                using ( DbCommand command = GetCommand( sqlQuery ) )
                {
                    using ( DbDataReader reader = command.ExecuteReader( ) )
                    {
                        if ( reader.HasRows )
                        {
                            if ( reader.Read( ) )
                            {
                                bsAccountNo = reader.GetInt64( reader.GetOrdinal( $"{Constants.keyAccountNo}" ) );
                            }

                            result = true;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!! BaseStationExists !!!!!!!!!!!!!! : {0}\n" , ex );

            }

            return result;
        }

        static public bool AssociateBaseStationWithAccount( RESTRequestStatus response , String sessionid , BaseStation baseStation )
        {
            bool result = false;

            Int64 bsAccountNo;

            Int64 accountNo = GetAccountNoUsingSessionId( sessionid );

            if ( accountNo > 0 )
            {
                bool basestation_exists = BaseStationExists( baseStation.MACAddress , out bsAccountNo );

                string sqlQuery;

                if ( !basestation_exists )
                {
                    sqlQuery = $"insert into BaseStation ({Constants.keyAccountNo}, {Constants.keyMacAddress}, {Constants.keyHWVersion}, {Constants.keyFwVersion}, {Constants.keyStatus}, {Constants.keyName} , {Constants.keyNameTimeStamp} , {Constants.keyUserDescription} , {Constants.keyUserDescriptionTimeStamp} , {Constants.keyTempUnit} , {Constants.keyTempUnitTimeStamp}, {Constants.keyLastCommunicated}) " +
                        $"values ({accountNo}," +
                        $"'{baseStation.MACAddress}'," +
                        $"'{baseStation.HWVersion}'," +
                        $"'{baseStation.FWVersion}'," +
                        $"'{baseStation.Status}'," +
                        $"'{baseStation.Name}'," +
                        $" {baseStation.NameTimeStamp}," +
                        $" '{baseStation.UserDescription}'," +
                        $" {baseStation.UserDescriptionTimeStamp}," +
                        $" '{baseStation.TempUnit}'," +
                        $" {baseStation.TempUnitTimeStamp}," +
                        $" {DataLayer.CurrentTimeStamp()})";
                } else
                {
                    sqlQuery = $"update BaseStation set {Constants.keyAccountNo}={accountNo}," +
                        $"{Constants.keyHWVersion} = '{baseStation.HWVersion}'," +
                        $"{Constants.keyFwVersion} = '{baseStation.FWVersion}'," +
                        $"{Constants.keyName} = '{baseStation.Name}'," +
                        $"{Constants.keyStatus} = '{baseStation.Status}'," +
                        $"{Constants.keyNameTimeStamp} = {baseStation.NameTimeStamp}," +
                        $"{Constants.keyUserDescription} = '{baseStation.UserDescription}'," +
                        $"{Constants.keyUserDescriptionTimeStamp} = {baseStation.UserDescriptionTimeStamp}," +
                        $"{Constants.keyTempUnit} = '{baseStation.TempUnit}'," +
                        $"{Constants.keyTempUnitTimeStamp} = {baseStation.TempUnitTimeStamp}," +
                        $"{Constants.keyLastCommunicated} = {DataLayer.CurrentTimeStamp()} " +
                        $"where {Constants.keyMacAddress} = '{baseStation.MACAddress}'";
                }

                using ( DbTransaction transaction = connection.BeginTransaction( ) )
                {
                    using ( DbCommand update_command = GetCommand( sqlQuery ) )
                    {
                        update_command.Transaction = transaction;

                        try
                        {
                            int affected_rows = update_command.ExecuteNonQuery( );

                            if ( affected_rows == 1 )
                            {
                                transaction.Commit( );

                                result = true;
                            } else
                            {
                                transaction.Rollback( );
                            }
                        }
                        catch ( Exception ex )
                        {
                            Console.WriteLine( "\n!!!!!!!!!!!!!! AssociateBaseStationWithAccount !!!!!!!!!!!!!! : {0}\n" , ex );


                            response.info = ex.Data;
                            transaction.Rollback( );
                        }
                    }
                }
            } else
            {
                result = false;
            }

            return result;
        }

        static public Int64 GetBaseStationId( RESTRequestStatus response , string sessionid , string macaddress )
        {

            Int64 result = 0;

            try
            {
                string sqlQuery = $"select {Constants.keyCloudId} " +
                                  $"from Account,Session,BaseStation " +
                                  $"where {Constants.keyAccountSessionId} = '{sessionid}' " +
                                  $"and Account.{Constants.keyEmail} = Session.{Constants.keyAccountName} " +
                                  $"and Account.{Constants.keyAccountNo} = BaseStation.{Constants.keyAccountNo} " +
                                  $"and BaseStation.{Constants.keyMacAddress} = '{macaddress}'";

                using ( DbCommand command = GetCommand( sqlQuery ) )
                {
                    object basestationid = command.ExecuteScalar( );

                    if ( basestationid != null )
                    {
                        result = ( Int64 )basestationid;

                    }
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "\n!!!!!!!!!!!!!! GetBaseStationId !!!!!!!!!!!!!! : {0}\n" , ex );

                response.info = ex.Data;

            }

            return result;
        }

        static string GetDBString( DbDataReader reader , string fieldname )
        {
            string result = "";
            int field_no = reader.GetOrdinal( fieldname );

            if ( !reader.IsDBNull( field_no ) )
            {
                result = reader.GetString( field_no );
            }

            return result;
        }

        static Int64 GetInt64( DbDataReader reader , string fieldname )
        {
            Int64 result = 0;


            result = reader.GetInt64( reader.GetOrdinal( fieldname ) );

            return result;
        }

        static public bool GetBaseStationsWithSessionId( RESTRequestStatus response , string sessionid )
        {

            bool result = false;
            List<BaseStation> baseStationsArray;

            try
            {
                string sqlQuery = $"select BaseStation.* , Session.accountname from BaseStation,Account,Session where SessionId='{sessionid}' " +
                    $"and Account.{Constants.keyEmail}=Session.{Constants.keyAccountName} " +
                    $"and Account.{Constants.keyAccountNo}=BaseStation.{Constants.keyAccountNo}";


                using ( DbCommand command = GetCommand( sqlQuery ) )
                {

                    using ( DbDataReader reader = command.ExecuteReader( ) )
                    {

                        if ( reader.HasRows )
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings( );
                            settings.StringEscapeHandling = StringEscapeHandling.Default;

                            baseStationsArray = new List<BaseStation>( );

                            while ( reader.Read( ) )
                            {

                                BaseStation basestation = new BaseStation( );
                                basestation.CloudId = GetInt64( reader , $"{Constants.keyCloudId}" );
                                basestation.Status = GetDBString( reader , $"{Constants.keyStatus}" );
                                basestation.AccountNo = GetInt64( reader , $"{Constants.keyAccountNo}" );
                                basestation.MACAddress = GetDBString( reader , $"{Constants.keyMacAddress}" );
                                basestation.HWVersion = GetDBString( reader , $"{Constants.keyHWVersion}" );
                                basestation.FWVersion = GetDBString( reader , $"{Constants.keyFwVersion}" );
                                basestation.Name = GetDBString( reader , $"{Constants.keyName}" );
                                basestation.NameTimeStamp = GetInt64( reader , $"{Constants.keyNameTimeStamp}" );
                                basestation.UserDescription = GetDBString( reader , $"{Constants.keyUserDescription}" );
                                basestation.UserDescriptionTimeStamp = GetInt64( reader , $"{Constants.keyUserDescriptionTimeStamp}" );
                                basestation.TempUnit = GetDBString( reader , $"{Constants.keyTempUnit}" );
                                basestation.TempUnitTimeStamp = GetInt64( reader , $"{Constants.keyTempUnitTimeStamp}" );
                                basestation.AccountName = GetDBString( reader , $"{Constants.keyAccountName}" );

                                baseStationsArray.Add( basestation );

                            }

                            response.info.Add( Constants.keyBaseStations , JsonConvert.SerializeObject( baseStationsArray , settings ) );


                            result = true;
                        }
                    }

                }

            }
            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!! GetBaseStationsWithSessionId !!!!!!!!!!!!!! : {0}\n" , ex );

                response.info = ex.Data;
                response.statuscode = RESTRequestStatusCode.failed;
                response.status = RESTRequestStatusCode.failed.ToString( );

            }

            return result;
        }

        static public bool GetSensorsWithSessionId( RESTRequestStatus response , string sessionid )
        {

            bool result = false;
            List<Sensor> sensorsArray;

            try
            {

                string sqlQuery = $"select Sensor.* , Session.accountname from Sensor,Account,Session where SessionId='{sessionid}' " +
                    $"and Account.{Constants.keyEmail}=Session.{Constants.keyAccountName} " +
                    $"and Account.{Constants.keyAccountNo}=Sensor.{Constants.keyAccountNo}";


                using ( DbCommand command = GetCommand( sqlQuery ) )
                {

                    using ( DbDataReader reader = command.ExecuteReader( ) )
                    {

                        if ( reader.HasRows )
                        {
                            JsonSerializerSettings settings = new JsonSerializerSettings( );
                            settings.StringEscapeHandling = StringEscapeHandling.Default;

                            sensorsArray = new List<Sensor>( );

                            while ( reader.Read( ) )
                            {

                                Sensor sensor = new Sensor( );
                                sensor.CloudId = GetInt64( reader , $"{Constants.keyCloudId}" );
                                sensor.BaseStationCloudId = GetInt64( reader , $"{Constants.keyBaseStationId}" );
                                sensor.Status = GetDBString( reader , $"{Constants.keyStatus}" );
                                sensor.AccountNo = GetInt64( reader , $"{Constants.keyAccountNo}" );
                                sensor.MACAddress = GetDBString( reader , $"{Constants.keyMacAddress}" );
                                sensor.MasterMacAddress = GetDBString( reader , $"{Constants.keyMasterMacAddress}" );
                                sensor.HWVersion = GetDBString( reader , $"{Constants.keyHWVersion}" );
                                sensor.FWVersion = GetDBString( reader , $"{Constants.keyFwVersion}" );
                                sensor.Name = GetDBString( reader , $"{Constants.keyName}" );
                                sensor.NameTimeStamp = GetInt64( reader , $"{Constants.keyNameTimeStamp}" );
                                sensor.UserDescription = GetDBString( reader , $"{Constants.keyUserDescription}" );
                                sensor.UserDescriptionTimeStamp = GetInt64( reader , $"{Constants.keyUserDescriptionTimeStamp}" );
                                sensor.SensorType = GetDBString( reader , $"{Constants.keySensorType}" );
                                sensor.AccountName = GetDBString( reader , $"{Constants.keyAccountName}" );
                                sensorsArray.Add( sensor );

                            }

                            response.info.Add( Constants.keyBaseSensors , JsonConvert.SerializeObject( sensorsArray , settings ) );


                            result = true;

                        }
                    }

                }

            }
            catch ( Exception ex )
            {
                Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!!GetSensorsWithSessionId : {0}\n" , ex );

                response.info = ex.Data;
                response.statuscode = RESTRequestStatusCode.failed;
                response.status = RESTRequestStatusCode.failed.ToString( );

            }

            return result;
        }

        static public BaseStation GetBaseStation( RESTRequestStatus response , Int64 cloudId , Account forAccount )
        {

            BaseStation basestation = new BaseStation( );

            try
            {

                string sqlQuery = $"select BaseStation.* from BaseStation where BaseStation.{Constants.keyCloudId}='{cloudId}' " +
                    $"and BaseStation.{Constants.keyAccountNo} = '{forAccount.AccountNo}'";

                using ( DbCommand command = GetCommand( sqlQuery ) )
                {

                    using ( DbDataReader reader = command.ExecuteReader( ) )
                    {

                        if ( reader.HasRows )
                        {
                            int counter = 0;

                            while ( reader.Read( ) )
                            {
                                if ( counter > 0 )
                                {

                                    throw new System.InvalidOperationException( "More then one BaseStation in Datalayer.GetBaseStation" );

                                }

                                basestation = new BaseStation( );
                                basestation.CloudId = GetInt64( reader , $"{Constants.keyCloudId}" );
                                basestation.Status = GetDBString( reader , $"{Constants.keyStatus}" );
                                basestation.AccountNo = GetInt64( reader , $"{Constants.keyAccountNo}" );
                                basestation.MACAddress = GetDBString( reader , $"{Constants.keyMacAddress}" );
                                basestation.HWVersion = GetDBString( reader , $"{Constants.keyHWVersion}" );
                                basestation.FWVersion = GetDBString( reader , $"{Constants.keyFwVersion}" );
                                basestation.Name = GetDBString( reader , $"{Constants.keyName}" );
                                basestation.NameTimeStamp = GetInt64( reader , $"{Constants.keyNameTimeStamp}" );
                                basestation.UserDescription = GetDBString( reader , $"{Constants.keyUserDescription}" );
                                basestation.UserDescriptionTimeStamp = GetInt64( reader , $"{Constants.keyUserDescriptionTimeStamp}" );
                                basestation.TempUnit = GetDBString( reader , $"{Constants.keyTempUnit}" );
                                basestation.TempUnitTimeStamp = GetInt64( reader , $"{Constants.keyTempUnitTimeStamp}" );
                                basestation.AccountName = forAccount.Email;

                                counter++;

                            }

                        }

                    }

                }
            }

            catch ( Exception ex )
            {
                Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!! GetBaseStation  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n" , ex );

                response.info = ex.Data;
                response.statuscode = RESTRequestStatusCode.failed;
                response.status = RESTRequestStatusCode.failed.ToString( );

            }

            return basestation;

        }

        static public Sensor GetSensor( RESTRequestStatus response , Int64 sensorCloudId )
        {

            Sensor sensor = new Sensor( );


            {
                try
                {

                    string sqlQuery = $"select * from Sensor where {Constants.keySensorId} = {sensorCloudId}";

                    using ( DbCommand command = GetCommand( sqlQuery ) )
                    {
                        using ( DbDataReader reader = command.ExecuteReader( ) )
                        {
                            if ( reader.HasRows )
                            {
                                while ( reader.Read( ) )
                                {
                                    sensor.CloudId = GetInt64( reader , $"{Constants.keyCloudId}" );
                                    sensor.Status = GetDBString( reader , $"{Constants.keyStatus}" );
                                    sensor.BaseStationCloudId = GetInt64( reader , $"{Constants.keyBaseStationId}" );
                                    sensor.AccountNo = GetInt64( reader , $"{Constants.keyAccountNo}" );
                                    sensor.MACAddress = GetDBString( reader , $"{Constants.keyMacAddress}" );
                                    sensor.MasterMacAddress = GetDBString( reader , $"{Constants.keyMasterMacAddress}" );
                                    sensor.HWVersion = GetDBString( reader , $"{Constants.keyHWVersion}" );
                                    sensor.FWVersion = GetDBString( reader , $"{Constants.keyFwVersion}" );
                                    sensor.Name = GetDBString( reader , $"{Constants.keyName}" );
                                    sensor.NameTimeStamp = GetInt64( reader , $"{Constants.keyNameTimeStamp}" );
                                    sensor.UserDescription = GetDBString( reader , $"{Constants.keyUserDescription}" );
                                    sensor.UserDescriptionTimeStamp = GetInt64( reader , $"{Constants.keyUserDescriptionTimeStamp}" );
                                    sensor.SensorType = GetDBString( reader , $"{Constants.keySensorType}" );
                                    sensor.AccountName = GetDBString( reader , $"{Constants.keyAccountName}" );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!! GetSensor  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n" , ex );

                    response.info = ex.Data;
                    response.statuscode = RESTRequestStatusCode.failed;
                    response.status = RESTRequestStatusCode.failed.ToString( );

                }

            }

            return sensor;
        }

        static public bool GetSensors( RESTRequestStatus response , string sessionid , string basestation_macaddress , out List<Sensor> sensors )
        {
            bool result = false;

            sensors = null;


            Int64 basestationid = GetBaseStationId( response , sessionid , basestation_macaddress );

            if ( basestationid > 0 )
            {

                try
                {

                    string sqlQuery = $"select * from Sensor where {Constants.keyCloudId} = {basestationid}";

                    using ( DbCommand command = GetCommand( sqlQuery ) )
                    {
                        using ( DbDataReader reader = command.ExecuteReader( ) )
                        {
                            if ( reader.HasRows )
                            {
                                sensors = new List<Sensor>( );

                                while ( reader.Read( ) )
                                {
                                    Sensor sensor = new Sensor( );
                                    sensor.CloudId = GetInt64( reader , $"{Constants.keyCloudId}" );
                                    sensor.Status = GetDBString( reader , $"{Constants.keyStatus}" );
                                    sensor.BaseStationCloudId = GetInt64( reader , $"{Constants.keyBaseStationId}" );
                                    sensor.AccountNo = GetInt64( reader , $"{Constants.keyAccountNo}" );
                                    sensor.MACAddress = GetDBString( reader , $"{Constants.keyMacAddress}" );
                                    sensor.MasterMacAddress = GetDBString( reader , $"{Constants.keyMasterMacAddress}" );
                                    sensor.HWVersion = GetDBString( reader , $"{Constants.keyHWVersion}" );
                                    sensor.FWVersion = GetDBString( reader , $"{Constants.keyFwVersion}" );
                                    sensor.Name = GetDBString( reader , $"{Constants.keyName}" );
                                    sensor.NameTimeStamp = GetInt64( reader , $"{Constants.keyNameTimeStamp}" );
                                    sensor.UserDescription = GetDBString( reader , $"{Constants.keyUserDescription}" );
                                    sensor.UserDescriptionTimeStamp = GetInt64( reader , $"{Constants.keyUserDescriptionTimeStamp}" );
                                    sensor.SensorType = GetDBString( reader , $"{Constants.keySensorType}" );
                                    sensor.AccountName = GetDBString( reader , $"{Constants.keyAccountName}" );

                                    sensors.Add( sensor );
                                }
                            }

                            result = true;
                        }
                    }
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!! GetSensors  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n" , ex );

                    response.info = ex.Data;
                    response.statuscode = RESTRequestStatusCode.failed;
                    response.status = RESTRequestStatusCode.failed.ToString( );

                }
            }

            return result;
        }

        static public bool GetDeviceTokensForAccount( Int64 accountNo , out List<String> tokens )
        {

            bool result = false;

            tokens = null;

            try
            {
                string sqlQuery = $"select devicetoken from Device where {Constants.keyAccountNo} = {accountNo}";

                using ( DbCommand command = GetCommand( sqlQuery ) )
                {
                    using ( DbDataReader reader = command.ExecuteReader( ) )
                    {
                        if ( reader.HasRows )
                        {
                            tokens = new List<String>( );

                            while ( reader.Read( ) )
                            {
                                tokens.Add( GetDBString( reader , $"{Constants.keyDeviceToken}" ) );
                            }
                        }

                        result = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                Console.WriteLine( "\n!!!!!!!!!!!!!! GetDeviceTokensForAccount !!!!!!!!!!!!!! : {0}\n" , ex );
            }

            return result;

        }

        static public bool UpdateSensor( RESTRequestStatus response , Sensor sensor )
        {

            bool result = false;

            try
            {

                string sqlQuery = $"update Sensor " +
                    $"set {Constants.keyFwVersion} = {sensor.FWVersion}," +
                    $"{Constants.keyLastCommunicated} = {DataLayer.CurrentTimeStamp()}," +
                    $"{Constants.keyName} = {sensor.Name}," +
                    $"{Constants.keyNameTimeStamp} = {sensor.NameTimeStamp}," +
                    $"{Constants.keyUserDescription} = {sensor.UserDescription}," +
                    $"{Constants.keyUserDescriptionTimeStamp} = {sensor.UserDescriptionTimeStamp}" +
                    $"where {Constants.keyCloudId} = {sensor.CloudId}";


                using ( DbTransaction transaction = connection.BeginTransaction( ) )
                {
                    using ( DbCommand update_command = GetCommand( sqlQuery , transaction ) )
                    {
                        try
                        {
                            int affected_rows = update_command.ExecuteNonQuery( );

                            if ( affected_rows == 1 )
                            {
                                transaction.Commit( );

                                response.statuscode = RESTRequestStatusCode.success;
                                response.status = RESTRequestStatusCode.success.ToString( );


                                result = true;

                            } else
                            {

                                transaction.Rollback( );

                            }
                        }
                        catch ( Exception ex )
                        {

                            response.info = ex.Data;
                            transaction.Rollback( );

                        }
                    }

                }

            }
            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!! UpdateSensor  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n" , ex );

                response.info = ex.Data;
                response.statuscode = RESTRequestStatusCode.failed;
                response.status = RESTRequestStatusCode.failed.ToString( );

            }

            return result;

        }

        static public bool UpdateBaseStation( RESTRequestStatus response , BaseStation baseStation )
        {

            bool result = false;

            try
            {

                string sqlQuery = $"update BaseStation " +
                    $"set {Constants.keyFwVersion} = '{baseStation.FWVersion}'," +
                    $"{Constants.keyLastCommunicated} = {DataLayer.CurrentTimeStamp()}," +
                    $"{Constants.keyName} = '{baseStation.Name}'," +
                    $"{Constants.keyNameTimeStamp} = {baseStation.NameTimeStamp}," +
                    $"{Constants.keyUserDescription} = '{baseStation.UserDescription}'," +
                    $"{Constants.keyUserDescriptionTimeStamp} = {baseStation.UserDescriptionTimeStamp}," +
                    $"{Constants.keyTempUnit} = '{baseStation.TempUnit}'," +
                    $"{Constants.keyTempUnitTimeStamp} = {baseStation.TempUnitTimeStamp}" +
                    $" where {Constants.keyCloudId} = {baseStation.CloudId}";


                using ( DbTransaction transaction = connection.BeginTransaction( ) )
                {
                    using ( DbCommand update_command = GetCommand( sqlQuery , transaction ) )
                    {
                        try
                        {
                            int affected_rows = update_command.ExecuteNonQuery( );

                            if ( affected_rows == 1 )
                            {
                                transaction.Commit( );
                            }

                            response.statuscode = RESTRequestStatusCode.success;
                            response.status = RESTRequestStatusCode.success.ToString( );

                            result = true;
                        }
                        catch ( Exception ex )
                        {
                            response.statuscode = RESTRequestStatusCode.failed;
                            response.status = RESTRequestStatusCode.failed.ToString( );

                            response.info = ex.Data;
                            transaction.Rollback( );
                        }
                    }

                }

            }
            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!! UpdateBaseStation  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n" , ex );

                response.info = ex.Data;
                response.statuscode = RESTRequestStatusCode.failed;
                response.status = RESTRequestStatusCode.failed.ToString( );

            }

            return result;

        }

        static public bool DeleteBaseStation( RESTRequestStatus response , BaseStation baseStation )
        {

            bool result = false;

            try
            {

                string sqlQuery = $"update BaseStation " +
                    $"set {Constants.keyFwVersion} = '{baseStation.FWVersion}'," +
                    $"{Constants.keyLastCommunicated} = {DataLayer.CurrentTimeStamp()}," +
                    $"{Constants.keyStatus} = '{baseStation.Status}'" +
                   
                    $" where {Constants.keyCloudId} = {baseStation.CloudId} AND {Constants.keyMacAddress} ='{baseStation.MACAddress}'";


                using ( DbTransaction transaction = connection.BeginTransaction( ) )
                {
                    using ( DbCommand update_command = GetCommand( sqlQuery, transaction ) )
                    {
                        try
                        {
                            int affected_rows = update_command.ExecuteNonQuery( );

                            if ( affected_rows == 1 )
                            {

                                transaction.Commit( );

                            }

                            response.statuscode = RESTRequestStatusCode.success;
                            response.status = RESTRequestStatusCode.success.ToString( );

                            result = true;

                        }
                        catch ( Exception ex )
                        {

                            Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!! UpdateBaseStation  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n" , ex );

                            response.statuscode = RESTRequestStatusCode.failed;
                            response.status = RESTRequestStatusCode.failed.ToString( );

                            response.info = ex.Data;
                            transaction.Rollback( );

                        }
                    }

                }

            }
            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!! UpdateBaseStation  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n" , ex );

                response.info = ex.Data;
                response.statuscode = RESTRequestStatusCode.failed;
                response.status = RESTRequestStatusCode.failed.ToString( );

            }

            return result;

        }

        static public bool DeleteSensor( RESTRequestStatus response , Sensor sensor )
        {

            bool result = false;

            try
            {

                string sqlQuery = $"update Sensor " +
                    $"set {Constants.keyFwVersion} = {sensor.FWVersion}," +
                    $"{Constants.keyLastCommunicated} = {DataLayer.CurrentTimeStamp()}," +
                    $"{Constants.keyStatus} = '{sensor.Status}'" +
                    $"where {Constants.keyCloudId} = {sensor.CloudId} AND {Constants.keyMacAddress}='{sensor.MACAddress}'";


                using ( DbTransaction transaction = connection.BeginTransaction( ) )
                {
                    using ( DbCommand update_command = GetCommand( sqlQuery, transaction ) )
                    {
                        try
                        {
                            int affected_rows = update_command.ExecuteNonQuery( );

                            if ( affected_rows == 1 )
                            {
                                transaction.Commit( );

                                response.statuscode = RESTRequestStatusCode.success;
                                response.status = RESTRequestStatusCode.success.ToString( );


                                result = true;

                            } 
                            else
                            {
                                transaction.Rollback( );
                            }
                        }
                        catch ( Exception ex )
                        {
                            response.info = ex.Data;
                            transaction.Rollback( );
                        }
                    }

                }

            }
            catch ( Exception ex )
            {
                Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!! UpdateSensor  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n" , ex );

                response.info = ex.Data;
                response.statuscode = RESTRequestStatusCode.failed;
                response.status = RESTRequestStatusCode.failed.ToString( );
            }

            return result;
        }

        static public bool AssociateSensorWithAccount( RESTRequestStatus response ,
                                                      string sessionid ,
                                                      string basestation_macaddress ,
                                                      string sensor_mac ,
                                                      string sensor_type ,
                                                      string hwversion ,
                                                      string fwversion )
        {
            bool result = false;

            Int64 basestationid = GetBaseStationId( response , sessionid , basestation_macaddress );

            if ( basestationid > 0 )
            {
                DbTransaction transaction = null;

                try
                {
                    string sqlQuery=$"insert into Sensor ({Constants.keyCloudId}," +
                                    $"{Constants.keyMacAddress}, {Constants.keySensorType}," +
                                    $"{Constants.keyHWVersion}, {Constants.keyFwVersion})" +
                                    $"{Constants.keyMasterMacAddress})" +
                                    $"values ({basestationid}, '{sensor_mac}', '{sensor_type}', '{hwversion}', '{fwversion}')";

                    using (DbCommand command = GetCommand(sqlQuery))
                    {
                        transaction = connection.BeginTransaction();
                        command.Transaction = transaction;

                        int affected_rows = command.ExecuteNonQuery();

                        if (affected_rows == 1)
                        {
                            transaction.Commit();

                            result = true;
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                }
                catch ( Exception ex )
                {

                    Console.WriteLine( "\n!!!!!!!!!!!!!!!!!!!!!!!! AssociateSensorWithAccount  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n" , ex );

                    response.info = ex.Data;
                    response.statuscode = RESTRequestStatusCode.failed;
                    response.status = RESTRequestStatusCode.failed.ToString( );

                    transaction.Rollback( );
                }
            }

            return result;
        }

        static public bool DeviceExists( Int64 accountNo , string token )
        {

            bool result = false;

            try
            {
                string sqlQuery = $"select Device.* from Device where {Constants.keyAccountNo}='{accountNo}' AND {Constants.keyDeviceToken}='{token}'";
               
                using ( DbCommand command = GetCommand( sqlQuery ) )
                {
                    using ( DbDataReader reader = command.ExecuteReader( ) )
                    {
                        if ( reader.HasRows )
                        {
                            result = true;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {

                Console.WriteLine( "\n!!!!!!!!!!!!!! DeviceExists !!!!!!!!!!!!!! : {0}\n" , ex );


            }

            return result;
        }


        static public bool AddDeviceToAccount( RESTRequestStatus response ,
                                               Int64 accountNo ,
                                               string token ,
                                               string platform )

        {
            bool result = false;

            if ( DeviceExists( accountNo , token ) )
            {
                result = true;

            } 
            else 
            {
                DbTransaction transaction = null;

                try
                {
                    string sqlQuery = $"insert into Device ({Constants.keyAccountNo}, {Constants.keyDeviceToken} , {Constants.keyPlatform} )" +
                                      $"values ('{accountNo}', '{token}', '{platform}')";
                        
                    using (DbCommand command = GetCommand(sqlQuery))
                    {
                        transaction = connection.BeginTransaction();
                        command.Transaction = transaction;

                        int affected_rows = command.ExecuteNonQuery();

                        if (affected_rows == 1)
                        {
                            transaction.Commit();

                            result = true;
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!! AddDeviceToAccount  !!!!!!!!!!!!!!!!!!!!!!!! : {0}\n", ex);

                    response.info = ex.Data;
                    response.statuscode = RESTRequestStatusCode.failed;
                    response.status = RESTRequestStatusCode.failed.ToString();

                    transaction.Rollback();
                }
            }

            return result;
        }
    }
}
