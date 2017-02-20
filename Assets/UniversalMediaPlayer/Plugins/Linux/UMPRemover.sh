#!/bin/sh
 
cd "$(dirname "$0")"
sudo sh -c "rm -f /etc/ld.so.conf.d/ump.conf"
sudo ldconfig
sudo apt-get remove libswscale-ffmpeg3 libavcodec-ffmpeg56
sudo apt-get autoremove
