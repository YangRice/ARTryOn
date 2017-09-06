# ARTryOn
AR Try On based on Kinect

## 建置需求
使用Unity 2017.1編譯
使用C#撰寫

## 硬體需求
- Windows 8/8.1/10 64bit
- Microsoft Kinect v2

## Scene
主體Scene位於`./Assets/MainProgram/Main.unity`
主要的追蹤程式位於`./Assets/MainProgram/ClothOverlayer.cs`
試穿的衣服模型位於`./Assets/Resources/t+shirts.fbx`

## 執行方法
1. 連接Kinect v2後執行程式
2. 使用者進入Kinect追蹤範圍，衣服會自動穿上
3. 於JointOverlayer

## ClothOverlayer.cs

> public float shoulderScale = 1.0f;  
> public float breastScale = 1.0f;  
> public float hipScale = 1.0f;  
> public float tallScale = 1.0f;  
> public Transform shoulderAnchor;  
> public Transform breastAnchor;  
> public Transform hipAnchor;  

shoulderScale, breastScale, hipScale, tallScale 可以手動控制衣服的變形，根據試穿者的體型而定  
shoulderAnchor, breastAnchor, hipAnchor 為變形的控制錨點，取跟衣服物件的相對位置  
