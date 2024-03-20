USB\VID_0951&PID_1727&MI_00\a&14013ddc&0&0000

pnputil /enum-devices /instanceid "USB\VID_0951&PID_1727&MI_00\a&14013ddc&0&0000" /ids





Get-WmiObject Win32_USBControllerDevice |%{[wmi]($_.Dependent)} | Sort Manufacturer,Description,DeviceID | Ft -GroupBy Manufacturer Description,Service,DeviceID
Get-WmiObject Win32_PnPEntity | Sort Manufacturer,Description,DeviceID | Ft -GroupBy Manufacturer Description,Service,DeviceID