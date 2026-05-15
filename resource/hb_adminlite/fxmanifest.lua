fx_version 'bodacious'
games {'gta5'}

name 'hb_adminlite'
description 'Lightweight HoneyBee admin tool without MenuAPI'
version 'v0.1.0'

ui_page 'ui/index.html'

client_script 'hbAdminLiteClient.net.dll'
server_script 'hbAdminLiteServer.net.dll'

files {
    'permissions.cfg',
    'config/permissions.cfg',
    'config/vehicle_catalog.json',
    'ui/index.html',
    'ui/app.js',
    'ui/styles.css'
}
