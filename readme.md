# NPPES-REST-API
### **Project:** ASP.NET Core 3.1 RESTful API against the NPPES Data
### **Author**:     [Neil Harvey](https://www.linkedin.com/in/neil-harvey-07009a2a/)
### **Date**:       May 2020
### **Tools**:    C#, Transact-SQL, SQL Server 2019
### **What is NPPES-REST-API?**
The National Plan and Provider Enumeration System (NPPES) maintains identifiers for all healthcare providers in the United States.  Each provider obtains a National Provider Identifier (NPI) stored in the NPPES.  This data provides detailed information about each provider, such as licensing, specialties, practice locations, points of contact, etc..  
This project provides the ability to find Healthcare providers that are within a specified distance in miles from a specified zip code.  The API is dependent upon the NPPES-Data-Load project for data.  The NPPES-Data-Load project uses the Federal Census geocoder API to geocode each Healthcare Provider's business address.  In some cases the Census system cannot find the exact geocode for an address, especially for some commercial locations.  In those instances the geocode used was taken from the zip code of the address.  Accuracy of geocoding was not my objective, it was using the geographic indexing capabilities of SQL Server and leveraging these in a RESTful API.

5/13/2020 - I am still adding features, so this project is ongoing.

#### Project Goals
- Create a RESTful API that leverages geocode indexing capabilities of SQL Server 2019.
- Leverage various Microsoft C# .Net data access methods (SQLDataReader, DataSet, Entity Framework).
- Leverage Transact-SQL stored procedures.
- Create a Model that leverages the Builder/Director pattern in C#.

#### How to use the API

- You must first implement the NPPES-Data-Load project to obtain a geocoded database of healthcare providers.
- Use a tool like Postman to invoke the api with GET on the following: https://localhost:44310/api/nppes/?zipcode=94044&distance=1
Change the zipcode as required, change the distance in miles as required.  There are currently no boundaries on number of Providers returned, so if you use
too wide a radius or a very densely populated zip code, you could find a very large number of Providers returned, or run out of resources.  
My version of the NPPES database is limited to California healthcare providers, so only California zip codes will provide a result.
- To retrieve a single, known Provider, invoke the api with GET on the following: https://localhost:44310/api/nppes/1609164599
Where the number at the end of the URL is the desired National Provider ID (NPI).
- Sample json response files are:
  - SampleAPIresponseZipcodeOneMile.json  (?zipcode=94044&distance=1)
  - SampleAPIresponseNPI.json

Note: Obviously the URL above will change depending upon how you run the program.


  #### See Also
  ### **NPPES-Data-Load** - process to load and geocode NPPES NPI data for Healthcare Providers.
  ### **CAHealthFacilityDBLoad** - process to load California healthcare facility, services, beds information to SQL Server.
  ### **CAHealthQueries** - C# queries against healthcare data.
