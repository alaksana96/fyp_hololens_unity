# Unity Project for Hololens

## Introduction

This repository contains the Unity project that is build and deployed onto the Microsoft Hololens.

## Goals

* [x] Establish ROS# Communication to a rosbridge server running on Linux box.
* [ ] Access camera stream in Unity and send images over ROS#
* [ ] Receive communication from ROS node with co-ordinates of faces and draw face pose estimations in AR

* [ ] Access Research mode sensor data in Unity

## Issues

### Text Mesh Pro & Urdf

I opened an issue on the [ROS# Project](https://github.com/siemens/ros-sharp/issues/193). The following was what I had to do to fix it.

>For anyone else facing similar problems, I believe I have partly solved the issue. When I attempt to build the project in Unity, I get the two errors from above followed by a build complete:
>
>![image](https://user-images.githubusercontent.com/17803005/56287900-7b28f500-6115-11e9-8923-b3eba96c8d61.png)
>
>`Build completed with a result of 'Succeeded' UnityEngine.GUIUtility:ProcessEvent(Int32, IntPtr)`
>
>A solution pops up in the App folder, which I can open in VS2017. When I attempt to deploy the program onto the Hololens, the following happens:
>
>![image](https://user-images.githubusercontent.com/17803005/56287626-cb538780-6114-11e9-95f3-c7ee97ca2d19.png)
>
>This seems to be an issue with Textmeshpro.
>
>This [Stack Exchange](https://gamedev.stackexchange.com/questions/162445/upgrade-to-unity-2018-2-2f1-resulting-in-missing-file-errors-from-visual-studio) thread solves the issue, and allows me to build and deploy the project to the Hololens. I can then check my rosbridge on the other computer and see a connection. [This](https://answers.unity.com/questions/1529584/cant-use-namespace-tmpro-with-the-textmeshpro-pack.html) sort of explains why the files dont get copied over.