using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace QiCheng
{
    class Province
    {
        public string name = "未知省份";
        public int addressNum = 0;
        public int _5kgBox = 0;
        public int _10kgBox = 0;
        public int totalBox = 0;
        public int totalWeight = 0;
        public int totalPrice = 0;
    }

    internal class Program
    {
        static Dictionary<string, string> Freight = new Dictionary<string, string>();//运费
        static Dictionary<string, Province> provinceInfo = new Dictionary<string, Province>();//各省份统计
        static Dictionary<string, int> xingInfo = new Dictionary<string, int>();//姓氏统计
        static Dictionary<string, int> nameInfo = new Dictionary<string, int>();//一次性下单统计
        static string nameKey = "姓名：";
        static string phoneKey = "电话：";
        static string addressKey = "地址：";
        static string countKey = "数量：";
        static string summarizeKey = "结语：";
        static string summarizeContent = "";
        static int timeCount = 3;
        static string fileFilter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";//文件过滤
        static string fileTitle = "选择.txt";//标题
        static string boxKey = "纸箱";
        static string boxCostConfig = "2-3";

        [STAThread]
        static void Main(string[] args)
        {
            //Fun();
            Console.WindowWidth = 150;  // 设置控制台宽度
            Console.WindowHeight = 50;  // 设置控制台高度
            Utils.WriteColorFun("");
            Utils.ReadKeyFun(ConsoleKey.A); //按A键开始程序
            string filePath = Utils.SelectFile(fileFilter, fileTitle);
            if (string.IsNullOrEmpty(filePath))
            {
                //取消选择，3秒后退出
                System.Threading.Timer time = new System.Threading.Timer(TimerCallback,null,0,1000);
                Thread.Sleep(3000);
                time.Dispose();
                time = null;
            }
            else
            {
                try
                {
                    InitFreight();//初始化运费

                    Utils.WriteColorFun($"开始统计 => {Path.GetFileName(filePath)}");

                    string patternProvince = @"([一-龥]+(?:省|自治区|特别行政区))";//@"([一-龥]+(?:省|自治区|特别行政区))"; // 匹配省份名称 
                    Regex regexProvince = new Regex(patternProvince);
                    string patternShi = @"([一-龥]+(?:市))";
                    Regex regexShi = new Regex(patternShi);
                    string patternWeight = @"(\d+)(?=箱).*?(\d+)(?=斤)"; // 匹配箱数跟斤数
                    Regex regexWeight = new Regex(patternWeight);
                    string patternPrice = @"(-?\d+)(?=元)"; // 匹配价格 @"(\d+)(?=元)"  @"(-?\d+)(?=元)"
                    Regex regexPrice = new Regex(patternPrice);
                    string patternName = @"([\u4e00-\u9fa5]{1,1})"; // 匹配姓氏
                    Regex regexName = new Regex(patternName);

                    // 使用 StreamReader 逐行读取文件
                    StreamReader reader = new StreamReader(filePath);
                    string line;

                    Province provinceInstance = null; //省份实例
                    string logContent = ""; //log内容
                    bool isLog = false; //控制log输出
                    int orderNum = 0; //订单计数
                    string nameStr = "未知姓名"; //每笔订单的名字

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line)) { continue; } //跳过空白行

                        //姓名
                        if (line.Contains(nameKey))
                        {
                            nameStr = line.Replace(nameKey, "").Trim();
                            Match matchName = regexName.Match(nameStr);
                            if (matchName.Success)
                            {
                                string xing = matchName.Groups[1].Value;
                                if (xingInfo.ContainsKey(xing))
                                {
                                    xingInfo[xing]++;
                                }
                                else
                                {
                                    xingInfo[xing] = 1;
                                }
                                logContent = $"{xing}";
                            }
                            orderNum++;
                        }

                        // 省份
                        if (line.Contains(addressKey))
                        {
                            Match matchProvince = regexProvince.Match(line);
                            //直辖市特殊处理
                            if (line.Contains("北京市") || line.Contains("天津市") || line.Contains("上海市") || line.Contains("重庆市"))
                            {
                                matchProvince = regexShi.Match(line);
                            }
                            if (matchProvince.Success)
                            {
                                string province = matchProvince.Groups[1].Value;
                                if (provinceInfo.ContainsKey(province))
                                {
                                    provinceInstance = provinceInfo[province];
                                    provinceInstance.addressNum++;
                                }
                                else
                                {
                                    provinceInstance = new Province();
                                    provinceInstance.addressNum = 1;
                                    provinceInstance.name = province;
                                    provinceInfo[province] = provinceInstance;
                                }
                                logContent = $"{logContent}  {province}";
                            }
                        }

                        if (line.Contains(countKey))
                        {
                            //箱数、斤数
                            MatchCollection matchesWeight = regexWeight.Matches(line);
                            if (matchesWeight.Count > 0)
                            {
                                int weightTotal = 0;
                                foreach (Match match in matchesWeight)
                                {
                                    int quantity = int.Parse(match.Groups[1].Value);  // 箱数
                                    int weight = int.Parse(match.Groups[2].Value);   // 斤数
                                    if (provinceInstance != null)
                                    {
                                        if (weight == 10) { provinceInstance._5kgBox = provinceInstance._5kgBox + quantity; }
                                        else if (weight == 20) { provinceInstance._10kgBox = provinceInstance._10kgBox + quantity; }
                                        weightTotal = weightTotal + quantity * weight;
                                        provinceInstance.totalBox = provinceInstance.totalBox + quantity;
                                        provinceInstance.totalWeight = provinceInstance.totalWeight + quantity * weight;
                                    }

                                    logContent = $"{logContent}  {quantity}箱{weight}斤";
                                }
                                nameInfo[nameStr] = weightTotal;
                            }

                            //价格
                            Match matchPrice = regexPrice.Match(line);
                            if (matchPrice.Success)
                            {
                                // 匹配到的价格
                                int price = int.Parse(matchPrice.Groups[1].Value);
                                if (provinceInstance != null) { provinceInstance.totalPrice = provinceInstance.totalPrice + price; }
                                logContent = $"{logContent}  {price}元";
                                isLog = true;
                            }
                        }

                        //保存结语
                        if (line.Contains(summarizeKey)) { summarizeContent = line.Replace(summarizeKey,""); }

                        if (isLog)
                        {
                            Utils.WriteColorFun($"{orderNum}、 {logContent}", ConsoleColor.Green);
                            isLog = false;
                        }
                    }

                    //统计结果
                    StatisticsRsults(orderNum);
                }
                catch (IOException e)
                {
                    Utils.WriteColorFun("错误: " + e.Message, ConsoleColor.Red);
                }

                Utils.PauseFun("按任意键继续……");
            }
        }

        static void TimerCallback(object state)
        {
            Utils.WriteColorFun($"已取消,等待{timeCount}秒之后自动退出");
            timeCount--;
        }
        
        static void StatisticsRsults(int orderNum)
        {
            if (provinceInfo.Count > 0)
            {
                Utils.WriteColorFun("", interval: 1);
                Utils.WriteColorFun($"各省份统计：");

                //纸箱成本
                string[] boxCosts = boxCostConfig.Split('-');
                int _5kgBoxCost = int.Parse(boxCosts[0]);
                int _10kgBoxCost = int.Parse(boxCosts[1]);
                
                Province totalP = new Province();
                Province totalMax = new Province();
                Province totalMix = new Province();
                totalMix.addressNum = 1;
                int totalCost = 0;

                foreach (var item in provinceInfo)
                {
                    var p = item.Value;
                    if (p.addressNum >= totalMax.addressNum)
                    {
                        totalMax.name = p.name;
                        totalMax.addressNum = p.addressNum;
                    }
                    if (p.addressNum <= totalMix.addressNum)
                    {
                        totalMix.name = p.name;
                        totalMix.addressNum = p.addressNum;
                    }

                    totalP.addressNum = totalP.addressNum + p.addressNum;
                    totalP._5kgBox = totalP._5kgBox + p._5kgBox;
                    totalP._10kgBox = totalP._10kgBox + p._10kgBox;
                    totalP.totalBox = totalP.totalBox + p.totalBox;
                    totalP.totalWeight = totalP.totalWeight + p.totalWeight;
                    totalP.totalPrice = totalP.totalPrice + p.totalPrice;

                    //运费
                    string freight = "0-0";
                    if (Freight.ContainsKey(item.Key)) { freight = Freight[item.Key]; }
                    else { Utils.WriteColorFun($"{item.Key}运费未配置,暂不计算该省份运费成本！！！", ConsoleColor.Red); }
                    string[] freights = freight.Split('-');
                    int _5kgFreights = int.Parse(freights[0]);
                    int _10kgFreights = int.Parse(freights[1]);
                    int freightsCost = p._5kgBox * _5kgFreights + p._10kgBox * _10kgFreights;
                    //纸箱
                    int boxCost = p._5kgBox * _5kgBoxCost + p._10kgBox * _10kgBoxCost;
                    //该省份去除成本之后挣的钱
                    float profit = p.totalPrice - freightsCost - boxCost;
                    //去除成本之后该省份挣的单价
                    string unitPrice = (profit / p.totalWeight).ToString("F2");

                    string contentt = $"{item.Key}: {p._5kgBox}箱10斤，{p._10kgBox}箱20斤，共{p.totalBox}箱   总计：{p.totalWeight}斤   {p.addressNum}单    {p.totalPrice}元    快递{freightsCost}元     纸箱{boxCost}元     挣{profit}元     平均{unitPrice}元/斤";
                    Utils.WriteColorFun(contentt, ConsoleColor.Cyan);

                    //所有省份累计总成本
                    totalCost = totalCost + freightsCost + boxCost;
                }

                //最大最小单量相同省份
                string someP1 = "";
                string someP2 = "";
                foreach (var item in provinceInfo)
                {
                    var p = item.Value;
                    if (p.addressNum == totalMax.addressNum) { someP1 = string.IsNullOrEmpty(someP1) ? item.Key : $"{someP1}、{item.Key}"; }
                    if (p.addressNum == totalMix.addressNum) { someP2 = string.IsNullOrEmpty(someP2) ? item.Key : $"{someP2}、{item.Key}"; }
                }

                //姓氏
                int xingMaxCount = 1;
                int xingMixCount = 1;
                foreach (var item in xingInfo)
                {
                    if (item.Value >= xingMaxCount) { xingMaxCount = item.Value; }
                    if (item.Value <= xingMixCount) { xingMixCount = item.Value; }
                }
                string xingMax = "";
                string xingMix = "";
                foreach (var item in xingInfo)
                {
                    if (item.Value == xingMaxCount) { xingMax = string.IsNullOrEmpty(xingMax) ? item.Key : $"{xingMax}、{item.Key}"; }
                    if (item.Value == xingMixCount) { xingMix = string.IsNullOrEmpty(xingMix) ? item.Key : $"{xingMix}、{item.Key}"; }
                }

                //一次性下单
                int weightMax = 1;
                string nameContent = "";
                foreach (var item in nameInfo)
                {
                    if (item.Value >= weightMax) { weightMax = item.Value; }
                }
                foreach (var item in nameInfo)
                {
                    if (item.Value == weightMax) { nameContent = string.IsNullOrEmpty(nameContent) ? item.Key : $"{nameContent}、{item.Key}"; }
                }

                //总挣的钱
                float totalProfit = totalP.totalPrice - totalCost;
                //全部的平均单价
                string unitPrice2 = (totalProfit / totalP.totalWeight).ToString("F2");

                Utils.WriteColorFun("");
                Utils.WriteColorFun("所有统计:  成本(快递费+纸箱)");
                string contenttt = $"1、共 {orderNum} 个订单";
                contenttt = $"{contenttt}\n\n2、{totalP._5kgBox}箱10斤，{totalP._10kgBox}箱20斤，共{totalP.totalBox}箱     总计：{totalP.totalWeight}斤     {totalP.totalPrice}元      {totalCost}元成本     挣{totalProfit}元      平均{unitPrice2}元/斤";
                contenttt = $"{contenttt}\n\n3、发往全国 {provinceInfo.Count} 个省份，单量最多省份：{someP1} ({totalMax.addressNum})单     最少省份：{someP2} ({totalMix.addressNum})单";
                contenttt = $"{contenttt}\n\n4、客户姓氏 {xingInfo.Count} 种，姓氏最多: {xingMax} ({xingMaxCount})个客户     姓氏最少: {xingMix} ({xingMixCount})个客户";
                contenttt = $"{contenttt}\n\n5、一次性下单最多：{nameContent}(先生/女士)    下单：{weightMax} (斤)";
                Utils.WriteColorFun(contenttt, ConsoleColor.DarkYellow);

                //打印结语
                if (!string.IsNullOrEmpty(summarizeContent)) 
                {
                    Utils.WriteColorFun("");
                    Utils.WriteColorFun(summarizeContent, ConsoleColor.Yellow);
                    Utils.WriteColorFun("");
                }
            }
            else
            {
                Utils.WriteColorFun("无数据!!！", ConsoleColor.Red);
            }
        }
        //运费
        static void InitFreight()
        {
            Freight.Clear();
            bool setFreight = false;
            bool setBoxCost = false;
            int result = Utils.ReadKeyFun2(ConsoleKey.B, ConsoleKey.C,"选择运费配置文件","使用程序默认运费");
            if (result == 1)
            {
                string freightPath = Utils.SelectFile(fileFilter, fileTitle);
                if (!string.IsNullOrEmpty(freightPath))
                {
                    StreamReader reader = new StreamReader(freightPath);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line)) { continue; } //跳过空白行
                        if (line.Contains(boxKey))
                        {
                            boxCostConfig = line.Split('=')[1].Trim();
                            setBoxCost = true;
                        }
                        else
                        {
                            string[] content = line.Split('=');
                            string key = content[0].Trim();
                            string value = content[1].Trim();
                            Freight.Add(key, value);
                        }
                    }
                    if (Freight.Count > 0 ) { setFreight = true; }
                }
                
            }

            if (!setBoxCost)
            {
                Utils.WriteColorFun($"未提供纸箱成本配置，使用程序默认", ConsoleColor.Yellow);
            }

            string[] box = boxCostConfig.Split('-');
            Utils.WriteColorFun($"纸箱: 10斤 {box[0]}元/个  20斤 {box[1]}元/个", ConsoleColor.Yellow);

            if (!setFreight)
            {
                Utils.WriteColorFun("未提供运费配置，使用程序默认（2024京东快递）:", ConsoleColor.Yellow);
                Freight["江西省"] = "11-17";
                Freight["广东省"] = "11-17";
                Freight["湖南省"] = "11-17";
                Freight["湖北省"] = "11-17";
                Freight["福建省"] = "11-17";
                Freight["江苏省"] = "11-17";
                Freight["安徽省"] = "11-17";
                Freight["浙江省"] = "11-17";
                Freight["上海市"] = "11-17";
                Freight["北京市"] = "15-22";
                Freight["天津市"] = "18-26";
                Freight["山东省"] = "18-26";
                Freight["山西省"] = "18-26";
                Freight["河北省"] = "18-26";
                Freight["广西壮族自治区"] = "18-26";
                Freight["河南省"] = "18-26";
                Freight["贵州省"] = "27-46";
                Freight["四川省"] = "27-46";
                Freight["重庆市"] = "27-46";
                Freight["陕西省"] = "27-46";
                Freight["甘肃省"] = "27-46";
                Freight["宁夏回族自治区"] = "27-46";
                Freight["云南省"] = "27-46";
                Freight["黑龙江省"] = "27-46";
                Freight["吉林省"] = "27-46";
                Freight["辽宁省"] = "27-46";
                Freight["内蒙古自治区"] = "27-46";
                Freight["海南省"] = "27-46";
                Freight["新疆维吾尔族自治区"] = "60-100";
                Freight["西藏自治区"] = "60-100";
                Freight["青海省"] = "60-100";
            }

            foreach (var item in Freight)
            {
                string[] s = item.Value.Split('-');
                Utils.WriteColorFun($"{item.Key}: 10斤{s[0]}元  20斤{s[1]}元", ConsoleColor.Yellow);
            }
            Utils.WriteColorFun("");
        }
    }
    class Utils
    {
        public static void TestFun()
        {

        }
        public static bool ReadKeyFun(ConsoleKey key = default(ConsoleKey), string tipContent = "继续", int num = 0)
        {
            string keyContent = $"{(key == default(ConsoleKey) ? "任意" : key.ToString())}";
            WriteColorFun($"按 {keyContent} 键{tipContent}", interval: 0);
            ConsoleKeyInfo info = Console.ReadKey(true);
            if (key == default(ConsoleKey))
            {
                WriteColorFun("");
                return true;
            }
            else
            {
                if (info.Key == key)
                {
                    if (num >= 3) { WriteColorFun($"哥，你是真看不懂啊，这都第{num + 1}次了"); }
                    else { WriteColorFun(""); }
                    return true;
                }
                else
                {
                    num += 1;
                    WriteColorFun($"哥，{SplicingContent(num, keyContent)}");
                    return ReadKeyFun(key, tipContent, num);
                }
            }
        }
        public static int ReadKeyFun2(ConsoleKey key1 = default(ConsoleKey), ConsoleKey key2 = default(ConsoleKey), string tipContent1 = "继续", string tipContent2 = "继续")
        {
            string keyContent1 = $"{(key1 == default(ConsoleKey) ? "任意" : key1.ToString())}";
            string keyContent2 = $"{(key2 == default(ConsoleKey) ? "任意" : key2.ToString())}";
            WriteColorFun($"按 {keyContent1} 键{tipContent1},或者 {keyContent2} 键{tipContent2}");
            ConsoleKeyInfo info = Console.ReadKey(true);
            if (info.Key == key1)
            {
                return 1;
            }
            else if (info.Key == key2)
            {
                return 2;
            }
            else
            {
                return ReadKeyFun2(key1, key2, tipContent1, tipContent2);
            }
        }
        public static void PauseFun(string logContent)
        {
            WriteColorFun(logContent);
            Console.ReadLine();
        }
        public static void WriteColorFun(string logContent = "", ConsoleColor color1 = ConsoleColor.White, ConsoleColor color2 = ConsoleColor.White, int interval = 1)
        {
            Console.ForegroundColor = color1;
            Console.WriteLine(logContent);
            for (int i = 0; i < interval; i++) { Console.WriteLine(); }
            Console.ForegroundColor = color2;
        }
        public static string SplicingContent(int num, string keyContent)
        {
            string logContent = "";

            for (int i = 0; i < num; i++)
            {
                logContent = $"{logContent}{(i == 0 ? "" : "、")}{keyContent}键";
            }
            return logContent;
        }

        public static string SelectFile(string filter,string title)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 设置过滤器，限定选择的文件类型为 .txt 文件
            openFileDialog.Filter = filter;
            openFileDialog.Title = title;

            // 显示对话框并检查用户是否选择了文件
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                // 获取选择的文件路径
                string selectedFilePath = openFileDialog.FileName;
                WriteColorFun($"选择的文件路径：{selectedFilePath}", ConsoleColor.Blue);
                return selectedFilePath;
            }
            else if (result == DialogResult.Cancel)
            {
                return "";
            }
            else
            {
                return SelectFile(filter, title);
            }
        }
    }
}
