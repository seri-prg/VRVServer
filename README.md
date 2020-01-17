# VRVServer

## What's this?
Head Mounted Display server system for Unity using android.  
Send streaming images from windows application and receive head tracking information from Android.  
![work_image](http://drive.google.com/uc?export=view&id=11nvW7HNptyBd0bJbXEqdW3aFT4RygGW3)

Android client is this.
https://play.google.com/store/apps/details?id=com.seri.hmdclient


demo play
https://www.youtube.com/watch?v=SZCVLgLQu1M


## How to setting

1. Add VRVController in same Gameobject as camera.
![insert_component](http://drive.google.com/uc?export=view&id=1GxA0WWejB7x7Rvl4xpiy8dCTc63ily2e)

2. Set a lens material.
![set_material](http://drive.google.com/uc?export=view&id=19eMjxGDNjke9dSrZDsjL40Vj9l4Jw1eq)

3. Set a Transform that reflects the head tracking angle.
![set_rot](http://drive.google.com/uc?export=view&id=1uzq7VVe5Gk-cPYvvTskv67szkpE2DpJ0)

4. Set other values as needed.

|name|Description|
----|----
|Port|Network listening port|
|Client UV Scale|client image scale|
|Bitrate|encoding bitrate|
|Use Cpu|[0-16] The lower the value, the higher the quality and the slower|

## Demo
This demo is a Unity Particle Tutorial with VRVServer added.  
https://drive.google.com/open?id=1-G6OTZO2tuOLOmh7RrehdE60sZNmuQ_z  
![screen_shot](http://drive.google.com/uc?export=view&id=12UK5TBf-BnqvtwT-878_EDp9p4Dkr_Ya)  

Original  
https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-particle-pack-127325

### how to play
1. download demo and play hmd_test.exe. 
2. input ip address to hmd client for android. 
![client_setting](http://drive.google.com/uc?export=view&id=1SoBqVSSisKGCfLCijd1LY4LPoBCBCeZT)

3. push "input IP And Port" button. 

