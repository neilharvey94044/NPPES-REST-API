using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NppesAPI.SQL
{
    public static class SQLCommands 
    {

        public static string NPPESSQL_0015
        {
            get
            {
                return "dbo.sp_npigeo_get";
            }
        }


        public static string NPPESSQL_0016
        {
            get
            {
                return "dbo.sp_provider_get";
            }
        }

        /*
      * Get a single Provider (Individual).
      * Parameters:
      * @npi  = the National Provider Identifier
      * 
      */
        public static string NPPESSQL_0005
        {
            get
            {
                return @"
                    Select npi.NPI as NPI
                            ,npi.Entity_Type_Code as Type
                            ,npi.Provider_Name_Prefix_Text as NamePrefix
		                    ,npi.Provider_First_Name as NameFirst
		                    ,npi.Provider_Last_Name as NameLast
		                    ,npi.Provider_Name_Suffix_Text as NameSuffix
                            ,npi.Provider_Org_Name as OrgName
		                    ,npi.Provider_First_Line_Business_Practice_Location_Address as AddrLine1
                            ,npi.Provider_Second_Line_Business_Practice_Location_Address as AddrLine2
		                    ,npi.Provider_Business_Practice_Location_Address_City_Name as AddrCity
                            ,npi.Provider_Business_Practice_Location_Address_State_Name as AddrState
                            ,npi.Provider_Business_Practice_Location_Address_Postal_Code as AddrZip
                    From dbo.npidata as npi
                    Where npi.NPI = @npi
                    "
                  ;
            }
        }



        /*
         * Geo search for Providers (Individuals).
         * Parameters:
         * @zipcode  = Zip code that represents the center of geo search
         * @distance = Radius in miles from Zipcode center of search
         * 
         */
        public static string NPPESSQL_0010
        {
            get { return  @"
                    Declare @MetersPerMile FLOAT = 1609.344;
                    Declare @zipLat Float;
                    Declare @zipLong Float;

                    Select	@zipLat = Latitude,
		                    @zipLong = Longitude
                    From dbo.zipgeo zip
                    where zip.Zip = @zipcode
                    ;

                    Declare @geopoint as Geography;
                    SET @geopoint = geography::STPointFromText('POINT(' + CAST(@zipLong AS VARCHAR(20)) + ' ' +
		                    CAST(@zipLat AS VARCHAR(20)) + ')', 4326);

                    Select geo.NPI
		                    ,geo.Longitude
		                    ,geo.Latitude
                            ,concat(npi.Provider_Name_Prefix_Text, ' ',
		                            npi.Provider_First_Name,' ',
		                            npi.Provider_Last_Name, ' ',
		                            npi.Provider_Name_Suffix_Text) as ProviderName
		                    ,npi.Provider_First_Line_Business_Practice_Location_Address as AddrLine1
                            ,npi.Provider_Second_Line_Business_Practice_Location_Address as AddrLine2
		                    ,npi.Provider_Business_Practice_Location_Address_City_Name as AddrCity
                            ,npi.Provider_Business_Practice_Location_Address_Postal_Code as AddrZip
                    From dbo.npigeo as geo, dbo.npidata as npi
                    Where @geopoint.STDistance(geo.Geopoint) < @MetersPerMile*@distance
	                     and geo.NPI = npi.NPI
	                     and npi.Entity_Type_Code = 1;
                        "
                    ; }
        }

        /*
         * Geo search for Healthcare Providers (Organizations).
         * Parameters:
         * @zipcode  = Zip code that represents the center of geo search
         * @distance = Radius in miles from Zipcode center of search
         */
        public static string NPPESSQL_0020
        {
            get {
                return @"
                    Declare @MetersPerMile FLOAT = 1609.344;
                    Declare @zipLat Float;
                    Declare @zipLong Float;

                    Select	@zipLat = Latitude,
		                    @zipLong = Longitude
                    From dbo.zipgeo zip
                    where zip.Zip = @zipcode
                    ;

                    Declare @geopoint as Geography;
                    SET @geopoint = geography::STPointFromText('POINT(' + CAST(@zipLong AS VARCHAR(20)) + ' ' +
		                    CAST(@zipLat AS VARCHAR(20)) + ')', 4326);

                    Select geo.NPI
		                    ,geo.Longitude
		                    ,geo.Latitude
		                    ,npi.Provider_Org_Name as Org
		                    ,npi.Provider_First_Line_Business_Practice_Location_Address as AddrLine1
                            ,npi.Provider_Second_Line_Business_Practice_Location_Address as AddrLine2
		                    ,npi.Provider_Business_Practice_Location_Address_City_Name as AddrCity
                            ,npi.Provider_Business_Practice_Location_Address_Postal_Code as AddrZip
                    From dbo.npigeo as geo, dbo.npidata as npi
                    Where @geopoint.STDistance(geo.Geopoint) < @MetersPerMile*@distance
	                     and geo.NPI = npi.NPI
	                     and npi.Entity_Type_Code = 2
                        ";
            }
        }
    }
}
