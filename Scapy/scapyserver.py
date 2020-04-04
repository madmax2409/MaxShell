from scapy.all import *

ping_packet= IP(dst="255.255.255.255")/UDP()/"ip, port"
result = send(ping_packet, return_packets=True) # return_packets=True)
print (result)
