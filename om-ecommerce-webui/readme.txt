Step to create angular multi-projects workspace

	ng new om-ecommerce-webui --create-application false
	cd om-ecommerce-webui
	ng generate application om-ecommerce-buyer-app
	ng generate application om-ecommerce-admin-app
	ng generate library ngx-account-library

Step to build angular multi-projects workspace

	ng build --> to build default app mentioned in angular.json file
	ng build om-ecommerce-buyer-app --configuration development
	ng build om-ecommerce-admin-app --configuration development
	ng build ngx-account-library --configuration development

Step to run angular multi-projects workspace

	ng serve --> to run default app mentioned in angular.json file
	ng serve om-ecommerce-buyer-app 
	ng serve om-ecommerce-admin-app

Step to use angular library locally with Hot Module Reload

	ng build ngx-account-library --watch
	OneTime:
	npm install file://E:\Repository\Github\om.ecommerce\om-ecommerce-webui\dist\ngx-account-library


Step to use builds and packages your code into a compressed file named using the name and version from your package.json

	npm run build ngx-account-library
	cd dist/ngx-account-library
	npm pack

	npm install /path/to/dist/ngx-account-library-0.0.1.tgz


npm package for okta SSO

	npm install @okta/okta-auth-js

npm package for AzureAd SSO

	npm i @azure/msal-angular