using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MagicOClockGenerator
{
    public partial class Form1 : Form
    {
        private string rootPath = @"F:\MagicOClock2026\DATA\listdata";
        private const string MAIN_LIST_NAME = "listclock.html";
        private const string GITHUB_URL = "https://raw.githubusercontent.com/oclockmagic/DATA/refs/heads/main/listdata/";
        
        public Form1()
        {
            InitializeComponent();
            this.Text = "Magic O'Clock HTML & JSON Generator";
            this.Width = 450; this.Height = 250; this.StartPosition = FormStartPosition.CenterScreen;

            Label lbl = new Label();
            lbl.Text = "Magic O'Clock Generator Pro";
            lbl.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
            lbl.AutoSize = true; lbl.Location = new System.Drawing.Point(80, 40);
            this.Controls.Add(lbl);

            Button btnGenerate = new Button();
            btnGenerate.Text = "BẮT ĐẦU TẠO HTML + JSON";
            btnGenerate.Size = new System.Drawing.Size(250, 60);
            btnGenerate.Location = new System.Drawing.Point(100, 110);
            btnGenerate.BackColor = System.Drawing.Color.FromArgb(79, 172, 254);
            btnGenerate.ForeColor = System.Drawing.Color.White;
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            btnGenerate.Click += BtnGenerate_Click;
            this.Controls.Add(btnGenerate);
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(rootPath)) { MessageBox.Show("Error: " + rootPath); return; }
                var categories = new List<CategoryInfo>();
                var htmlFilesBuilt = new List<string>();
                var subDirs = Directory.GetDirectories(rootPath);

                foreach (var dirPath in subDirs)
                {
                    string dirName = Path.GetFileName(dirPath);
                    if (File.Exists(Path.Combine(rootPath, dirName + ".jpg")) && File.Exists(Path.Combine(rootPath, dirName + ".txt")))
                    {
                        string title = File.ReadAllLines(Path.Combine(rootPath, dirName + ".txt")).FirstOrDefault() ?? dirName;
                        var items = GetClockItems(dirPath);
                        string safeId = dirName.Length > 9 ? dirName.Substring(0, 9) : dirName;
                        string fileName = safeId.ToLower() + ".html";
                        var catInfo = new CategoryInfo { Id = dirName, SafeFileName = fileName, Title = title, Image = dirName + ".jpg", Count = items.Count, Items = items };
                        categories.Add(catInfo);
                        GenerateDetailFile(catInfo);
                        htmlFilesBuilt.Add(fileName);
                    }
                }
                GenerateMainListFile(categories);
                htmlFilesBuilt.Insert(0, MAIN_LIST_NAME);
                GenerateSimpleJson(htmlFilesBuilt);
                MessageBox.Show("Xong! Đã sửa thanh tiến trình và font chữ.");
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void GenerateSimpleJson(List<string> htmlFiles)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{\n  \"data\": [");
            var e = htmlFiles.Select(f => $"    \"{f}\"").ToList();
            sb.Append(string.Join("," + Environment.NewLine, e));
            sb.AppendLine("\n  ]\n}");
            File.WriteAllText(Path.Combine(rootPath, "data.json"), sb.ToString(), Encoding.UTF8);
        }

        private List<ClockItem> GetClockItems(string dirPath)
        {
            var list = new List<ClockItem>();
            foreach (var clk in Directory.GetFiles(dirPath, "*.clk"))
            {
                string name = Path.GetFileName(clk);
                string bmp = Path.GetFileNameWithoutExtension(clk) + ".bmp";
                list.Add(new ClockItem { ClkFile = name, BmpFile = File.Exists(Path.Combine(dirPath, bmp)) ? bmp : "" });
            }
            return list;
        }

        private void GenerateMainListFile(List<CategoryInfo> categories)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetHeader("Magic Store", true));
            foreach (var cat in categories)
            {
                sb.Append($@"
            <a href='{cat.SafeFileName}' class='category_item'>
                <img src='{GITHUB_URL + cat.Image}' alt='{cat.Title}'>
                <div class='category_title'>{cat.Title}</div>
                <div class='category_count'>{cat.Count}</div>
            </a>");
            }
            sb.Append(GetFooter(true));
            File.WriteAllText(Path.Combine(rootPath, MAIN_LIST_NAME), sb.ToString(), Encoding.UTF8);
        }

        private void GenerateDetailFile(CategoryInfo cat)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetHeader("Magic - " + cat.Title, false));
            foreach (var item in cat.Items)
            {
                string githubImg = string.IsNullOrEmpty(item.BmpFile) ? "" : GITHUB_URL + cat.Id + "/" + item.BmpFile;
                string githubClk = GITHUB_URL + cat.Id + "/" + item.ClkFile;
                // Bỏ đuôi file khi hiển thị
                string displayName = Path.GetFileNameWithoutExtension(item.ClkFile);
                sb.Append($@"
                <div class='view_item'>
                    <div class='vi_left'>
                        <img src='{githubImg}' alt='{displayName}'>
                        <div class='file-name'>{displayName}</div>
                    </div>
                    <button class='btn-download' data-url='{githubClk}' data-name='{item.ClkFile}'>Download</button>
                </div>");
            }
            sb.Append(GetFooter(false));
            File.WriteAllText(Path.Combine(rootPath, cat.SafeFileName), sb.ToString(), Encoding.UTF8);
        }

        private string GetHeader(string title, bool isIndex)
        {
            string s = isIndex ? @"
        .category_grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; margin-top: 10px; }
        .category_item { background: rgba(255,255,255,0.08); border: 1px solid rgba(255,255,255,0.1); border-radius: 12px; padding: 10px; text-align: center; text-decoration: none; color: inherit; }
        .category_item img { width: 100%; aspect-ratio: 1; border-radius: 8px; object-fit: cover; }
        .category_title { font-size: 10px; font-weight: 500; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .category_count { font-size: 8px; opacity: 0.6; }" : @"
        .view_wrap { display: flex; flex-wrap: wrap; justify-content: flex-start; gap: 10px; margin-top: 10px; }
        .view_item { background: rgba(255,255,255,0.1); border: 1px solid rgba(255,255,255,0.1); border-radius: 12px; padding: 10px; display: inline-flex; flex-direction: column; align-items: center; width: 95px; }
        .vi_left img { width: 75px; height: 75px; border-radius: 8px; object-fit: cover; }
        .btn-download { background: linear-gradient(135deg, #4facfe, #00f2fe); color: white; border: none; border-radius: 6px; padding: 5px 0; width: 100%; font-size: 10px; font-weight: bold; margin-top: 8px; cursor: pointer; }
        .file-name { font-size: 8px; margin-top: 4px; opacity: 0.7; color: #00f2fe; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 80px; }";

            return $@"<!DOCTYPE html><html><head><title>{title}</title><meta charset='utf-8'><meta name='viewport' content='width=device-width,initial-scale=0.8, user-scalable=no'>
<style>body {{ display: flex; padding: 10px; min-height: 100vh; align-items: center; justify-content: center; background-color: rgb(6,2,29); color: #fff; font-family: 'Segoe UI', sans-serif; margin: 0; }}
.wrapper {{ width: 450px; max-width: 95vw; background: rgba(0,0,0,0.3); padding: 20px; border-radius: 20px; backdrop-filter: blur(15px); border: 1px solid rgba(255,255,255,0.1); }}
h1 {{ text-align: center; font-weight: 300; font-size: 20px; margin: 10px 0; }} hr {{ border: 0; height: 1px; background: linear-gradient(90deg, transparent, rgba(255,255,255, 0.2), transparent); margin: 15px 0; }}
.home-img {{ width: 100%; background: rgba(255,255,255,0.05); height: 40px; color: #FFF; border-radius: 10px; border: 1px solid rgba(255,255,255,0.2); margin-top: 20px; cursor: pointer; }}
.conectt {{ display: block; font-size: 10px; color: rgba(255,255,255,0.4); margin-top: 15px; text-align: right; }}
.overlay {{ display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.7); z-index: 9999; justify-content: center; align-items: center; backdrop-filter: blur(5px); }}
.overlay-content {{ background: rgba(20,20,20,0.8); padding: 30px; border-radius: 15px; text-align: center; width: 260px; }}
.loader {{ border: 3px solid rgba(255,255,255,0.1); border-top: 3px solid #4facfe; border-radius: 50%; width: 35px; height: 35px; animation: spin 1s linear infinite; margin: 0 auto 15px; }}
.progress-container {{ width: 100%; height: 8px; background: rgba(255,255,255,0.1); border-radius: 4px; margin-top: 15px; overflow: hidden; display: none; }}
.progress-bar {{ width: 0%; height: 100%; background: linear-gradient(90deg, #4facfe, #00f2fe); transition: width 0.3s; }}
@keyframes spin {{ 0% {{ transform: rotate(0deg); }} 100% {{ transform: rotate(360deg); }} }} {s}</style></head>
<body><div id='overlay' class='overlay'><div class='overlay-content'><div class='loader'></div><p id='status-text' style='font-size:14px;'>Đang tải...</p><div class='progress-container' id='p-container'><div class='progress-bar' id='p-bar'></div></div><p id='p-text' style='font-size:10px; margin-top:5px; color:#4facfe;'></p></div></div>
<div class='wrapper'><h1>Magic Store</h1><hr><div class='{(isIndex ? "category_grid" : "view_wrap")}' id='items-container'>";
        }

        private string GetFooter(bool isIndex)
        {
            return $@"</div><hr>{(isIndex ? "" : $"<button class='home-img' onclick=\"window.location.href='{MAIN_LIST_NAME}'\">BACK TO LIST</button>")}
<label class='conectt' id='conet'>Status: Connecting...</label></div>
<script>var websocket; function initWebSocket() {{
var host = window.location.host || '192.168.4.1'; websocket = new WebSocket('ws://' + host + '/ws');
websocket.onopen = function() {{ document.getElementById('conet').innerHTML = 'Status: Connected'; }};
websocket.onmessage = function(evt) {{ 
    var data = evt.data;
    if (data === 'SAVEOK') {{
        document.getElementById('status-text').innerText = 'Thành công!';
        document.getElementById('p-container').style.display = 'none';
        setTimeout(() => {{ document.getElementById('overlay').style.display='none'; }}, 1500);
    }} else if (data.startsWith('#PRG@')) {{
        var p = data.substring(5);
        document.getElementById('p-container').style.display = 'block';
        document.getElementById('p-bar').style.width = p + '%';
        document.getElementById('p-text').innerText = p + '%';
    }}
}};
}}
function download(url, name) {{ 
    document.getElementById('overlay').style.display='flex'; 
    document.getElementById('status-text').innerText = 'Đang tải ' + name + '...';
    if (websocket && websocket.readyState === 1) websocket.send('#DLD@' + url); 
}}
document.getElementById('items-container').addEventListener('click', function(e) {{
var t = e.target.closest('.btn-download'); if (t) download(t.getAttribute('data-url'), t.getAttribute('data-name'));
}}); window.onload = initWebSocket;</script></body></html>";
        }
    }
    public class CategoryInfo { public string Id { get; set; } public string SafeFileName { get; set; } public string Title { get; set; } public string Image { get; set; } public int Count { get; set; } public List<ClockItem> Items { get; set; } }
    public class ClockItem { public string ClkFile { get; set; } public string BmpFile { get; set; } }
}
