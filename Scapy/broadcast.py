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
print (output)
reg = re.compile(r'((?:\d{1,3}.){3}\d{1,3})\s+((?:[\da-f]{2}-){5}[\da-f]{2})\s+dynamic')
ext = reg.findall(output)

port = str(find_free_port())
while True:
    for s in ext:
        run_command('ping' + s[0])
        print (s[0], str(port))
        data = s[0]+ str(port)
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        try:
            sock.sendto((bytes(data, 'utf-8')), (s[0], 10000))
        except:
            print ("failed")
        finally:
            time.sleep(1)
