# Ngsoft.Wol.Standard
## Quick use
```c#
await WolBuilder
  .Create(localIpAddress, localPort, remoteMacAddress)
  .WakeUpAsync();
```
This operation creates magic packet and sends it to default broadcast address ```255.255.255.255```.  
Local IP address and port should be provided because of using ```UdpClient```.
## Extended use
If you want to send packet to another network, you can specify target IP address and subnet mask:
```c#
await WolBuilder
  .Create(localIpAddress, localPort, remoteMacAddress)
  .SetRemoteIpAddress(remoteIpAddress)
  .SetRemoteSubnetMask(remoteSubnetMask)
  .WakeUpAsync();
```
This operation creates magic packet and sends it to target broadcast address calculated with provided IP address and subnet mask.  
For example, for IP address ```192.168.0.1``` with subnet mask ```255.255.255.0``` will be produced ```192.168.0.255``` broadcast address.
