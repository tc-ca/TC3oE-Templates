param name string

resource sa 'Microsoft.Storage/storageAccounts@2021-06-01'={
  name: name
  kind: 'StorageV2'
  location: resourceGroup().location
  sku: {
    name: 'Standard_LRS'
  }
}

resource blobServices 'Microsoft.Storage/storageAccounts/blobServices@2021-06-01' = {
  parent: sa
  name: 'default'
}

resource container_dev 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-06-01' = {
  parent: blobServices
  name: '$web'
}

resource kv 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  location: resourceGroup().location
  name: name
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: [
    ]
  }
}

resource name_secret 'Microsoft.KeyVault/vaults/secrets@2021-06-01-preview' = {
  name: 'storage-account-name'
  parent: kv
  properties: {
    value: sa.name
  }
}

resource access_key_secret 'Microsoft.KeyVault/vaults/secrets@2021-06-01-preview' = {
  name: 'storage-account-access-key'
  parent: kv
  properties: {
    value: sa.listKeys().keys[0].value
  }
}

output name string = sa.name
