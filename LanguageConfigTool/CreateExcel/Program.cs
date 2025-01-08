using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace CreateExcel
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            JsToExcel.Start();
        }
    }
    class JsToExcel
    {
        static string useColumns = "C";
        static int columnsLen = 3;
        public static void Start()
        {
            string jPath = GetJsPath();
            string ePath = GetExcelPath();
            bool newExcel = string.IsNullOrEmpty(ePath);

            if (newExcel)
            {
                //未选择Excel，主动创建
                ePath = $"{Path.GetDirectoryName(jPath)}\\多语言配置表.xlsx";
                File.Create(ePath).Dispose();
            }

            Dictionary<string, int> tDic = new Dictionary<string, int>();

            // 设置 EPPlus 许可证上下文
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 或者 LicenseContext.Commercial
            using (ExcelPackage package = new ExcelPackage(new FileInfo(ePath)))
            {
                ExcelWorkbook workbook = package.Workbook; //获取工作表集合
                ExcelWorksheet worksheet = null;
                int Columns = 0;
                int Rows = 0;
                if (newExcel)
                {
                    Columns = columnsLen;
                    Rows = 1;
                    worksheet = workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells[$"A1"].Value = "Key";
                    for (int i = 1; i <= Columns; i++)
                    {
                        worksheet.Column(i).Width = 35;
                    }
                }
                else
                {
                    worksheet = workbook.Worksheets[0];
                    Columns = worksheet.Dimension.Columns;
                    Rows = worksheet.Dimension.Rows;

                    if (Columns > 0 && Rows > 0)
                    {
                        for (int i = 2; i <= Rows; i++)
                        {
                            int index = i;
                            var key = worksheet.Cells[$"A{i}"].Value.ToString().Trim();

                            if (tDic.ContainsKey(key))
                            {
                                tDic[key] = index;
                            }
                            else
                            {
                                tDic.Add(key, index);
                            }
                        }
                    }
                }

                Dictionary<int, List<string>> diffDic = new Dictionary<int, List<string>>();

                StreamReader reader = new StreamReader(jPath); // 使用 StreamReader 逐行读取文件
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line)) { continue; }
                    else
                    {
                        line = line.Trim();
                        line = line.Replace("\\\"", Utils.useChar); //防止翻译里面有 \" ,这里先用自定义字符替换，下面再替换回来

                        Match matchLanguage = Regex.Match(line, Utils.patternLanguage);
                        Match match = Regex.Match(line, Utils.patternKeyValue);

                        //Utils.WriteColorFun($"匹配结果：{match.Success}   {line}", color1: (match.Success ? ConsoleColor.Green : ConsoleColor.Red));

                        if (matchLanguage.Success)
                        {
                            //设置第一行的语言类型
                            string language = matchLanguage.Groups[1].Value.Trim();
                            worksheet.Cells[$"{useColumns}1"].Value = language;
                        }
                        else if (match.Success)
                        {
                            string key = match.Groups[1].Value.Trim();
                            string value = match.Groups[2].Value.Trim();
                            value = value.Replace(Utils.useChar, "\\\"");
                            //Utils.WriteColorFun($"Key: {key}, Value: {value}");

                            if (tDic.ContainsKey(key))
                            {
                                int row = tDic[key];
                                string excelValue = worksheet.Cells[$"{useColumns}{row}"].Value.ToString().Trim();
                                if (!value.Equals(excelValue))
                                {
                                    //Utils.WriteColorFun($"有差异 Key: {key}\n配置表：{excelValue}\n前端js：{value}", ConsoleColor.Yellow, interval:2);
                                    diffDic.Add(row, new List<string>() { key, excelValue, value });
                                    //worksheet.Cells[$"{useColumns}{row}"].Value = value;
                                    var rowFill = worksheet.Cells[row, 1, row, Columns].Style.Fill;
                                    rowFill.PatternType = ExcelFillStyle.Solid;
                                    rowFill.BackgroundColor.SetColor(ExcelIndexedColor.Indexed5);
                                }
                            }
                            else
                            {
                                Rows++;
                                worksheet.Cells[$"A{Rows}"].Value = key;
                                worksheet.Cells[$"{useColumns}{Rows}"].Value = value;
                                var rowFill = worksheet.Cells[Rows, 1, Rows, Columns].Style.Fill;
                                rowFill.PatternType = ExcelFillStyle.Solid;
                                rowFill.BackgroundColor.SetColor(ExcelIndexedColor.Indexed17);
                            }
                        }
                    }
                }

                if (diffDic.Count > 0)
                {
                    Utils.WriteColorFun($"检测到有一些Key的翻译，Excel与js文件有差异(包括标点/特殊字符)，检查是否需要更改，然后再重新导出", ConsoleColor.Yellow);
                    foreach (var diff in diffDic)
                    {
                        var value = diff.Value;
                        Utils.WriteColorFun($"Line：{diff.Key}\nKey: {value[0]}\n配置Excel：{value[1]}\n前端js：{value[2]}", ConsoleColor.Yellow);
                    }
                }

                package.Save();
                Utils.WriteColorFun($"导出成功：{ePath}");
                Utils.WriteColorFun($"检查 {Path.GetFileName(ePath)}，黄色行是有差异的翻译（新增Excel请忽略），绿色行是新增的翻译");
            }
            Utils.WaitFun();
        }

        public static string GetExcelPath()
        {
            var key = Utils.GetReadKey(content: $"按A键选择Excel配置表，如无配置则按 Spacebar 继续，稍后会在同目录下自动生成Excel");
            if (key == ConsoleKey.A)
            {
                string ePath = Utils.SelectFile(Utils.excelFilter, Utils.excelTitle);
                return ePath;
            }
            else if (key == ConsoleKey.Spacebar)
            {
                return "";
            }
            else
            {
                return GetExcelPath();
            }
        }
        public static string GetJsPath()
        {
            Utils.ReadKeyFun(ConsoleKey.Spacebar, "选择需要导出的.js"); //开始程序
            string jPath = Utils.SelectFile(Utils.jsFilter, Utils.jsTitle);
            return jPath;
        }
    }
    class Utils
    {
        public static string excelFilter = "表格文件|*.xlsx|All files (*.*)|*.*";
        public static string excelTitle = "选择.xlsx";
        public static string jsFilter = "JavaScript文件|*.js|All files (*.*)|*.*";
        public static string jsTitle = "选择.js";
        // 匹配 window.i18n.languages['English'] 句式
        public static string patternLanguage = @"window\.i18n\.languages\['([^']+)'\]";
        // 匹配 "xxxxx":"xxxxx"  这样的键值对
        public static string patternKeyValue = @"^""([^""]+)""\s*:\s*""([^""]*)""(?:,)?"; // @"^""([^""]+)""\s*:\s*""([^""]+)""(?:,)?";   // @"^""([^""]+)""\s*:\s*""([^""]+)""$";
        public static string useChar = "++++";

        public static string GetProjectPath()
        {
            //判断是 VisualStudio 启动还是直接双击 exe 启动
            bool isVSStartUp = Environment.GetEnvironmentVariable("VisualStudioVersion") != null;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            return isVSStartUp ? Path.GetFullPath(Path.Combine(path, @"..\..")) : path;
        }

        public static ConsoleKey GetReadKey(bool intercept = true, string content = "按任意键继续")
        {
            if (!string.IsNullOrEmpty(content)) { WriteColorFun(content); }
            return Console.ReadKey(intercept).Key;
        }
        public static bool ReadKeyFun(ConsoleKey key = default(ConsoleKey), string tipContent = "继续", bool isLoop = true)
        {
            string keyContent = $"{(key == default(ConsoleKey) ? "任意" : key.ToString())}";
            var inputKey = GetReadKey(content: $"按 {keyContent} 键{tipContent}");
            if (key == default(ConsoleKey))
            {
                return true;
            }
            else
            {
                if (inputKey == key)
                {
                    return true;
                }
                else
                {
                    if (isLoop)
                    {
                        return ReadKeyFun(key, tipContent, isLoop);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public static string SelectFile(string filter, string title)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = filter;
            openFileDialog.Title = title;
            openFileDialog.FilterIndex = 1; // 设置默认选择第一个过滤器
            openFileDialog.RestoreDirectory = true; // 记住上次打开的目录
            openFileDialog.InitialDirectory = GetProjectPath();

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
                GetReadKey(content: "已取消,按任意键继续选择");
                return SelectFile(filter, title);
            }
            else
            {
                return SelectFile(filter, title);
            }
        }
        public static void WriteColorFun(string logContent = "", ConsoleColor color1 = ConsoleColor.White, ConsoleColor color2 = ConsoleColor.White, int interval = 1)
        {
            Console.ForegroundColor = color1;
            Console.WriteLine(logContent);
            for (int i = 0; i < interval; i++) { Console.WriteLine(); }
            Console.ForegroundColor = color2;
        }
        public static void WaitFun(string logContent = "按任意键继续……")
        {
            if (!string.IsNullOrEmpty(logContent)) { WriteColorFun(logContent); }
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    GetReadKey(content: "");
                    break;
                }
            }
        }
    }
}
