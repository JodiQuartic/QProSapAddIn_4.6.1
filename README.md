# SapHanaAddIn
An Esri ArcGIS Pro addin for connecting to and exploring SAP HANA databases.

Download code from github.

Before connecting to a HANA database using the SapHanaAddIn a user must setup the addin options as follows:
  1. Create an xml file containing server names and connection strings for your specific site. A sample file is included. Start by copying ...\Projects\SapHanaAddIn-master\SapHanaAddIn\HANAServers\HANAProperties.xml to the local arcgis pro addin directory:   C:\Users\xxxxx\Documents\ArcGIS\AddIns\ArcGISPro2.3\HANAProperties.xml
  2. In ArcGIS Pro click on the OPtions tab then  OPtions then Hana Properties
  3. Fill in the xml file location and the user name and password to be used
  
Once the Hana Properties section has been completed a user open the addin tab "HANA Tools"
  1. Click on the drop down and a list of Servers to connect to should appear
  2. Click connect. A connection will be made to that HANA server using the usewr name and password entered in the HANA properties section
  3. Click on the Explor tool 
  4. Click on a schema from the schema dropdown and choose a table
  5. Records should be displayed.
