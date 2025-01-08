@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

::当前路径
set currPath=!cd!
echo 当前路径：%currPath%

::启动 Node.js
set serverDir=HttpServer.js
echo 服务器路径：%serverDir%
start "服务器" "node" "%serverDir%"

::等待几秒钟，确保 Node.js 启动成功
timeout /t 1 /nobreak

:: 新开一个窗口运行 .exe 文件
set clientDir=bin\Debug\HttpDemo-1.exe
echo 客户端路径：%clientDir%
start "客户端" "%clientDir%" 11 22