from scapy.all import *
import subprocess

def run_command(cmd):
    return subprocess.Popen(cmd, shell=True,
                            stdout=subprocess.PIPE,
                            stderr=subprocess.PIPE,
                            stdin=subprocess.PIPE).communicate()

output = run_command('arp -a')[0]
output = output.decode("utf-8")
        
print (output)



