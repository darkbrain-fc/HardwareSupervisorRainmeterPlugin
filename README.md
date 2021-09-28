# HardwareSupervisorRainmeterPlugin

*Version 0.1.0*

HardwareSupervisorRainmeterPlugin is a Rainmeter Plugin used to show HardwareSupervisor sensors data.
It's based on PluginWMI made by E.Butusov <ebutusov@gmail.com> 
https://github.com/ebutusov/RainmeterPlugins/tree/master/PluginWMI

## Idea behind it ##
I was a lot frustrated to use an Administrator account to run all hardware monitoring
software like Open Hardware Monitor or others. For security reasons it's quite normal today to use a
normal account to achieve all day operations, so if you want to run hardware monitoring software you
have to enter administrator password to continue. But with a windows service this step is no more
necessary! To achieve this goal I've developed HardwareSupervisor: https://github.com/darkbrain-fc/HardwareSupervisor
but some widgets were needed to show information collected by HardwareSupervisor. 
So HardwareSupervisorRainmeterPlugin was born.

## WMI ##
HardwareSupervisorRainmeterPlugin uses Windows Management Instrumentation(WMI) protocol to show
data, so it's not necessary to use it with HardwareSupervisor, it can collect information
from other WMI sources.

## Install ##
Simply double click the .rmskin file or drag&drop it into the rainmeter dashboard, then press install.

## Configurator ##
Because Rainmeter uses ini files to configure plugins and they are quite strange to modify,
Configurator comes in hand to simplify ini file creation.
It's quite basic, you can specify a WMI namespace (`root\HardwareSupervisor` by default), and 
you can write your own WMI query (`SELECT * FROM Sensor` for example) and then you can press
*Search*. A simple list of keys and values will appear. Remove with *Canc* all rows that 
you don't want to see in HardwareSupervisorRainmeterPlugin, and edit *Name* entries to 
what you want. Then press *Generate* and HardwareSupervisor.ini file will be generated into
the Configurator directory. Now you can put it in
`C:\Users\<Your account>\Documents\Rainmeter\Skins\HardwareSupervisor` directory and then 
you can refresh the skin. 
It was designed to work with HardwareSupervisor so probably it will not work correctly 
with outhers namespaces, anyway you can tweak HardwareSupervisor.ini file to accomplish 
your needs.

## HardwareSupervisor ##
HardwareSupervisor: https://github.com/darkbrain-fc/HardwareSupervisor

## Future goals ##
* Add some eyecandy
* Enhance Configuration integration

