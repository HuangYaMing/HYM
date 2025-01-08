const http = require('http');
const url = require('url');

const logFun = (writeStr = null, lineNum = 1 ) => {
    if (writeStr)  { console.log(writeStr); }
    for (let index = 0; index < lineNum; index++) { console.log() }
}

// 创建服务器
const server = http.createServer(async (req, res) => {
    res.setHeader( 'Content-Type','application/json' );
    // 解析请求 URL 和查询参数
    const parsedUrl = url.parse(req.url, true);
    const pathname = parsedUrl.pathname;
    const query = parsedUrl.query;

    logFun(`收到客户端请求，requestId = ${query.requestId}`);

    const respData = {
        code : -1,
        responseId : query.requestId,
        message : "",
        params:""
    }

    responFun = (reqType,state,code,message,params) => {
        logFun(`响应客户端请求，reqType = ${reqType}  state = ${state}  code = ${code}  requestId = ${query.requestId}`);
        respData.code = code
        respData.message = message
        respData.params = params
        res.writeHead(state,{ 'Content-Type': 'application/json' });
        res.end(JSON.stringify(respData));

        logFun(null,3)
    }

    if (pathname === "/process") 
    {
        if (req.method == "GET" || req.method == "DELETE")
        {
            // 处理 GET 请求 - 解析查询字符串参数
            responFun(req.method,200,0,'GET request received',query)
        }
        else if (req.method == "POST" || req.method == "PUT")
        {
            let body = '';

            // 监听 data 事件接收数据
            await req.on('data', chunk => { body += chunk.toString(); });
    
            // 监听 end 事件，在请求体数据接收完毕时解析 JSON
            await req.on('end', () => {
                try {
                    const jsonData = JSON.parse(body); // 将请求体解析为 JSON
                    responFun(req.method,200,0,req.method + ' request received',jsonData)
                } catch (error) {
                    responFun(req.method,400,400,`无效的JSON数据`,"")
                }
            });
        }
        else
        {
            // 处理未定义的路由
            responFun(`未定义`,404,404,`未定义的路由`,"")
        }
    }
    else
    {
        logFun(`pathname error = ${pathname}`);
        responFun(`未知服务`,404,404,`未知的服务`,"")
    }


    // 根据不同的请求 ID 处理逻辑
    // switch (requestId) {
    //     case '123':
    //         resData.message = `这是 ${requestId} 请求的响应`;
    //         break;
    //     case '456':
    //         resData.message = `这是 ${requestId} 请求的响应`;
    //         break;
    //     default:
    //         resData.message = `这是 未知 请求的响应`;
    // }

    // const resStr = JSON.stringify(resData)
    // logFun(`回包数据：${resStr}`)

    // 设置响应头和响应内容
    // res.writeHead(200, { 'Content-Type': 'application/json' });
    // res.end(resStr);
});

// 启动服务器
const PORT = 3000;
server.listen(PORT, () => {
    logFun()
    logFun(`服务器已启动，开始接受客户端请求，地址： http://localhost:${PORT}`,2);
});
