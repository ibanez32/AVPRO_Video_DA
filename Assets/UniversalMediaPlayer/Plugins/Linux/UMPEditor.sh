#!/bin/sh

sudo apt-get install libswscale-ffmpeg3 libavcodec-ffmpeg56

sudo sh -c "echo 'Path_to_your_libvlc_libraries_x86_64' >> /etc/ld.so.conf.d/umpeditor.conf"
sudo ldconfig

sudo sh -c "echo 'Path_to_your_libvlc_libraries_x86' >> /etc/ld.so.conf.d/umpeditor.conf"
sudo ldconfig

#example
#sudo sh -c "echo '/home/pcname/Documents/MyProject/Assets/UniversalMediaPlayer/Plugins/Linux/x86_64' >> /etc/ld.so.conf.d/umpeditor.conf"
#sudo ldconfig

#sudo sh -c "echo '/home/pcname/Documents/MyProject/Assets/UniversalMediaPlayer/Plugins/Linux/x86' >> /etc/ld.so.conf.d/umpeditor.conf"
#sudo ldconfig
