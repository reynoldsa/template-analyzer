@description('Location for all resources.')
param location string

resource linuxKind 'Microsoft.Web/sites@2019-08-01' = {
  kind: 'linux'
  name: 'linuxKind'
  location: location
  properties: {
  }
}

resource withoutSpecifyingProperties 'Microsoft.Web/sites@2019-08-01' = {
  kind: 'api'
  name: 'withoutSpecifyingProperties'
  location: location
  properties: {
  }
}

resource undesiredFtpsState 'Microsoft.Web/sites@2019-08-01' = {
  kind: 'api'
  name: 'undesiredFtpsState'
  location: location
  properties: {
    siteConfig: {
      ftpsState: 'undesiredValue'
    }
  }
}