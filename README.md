# Dingus Labs Readme

## Licenses 
This project was built on top of ML-agents for unity: https://github.com/Unity-Technologies/ml-agents
As such, all licenses, expectations and notices as described here and in the base project

## Getting Started
I highly recommend getting started via https://unity-technologies.github.io/ml-agents/Getting-Started/
ML-agents does have a learning curve and a bit of an involved set-up process, I strongly recommend using conda too
Becuase of the obtuse nature of programming I am not available to provide support of the building, modification or training of any content within this repo

## Context
This repo and most of it's content was built extremely quickly with virtually no consideration for efficiency, good coding practices, good unity practices, any good practices at all. It is to be used as an insight for what can be done with reinforcement learning and MLagents, but not at all as an indication of HOW you should do it.

## Added content
The added content that is the dinguslabs content can be found in Projects/Assets/DingusLabsProjects, the new scenes and environments can be accessed from there

with additional resources from:
YughuesFreeMetalMaterials - https://assetstore.unity.com/packages/2d/textures-materials/metals/yughues-free-metal-materials-12949?srsltid=AfmBOooBQqj3DB1g9Rd_Uy7FYExq82Rk-o6APx8NA--b3sItkS3svjII
Snow Mountain - https://assetstore.unity.com/packages/3d/environments/landscapes/free-snow-mountain-63002?srsltid=AfmBOoqvO59nk_5P77TYzVheHLWKAxVm8y-i4VApkLpNhPRmgtWBn501

## Issue support
Becuase of the obtuse nature of programming I am not available to provide support of the building, modification or training of any content within this repo

However here is a bit of info that may help you if you encounter issues:
This repo is a frankenstein conglomeration of a great many different dinguslabs projects, it required a lot of adding and modifying layers, tags, resources, training files and their references. I Have done what I can to ensure all training scenes run and use the correct sensors/tags/layers however there may be errors, if an agent seems to not be learning check that the tags in vector sensors make sense and that they are interacting with correct layers.

The latest version of mlagents for unity broke the threaded training option which could be specified in the config files, I reached out with the bug however they unfortunately chose not to fix it, if you are having training that crashes shortly after starting the environment this could be it, remove the threaded option from the config yaml.

Training speed is directly correlated to how many agents are training at once, increase the number of agents and environments training at once with consideration to your machine's capabilities to try and increase training.

It is worth learning how training works on the very basic ml-agents demo environments first, jumping in and being confused at the more complex and cobbled together dinguslabs environments will likely not help you learn as fast.