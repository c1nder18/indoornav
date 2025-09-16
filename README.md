 # Indoor AR Navigation System

这是一个基于 **Unity + AR Foundation + Immersal SDK** 的室内导航系统 Demo，应用于大学教学楼内的室内导航场景。

## 🎯 项目介绍
本项目通过 **AR 实时定位** + **路径规划** + **动态箭头指引**，实现了室内导航功能。  
用户进入建筑后，使用手机扫描场景，系统会自动定位，并在地面生成导航箭头指引前往目标房间。

## 🔧 技术栈
- **Unity 2023 / 2024**
- **URP 渲染管线**
- **AR Foundation + ARCore**
- **Immersal SDK**（点云地图与定位）
- **NavMesh + A\***（路径规划）
- **C# 脚本 + Shader (HLSL/ShaderLab)**

## 🚀 功能特点
- 实时定位（基于 Immersal 点云）
- 全局路径规划（Unity NavMesh）
- 本地路径优化（自定义 A* 算法）
- 动态箭头生成与路径可视化
- UI 界面选择导航目标
- 支持 Android 部署和测试



