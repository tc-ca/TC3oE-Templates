Write-Host "Deploying resources";
$outputs = az deployment group create `
	--name "main" `
	--resource-group "myResourceGroup" `
	--template-file "main.bicep" `
	--parameters "@main.parameters.json" `
	--subscription "mySubscription" `
| ConvertFrom-Json;

Write-Host "Enabling static website hosting";
az storage blob service-properties update `
	--account-name $outputs.properties.outputs.name.value `
	--static-website `
	--404-document "index.html" `
	--index-document "index.html" `
	--subscription "mySubscription";