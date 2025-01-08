using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HttpDemo_1
{
    internal class Program
    {
        static HttpResponseMessage response;
        private static readonly HttpClient client = new HttpClient();
        // 服务器地址
        static string baseUrl = "http://localhost:3000/process?requestId=";
        static string url;
        static async Task Main(string[] args)
        {
            WriteLineFun();
            if (args != null && args.Length > 0) WriteLineFun($"收到传参 args = {args[0]}");
            else WriteLineFun($"args = null");

            client.Timeout = TimeSpan.FromSeconds(10); // 设置10秒超时
            await LaunchReq();
        }
       
        static async Task LaunchReq()
        {
            WriteLineFun();
            WriteFun($"输入请求数据：");
            string[] inputs = Console.ReadLine().Split('|');
            WriteLineFun();
            string[] paramList = null;
            if (inputs.Length > 2) { paramList = inputs.Skip(2).Take(inputs.Length - 2).ToArray(); }

            string reqType = inputs[0];
            string requestId = inputs[1];
            await reqFun(reqType, requestId, paramList);
        }


        static void WriteLineFun(string writeStr = null,int lineNum = 1 )
        {
            if(!string.IsNullOrEmpty(writeStr)) Console.WriteLine(writeStr);
            for (int i = 0; i < lineNum; i++) Console.WriteLine();
        }

        static void WriteFun(string writeStr = null)
        {
            if (!string.IsNullOrEmpty(writeStr)) Console.Write(writeStr);
        }

        static async Task reqFun(string reqType,string requestId, string[] data)
        {
            if (reqType == "GET" || reqType == "DELETE")
            {
                await normalReqFun(reqType, requestId, data);
            }
            else if (reqType == "POST" || reqType == "PUT")
            {
                await formReqFun(reqType, requestId, data);
            }
        }
        
        static async Task normalReqFun(string reqType,string requestId, string[] data = null)
        {
            try
            {
                url = $"{baseUrl}{requestId}";

                if (data != null && data.Length > 0)
                {
                    for (int i = 0; i < data.Length; i++) { url = $"{url}&param{i + 1}={data[i]}"; }
                }

                WriteLineFun($"发起 {reqType} 请求, Url = {url}");

                if (reqType == "GET")
                {
                    // 发送 GET 请求
                    response = await client.GetAsync(url);
                }
                else if (reqType == "DELETE")
                {
                    // 发送 DELETE 请求
                    response = await client.DeleteAsync(url);
                }
                
                // 确保请求成功
                //response.EnsureSuccessStatusCode();
                // 检查请求是否成功
                if (response.IsSuccessStatusCode)
                {
                    await responseFun(response);
                }
                else
                {
                    WriteLineFun($"请求失败 Code = {response.StatusCode}");
                    await LaunchReq();
                }
            }
            catch (HttpRequestException e)
            {
                WriteLineFun($"请求异常，Message = {e.Message}");
                await LaunchReq();
            }
        }


        static async Task formReqFun(string reqType,string requestId, string[] data)
        {
            try
            {
                //string jsonString = "{ \"message\": \"Hello, World!\", \"status\": \"success\", \"data\": { \"id\": 1, \"name\": \"Alice\" } }";
                // 创建数据对象
                //var data = new MyData
                //{
                //    Param1 = "value1",
                //    Param2 = "value2"
                //};

                //// 将数据序列化为 JSON
                //string json = JsonSerializer.Serialize(data);
                //var content = new StringContent(json, Encoding.UTF8, "application/json");

                url = $"{baseUrl}{requestId}";
                string jsonString = "";
                if (data.Length > 0)
                {
                    jsonString = "{";
                    for (int i = 0; i < data.Length; i++)
                    {
                        jsonString = jsonString + string.Format("\"param{0}\": \"{1}\"", i + 1, data[i]);
                        jsonString = (jsonString += (i == data.Length - 1 ? "}" : ","));
                    }
                }

                WriteLineFun($"发起 {reqType } 请求, Url = {url}   jsonString = {jsonString}");

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                if (reqType == "POST")
                {
                    // 发送 GET 请求
                    response = await client.PostAsync(url, content);
                }
                else if (reqType == "PUT")
                {
                    // 发送 POST 请求
                    response = await client.PutAsync(url, content);
                }

                
                // 确保请求成功
                //response.EnsureSuccessStatusCode();
                // 检查请求是否成功
                if (response.IsSuccessStatusCode)
                {
                    await responseFun(response);
                }
                else
                {
                    WriteLineFun($"请求失败 Code = {response.StatusCode}");
                    await LaunchReq();
                }
            }
            catch (HttpRequestException e)
            {
                WriteLineFun($"请求异常，Message = {e.Message}");
                await LaunchReq();
            }
        }
        
        static async Task responseFun(HttpResponseMessage response)
        {
            // 读取响应内容
            string responseBody = await response.Content.ReadAsStringAsync();
            WriteLineFun($"服务器响应 responseBody = {responseBody}", 3);
            await LaunchReq();
            //return;

            //var stream = await response.Content.ReadAsStreamAsync();

            //StreamReader reader = new StreamReader(stream);

            //string json = await reader.ReadToEndAsync();
            //var myObject = JsonConvert.DeserializeObject(json);

            //Dictionary<string, object> jsonobj = myObject as Dictionary<string, object>;
            //int code = -1;
            //int.TryParse(jsonobj["code"] as string, out code);
            //int responseId = -1;
            //int.TryParse(jsonobj["responseId"] as string, out responseId);
            //string message = jsonobj["message"] as string;
            //string paramsData = jsonobj["params"] as string;
            //WriteLineFun($"服务器响应 StatusCode = {response.StatusCode}  code = {code}  responseId = {responseId}  message = {message}  paramsData = {paramsData}");

        }
    }
}
