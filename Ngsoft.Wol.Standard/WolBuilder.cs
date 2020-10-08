using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Ngsoft.Wol
{
    public class WolBuilder
    {
        private readonly UdpClient _udp;
        private readonly PhysicalAddress _macAddress;
        private IPAddress _ipAddress;
        private IPAddress _subnetMask;
        private int _port = 9;

        public WolBuilder(IPAddress localIpAddress, int localPort, PhysicalAddress remoteMacAddress)
        {
            if (localIpAddress == null)
            {
                throw new ArgumentNullException(nameof(localIpAddress));
            }
            if (localPort <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(localPort), "Local port value cannot be negative or zero.");
            }
            if (remoteMacAddress == null)
            {
                throw new ArgumentNullException(nameof(remoteMacAddress));
            }

            _udp = new UdpClient(localEP: new IPEndPoint(localIpAddress, localPort));
            _macAddress = remoteMacAddress;
        }

        public static WolBuilder Create(IPAddress localIpAddress, int localPort, PhysicalAddress remoteMacAddress)
        {
            return new WolBuilder(localIpAddress, localPort, remoteMacAddress);
        }

        public WolBuilder SetRemoteIpAddress(IPAddress ipAddress)
        {
            _ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            return this;
        }

        public WolBuilder SetRemoteSubnetMask(IPAddress subnetMask)
        {
            _subnetMask = subnetMask ?? throw new ArgumentNullException(nameof(subnetMask));
            return this;
        }

        public WolBuilder SetRemotePort(int port)
        {
            _port = (port > 0) ? port : throw new ArgumentOutOfRangeException(nameof(port), "Remote port value cannot be negative or zero.");
            return this;
        }

        public async Task WakeUpAsync()
        {
            using (_udp)
            {
                byte[] magicPacket;
                // Create magic packet content.
                using (var memoryStream = new MemoryStream())
                {
                    byte[] syncStream = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                    memoryStream.Write(buffer: syncStream, offset: 0, count: syncStream.Length);
                    var macAddress = _macAddress.GetAddressBytes();
                    for (int i = 0; i < 16; i++)
                    {
                        memoryStream.Write(buffer: macAddress, offset: 0, count: macAddress.Length);
                    }
                    magicPacket = memoryStream.ToArray();
                }
                IPEndPoint remoteEndPoint;
                if (_ipAddress != null && _subnetMask != null)
                {
                    // Get broadcast address by remote IP address & subnet mask.
                    var address = BitConverter.ToInt32(value: _ipAddress.GetAddressBytes(), startIndex: 0);
                    var subnetMask = BitConverter.ToInt32(value: _subnetMask.GetAddressBytes(), startIndex: 0);
                    var broadcast = address | ~subnetMask;
                    var broadcastAddress = new IPAddress(address: BitConverter.GetBytes(broadcast));
                    remoteEndPoint = new IPEndPoint(address, _port);
                }
                else
                {
                    // Use default broadcast address.
                    remoteEndPoint = new IPEndPoint(address: IPAddress.Broadcast, _port);
                }
                await _udp.SendAsync(datagram: magicPacket, bytes: magicPacket.Length, endPoint: remoteEndPoint);
            }
        }
    }
}
