@echo off
chcp 65001 >nul

REM 传参进来的js路径
set filePath=%1

REM 判断路径是否为空
if "%filePath%"=="" (
    echo filePath is empty!
	pause
) else (

	REM 检查 js-beautify 是否已全局安装
	npm list -g js-beautify >nul 2>&1

	REM 如果没有安装 js-beautify，返回的错误代码为 1
	if %errorlevel% neq 0 (
		echo js-beautify 没有安装，正在安装...
		npm install -g js-beautify
		
		REM 确保安装完成后再执行后续逻辑
		if %errorlevel% neq 0 (
			echo 安装失败，请检查 npm 或网络连接。
			exit /b 1
		)
	
	) else (
		echo js-beautify 已经全局安装。
	)
	
    REM node "%~dp0\Beautify.js" %filePath%

	REM 执行 js-beautify 格式化 myfile.js 文件
	js-beautify %filePath% --indent-size 2 --type js --replace

	REM 检查是否执行成功
	if %errorlevel% neq 0 (
		echo Error: %filePath% 格式化失败
	) else (
		echo Success: %filePath% 格式化成功
	)
)
