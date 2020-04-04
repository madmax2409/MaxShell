from scapy.all import *

print("started sniffing")
p = sniff(count=1,filter="udp and host 192.168.1.214")
print (p)
