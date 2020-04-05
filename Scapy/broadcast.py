import socket
import subprocess
import re
import time
from contextlib import closing

def run_command(cmd):
    return subprocess.Popen(cmd,
        shell=True, # not recommended, but does not open a window
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        stdin=subprocess.PIPE).communicate()

def find_free_port():
    with closing(socket.socket(socket.AF_INET, socket.SOCK_STREAM)) as s:
        s.bind(('', 0))
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        return s.getsockname()[1]

output = run_command('arp -a')[0]
output = output.decode('utf-8')
reg = re.compile(r'((?:\d{1,3}.){3}\d{1,3})\s+((?:[\da-f]{2}-){5}[\da-f]{2})\s+dynamic')
ext = reg.findall(output)

port = str(find_free_port())
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.setsockopt(socket.SOL_SOCKET,socket.SO_REUSEADDR,1)
data = 'localhost'+ str(port)
while True:
    sock.sendto((bytes(data, 'utf-8')), ('192.168.1.249', 10000))  # s[o]
    print ('tried sending')
    time.sleep(1)
# while True:
   #  for s in ext:
        # print (s[0], str(port))
        # data = s[0]+ str(port)
        
        # time.sleep(2)
