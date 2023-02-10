# CaaS
Cart as a Service

# Startup Guide
* Create database file
* Connect to database in Visual Studio
* Change connection string in the appsettings.json file in CaaS.Core and CaaS.Api to that databases connection string
* Configure CaaS.Core as startup project
* Run CaaS.Core in debug mode to insert sample data (this will take a while ~20s)
* Configure CaaS.Api as startup project
* Run CaaS.Api
* Your web browser should open with the created shops
* Open Frontend projekt
* Copy the Guid from 'Herberts Electronic Stuff'
* Paste this Guid in the frontend projekt at environments.ts 'const shopId: string = 'theGuid'' located under /Shop/src/environments/environments.ts
* Now you can start the frontend with ng serve (within Location /Frontent/Shop)
* keycloac is disaled you need no passwords
