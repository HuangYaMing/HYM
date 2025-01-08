using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HttpDemo_2
{
    internal class Program
    {
        static HttpClient client = new HttpClient();
        static HttpResponseMessage response;
        static string baseUrl = "http://localhost:3000/process?requestId=";
        static string url;
        static async Task Main(string[] args)
        {
            client.Timeout = TimeSpan.FromSeconds(10); // 设置10秒超时
            //HttpResponseMessage response = await client.GetAsync("http://localhost:3000/process?requestId=123");
            //string responseData = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseData);
            //Console.ReadLine();
            await LaunchReq();
        }
        static async Task LaunchReq()
        {
            try
            {
                Console.WriteLine($"输入请求ID");
                string[] inputs = Console.ReadLine().Split('|');
                string requestId = inputs[0];
                //string[] paramList = new string[inputs.Length - 1];
                //for (int i = 1; i < inputs.Length; i++)
                //{
                //    paramList[i - 1] = inputs[i];
                //}

                url = $"{baseUrl}{requestId}";

                Console.WriteLine($"发起 GET 请求 url = {url}");

                // 发送 GET 请求
                response = await client.GetAsync(url);
                // 检查是否成功响应
                //response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    // 读取并输出响应内容
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //Dictionary<string, object> jsonobj = JsonConvert.DeserializeObject(responseBody) as Dictionary<string, object>;
                    //int code = -1;
                    //int.TryParse(jsonobj["code"] as string, out code);
                    //int responseId = -1;
                    //int.TryParse(jsonobj["responseId"] as string, out responseId);
                    //string message = jsonobj["message"] as string;
                    //string paramsData = jsonobj["params"] as string;
                    //Console.WriteLine($"收到服务器响应 StatusCode = {response.StatusCode}  code = {code}  responseId = {responseId}  message = {message}  paramsData = {paramsData}");
                    //await LaunchReq();
                    Console.WriteLine(responseBody);
                }
                else
                {
                    Console.WriteLine($"请求失败 Code = {response.StatusCode}");
                }
            }
            catch (HttpRequestException e)
            {
                // 捕获请求异常
                Console.WriteLine("Request error: " + e.Message);
            }

        }
    }
}
