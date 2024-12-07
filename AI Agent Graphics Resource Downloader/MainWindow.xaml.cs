using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace AI_Agent_Graphics_Resource_Downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Storyboard 上划插入动画;
        private readonly Storyboard Tab淡入动画;

        private bool 打开过程序栏 = false;
        private bool 打开过工具栏 = false;
        private bool 打开过外观栏 = false;
        private bool 打开过角色栏 = false;

        private readonly string Host;

        private NoSleepManager? NoSleepManager;
        public MainWindow()
        {
            InitializeComponent();
            if (Resources["上划插入动画"] is Storyboard 上划插入动画)
            {
                this.上划插入动画 = 上划插入动画;
            }
            else
            {
                throw new Exception("加载动画失败: 上划插入动画");
            }
            if (Resources["Tab淡入动画"] is Storyboard Tab淡入动画)
            {
                this.Tab淡入动画 = Tab淡入动画;
            }
            else
            {
                throw new Exception("加载动画失败: Tab淡入动画");
            }
            while (true)
            {
                var loadding = new 加载();
                loadding.Show();
                Host = 获取url().Result;
                loadding.Close();
                if (Host == string.Empty)
                {
                    var result = System.Windows.Forms.MessageBox.Show(
                        "错误: 无法连接服务器\n是否重试？", "无法连接服务器",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error
                        );
                    if (result.HasFlag(System.Windows.Forms.DialogResult.Yes))
                    {
                        continue;
                    }
                    else
                    {
                        throw new Exception("无法连接服务器");
                    }
                }
                break;
            }
            async Task 下载文件循环()
            {
                using HttpClient client = new();
                while (true)
                {
                    // Run一个新任务，避免阻塞UI线程，因为整个循环都是在UI线程运行
                    var 下载队列项 = await Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (下载队列项队列.TryPeek(out var result))
                            {
                                return result;
                            }
                            await Task.Delay(1000);
                        }
                    });
                    多线程下载文件 多线程下载文件 = new(client, 下载队列项);
                    多线程下载文件.Init();

                    bool success = false;
                    while (!success)
                    {
                        try
                        {
                            await 多线程下载文件.DownloadFileAsync();
                            success = true;
                        }
                        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is IOException)
                        {
                            var shouldRetry = Dispatcher.Invoke(() => 自动重试.IsChecked ?? false);
                            if (shouldRetry)
                            {
                                continue;
                            }
                            else
                            {
                                var result = System.Windows.Forms.MessageBox.Show($"下载时发生IO或任务异常: {ex.Message}\n是否重试?", "下载时发生IO或任务异常!", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                                if (result.HasFlag(System.Windows.Forms.DialogResult.Yes))
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        catch (Exception ex) when (ex is UnauthorizedAccessException)
                        {
                            var result = System.Windows.Forms.MessageBox.Show($"下载时发生权限异常: {ex.Message}\n是否重试?", "下载时发生权限异常!", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                            if (result.HasFlag(System.Windows.Forms.DialogResult.Yes))
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.Message, "下载时发生未知异常!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                    }
                    RemoveWith下载队列项队列(); // 移除和播放动画
                }
            }
            _ = 下载文件循环();

        }
        public void AddFrameworkElement(FrameworkElement containingObject, System.Windows.Controls.StackPanel stackPanel)
        {
            containingObject.RenderTransform = new TranslateTransform(0.0, mainWindow.Height);
            stackPanel.Children.Add(containingObject);
            上划插入动画.Begin(containingObject);
        }
        public void RemoveWithFrameworkElement(FrameworkElement containingObject, System.Windows.Controls.StackPanel stackPanel)
        {
            stackPanel.BeginInit();
            stackPanel.Children.Remove(containingObject);
            foreach (var item in stackPanel.Children)
            {
                if (item is 下载队列项 下载队列项)
                    下载队列项.RenderTransform = new TranslateTransform(0.0, 40.0);
            }
            stackPanel.EndInit();
            foreach (var item in stackPanel.Children)
            {
                if (item is 下载队列项 下载队列项)
                    上划插入动画.Begin(下载队列项);
            }
        }
        private void 设置浏览按钮触发器(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new();
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result.HasFlag(System.Windows.Forms.DialogResult.OK) && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                保存路径.Text = folderBrowserDialog.SelectedPath;
            }
        }
        private void 设置防止系统睡眠开启触发器(object sender, RoutedEventArgs e)
        {
            NoSleepManager = new();
        }
        private void 设置防止系统睡眠关闭触发器(object sender, RoutedEventArgs e)
        {
            NoSleepManager?.Dispose();
        }
        private void 可下载项按钮触发器(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button)
            {
                if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(button)))) is 可下载项 可下载项)
                {
                    AddWith下载队列项队列(new 下载队列项()
                    {
                        名称 = 可下载项.名称,
                        Url = 可下载项.Url,
                        总大小 = 可下载项.原始大小,
                        已下载 = 0,
                        FilePath = Path.Combine(保存路径.Text, 可下载项.名称)
                    });
                }

            }
        }
        private void Tab控件点击触发器(object sender, SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TabControl tabControl)
            {
                if (tabControl.SelectedItem is TabItem tabItem)
                {
                    Tab淡入动画.Begin(tabItem);
                    switch (tabItem.Name)
                    {
                        case "程序":
                            if (!打开过程序栏)
                            {
                                _ = 更新程序栏();
                            }
                            打开过程序栏 = true;
                            break;
                        case "工具":
                            if (!打开过工具栏)
                            {
                                _ = 更新工具栏();
                            }
                            打开过工具栏 = true;
                            break;
                        case "外观":
                            if (!打开过外观栏)
                            {
                                _ = 更新外观栏();
                            }
                            打开过外观栏 = true;
                            break;
                        case "角色":
                            if (!打开过角色栏)
                            {
                                _ = 更新角色栏();
                            }
                            打开过角色栏 = true;
                            break;
                    }

                }
            }
        }
        private void Tab控件双击触发器(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TabControl tabControl)
            {
                if (tabControl.SelectedItem is TabItem tabItem)
                {
                    switch (tabItem.Name)
                    {
                        case "程序":
                            _ = 更新程序栏();
                            break;
                        case "工具":
                            _ = 更新工具栏();
                            break;
                        case "外观":
                            _ = 更新外观栏();
                            break;
                        case "角色":
                            _ = 更新角色栏();
                            break;

                    }
                }
            }
        }

        private static async Task<string> 获取url()
        {
            List<string> urls =
            [
            "http://www.taycraft.cn:18085",
            "http://154.40.59.71:18085"
            ];
            using HttpClient client = new();
            // 存储任务的列表
            List<Task<HttpResponseMessage>> tasks = [];
            try
            {
                // 对每个URL发起请求
                foreach (var url in urls)
                {
                    tasks.Add(client.GetAsync(url));
                }

                // 等待任意一个任务完成
                HttpResponseMessage fastestResponse = await Task.WhenAny(tasks).Result;
                tasks.ForEach(task =>
                {
                    if (!task.IsCompleted)
                    {
                        task.ContinueWith(t => t.Dispose(), TaskContinuationOptions.OnlyOnRanToCompletion);
                    }
                });
                return fastestResponse.RequestMessage?.RequestUri?.ToString() ?? string.Empty;
            }
            catch (AggregateException ae)
            {
                if (ae.InnerExceptions.Count == tasks.Count)
                {
                    return string.Empty;
                }
            }
            catch (HttpRequestException)
            {

            }
            return string.Empty;
        }


        private async Task 更新程序栏()
        {
            StackPanel stackPanel = updatafiles;
            stackPanel.Children.Clear();
            var task = 更新XXX栏(stackPanel, "files.txt", ["AI桌宠.exe"]);
            stackPanel.IsEnabled = false;
            var 提示 = new TextBlock()
            {
                Text = "正在获取信息..."
            };
            AddFrameworkElement(提示, stackPanel);
            await task;
            stackPanel.IsEnabled = true;
            RemoveWithFrameworkElement(提示, stackPanel);
        }
        private async Task 更新工具栏()
        {
            StackPanel stackPanel = toolsfiles;
            stackPanel.Children.Clear();
            var task = 更新XXX栏(stackPanel, "toolsfiles.txt");
            stackPanel.IsEnabled = false;
            var 提示 = new TextBlock()
            {
                Text = "正在获取信息..."
            };
            AddFrameworkElement(提示, stackPanel);
            await task;
            stackPanel.IsEnabled = true;
            RemoveWithFrameworkElement(提示, stackPanel);
        }
        private async Task 更新外观栏()
        {
            StackPanel stackPanel = skinfiles;
            stackPanel.Children.Clear();
            var task = 更新XXX栏(stackPanel, "skinfiles.txt");
            stackPanel.IsEnabled = false;
            var 提示 = new TextBlock()
            {
                Text = "正在获取信息..."
            };
            AddFrameworkElement(提示, stackPanel);
            await task;
            stackPanel.IsEnabled = true;
            RemoveWithFrameworkElement(提示, stackPanel);
        }
        private async Task 更新角色栏()
        {
            StackPanel stackPanel = rolefiles;
            stackPanel.Children.Clear();
            var task = 更新XXX栏(stackPanel, "rolefile.txt");
            stackPanel.IsEnabled = false;
            var 提示 = new TextBlock()
            {
                Text = "正在获取信息..."
            };
            AddFrameworkElement(提示, stackPanel);
            await task;
            stackPanel.IsEnabled = true;
            RemoveWithFrameworkElement(提示, stackPanel);
        }
        private async Task 更新XXX栏(StackPanel stackPanel, string 清单文件, string[]? 额外清单 = null)
        {
            try
            {
                using var client = new HttpClient();

                var 清单内容 = await client.GetStringAsync($"{Host}files/{清单文件}");
                var 清单 = 清单内容.Split("\r\n").ToList();
                if (额外清单 != null)
                {
                    foreach (var item in 额外清单)
                    {
                        清单.Add(item);
                    }
                }
                List<Task<HttpResponseMessage>> 获取信息Task = [];
                foreach (var item in 清单)
                {
                    获取信息Task.Add(client.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"{Host}files/{item}")));
                }
                foreach (var (First, Second) in 获取信息Task.Zip(清单))
                {
                    var 信息 = await First;
                    Int64? 大小 = null;
                    string? 时间 = null;
                    if (信息.Content.Headers.TryGetValues("Content-Length", out var contentLengthValues))
                    {
                        大小 = long.Parse(contentLengthValues.First());
                    }
                    if (信息.Headers.TryGetValues("Date", out var lastModifiedValues))
                    {
                        时间 = lastModifiedValues.First();
                    }
                    if (大小 == null || 时间 == null)
                        break;
                    可下载项 可下载项;
                    try
                    {
                        可下载项 = new 可下载项()
                        {
                            名称 = Second,
                            Url = $"{Host}files/{Second}",
                            大小 = 大小.Value,
                            时间 = DateTime.ParseExact(时间, "R", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),

                        };
                    }
                    catch (FormatException)
                    {
                        可下载项 = new 可下载项()
                        {
                            名称 = Second,
                            Url = $"{Host}files/{Second}",
                            大小 = 大小.Value,
                            时间 = 时间,

                        };
                    }
                    可下载项.可下载项按钮.Click += 可下载项按钮触发器;
                    AddFrameworkElement(可下载项, stackPanel);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    ex.Message, "获取信息时错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // 下载队列项队列管理 (为什么不单独写一个类？因为UI会玄学不更新)
        private readonly ConcurrentQueue<下载队列项> 下载队列项队列 = [];
        private void AddWith下载队列项队列(下载队列项 下载队列项)
        {
            AddFrameworkElement(下载队列项, this.下载项队列);
            下载队列项队列.Enqueue(下载队列项);
            this.下载列表长度.Text = 下载队列项队列.Count.ToString();
        }
        private void RemoveWith下载队列项队列()
        {
            if (!下载队列项队列.TryDequeue(out 下载队列项? 目标下载项) && 目标下载项 == null)
                return;
            RemoveWithFrameworkElement(目标下载项, 下载项队列);
            this.下载列表长度.Text = 下载队列项队列.Count.ToString();
        }

    }
    public class 为0隐藏 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string intValue && intValue == "0")
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Utils
    {
        public static string ConvertBytesToReadableString(Int64 bytes)
        {
            string[] sizes = ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB"];
            double formattedSize = bytes;
            int sizeIndex = 0;

            while (formattedSize >= 1024 && sizeIndex < sizes.Length - 1)
            {
                formattedSize /= 1024;
                sizeIndex++;
            }

            // 如果转换后的值大于1，则保留两位小数，否则直接显示为整数
            return (formattedSize > 1) ? $"{formattedSize:0.##} {sizes[sizeIndex]}" : $"{formattedSize} {sizes[sizeIndex]}";
        }
    }

    public partial class NoSleepManager : IDisposable
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        private enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        private bool _isDisposed;
        private EXECUTION_STATE _state;

        public NoSleepManager()
        {
            // 初始化状态
            _state = EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED;
            // 防止系统进入睡眠状态
            SetThreadExecutionState(_state);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                // 允许系统进入睡眠状态
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                _isDisposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~NoSleepManager()
        {
            Dispose();
        }
    }
    public class 多线程下载文件(HttpClient client, 下载队列项 下载队列项)
    {
        public int 线程数 { get; set; } = 8;
        public List<下载文件上下文> 上下文列表 = [];

        public void Init()
        {
            Int64 partSize = 下载队列项.总大小 / 线程数;
            for (int i = 0; i < 线程数; i++)
            {
                下载文件上下文 上下文 = new(
                    i * partSize,
                    i + 1 != 线程数 ? (i + 1) * partSize - 1 : 下载队列项.总大小 - 1,
                    下载队列项.FilePath,
                    下载队列项.Url,
                    client);
                上下文列表.Add(上下文);
            }
        }
        public async Task DownloadFileAsync()
        {
            var task = new Task[线程数];
            for (int i = 0; i < 线程数; i++)
            {
                task[i] = DownloadPartAsync(上下文列表[i]);
            }
            await Task.WhenAll(task);
        }
        async Task DownloadPartAsync(下载文件上下文 下载文件上下文)
        {
            const int bufferSize = 1024 * 16;

            var HTTP请求 = new HttpRequestMessage(HttpMethod.Get, 下载文件上下文.Url);
            HTTP请求.Headers.Range = new(下载文件上下文.Beg, 下载文件上下文.End);
            using var response = await 下载文件上下文.Client.SendAsync(HTTP请求, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            using var fileStream = new FileStream(下载文件上下文.FilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, bufferSize * 2, true);
            fileStream.Seek(下载文件上下文.Beg, SeekOrigin.Begin);
            using var webStream = await response.Content.ReadAsStreamAsync();
            byte[] buffer = new byte[bufferSize];
            int readBytes;
            while ((readBytes = await webStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, readBytes));
                下载文件上下文.Beg += readBytes;
                下载队列项.已下载 += readBytes;
            }
        }
    }
    public class 下载文件上下文(long beg, long end, string filePath, string url, HttpClient client)
    {
        public Int64 Beg { get; set; } = beg;
        public Int64 End { get; set; } = end;
        public string FilePath { get; set; } = filePath;
        public string Url { get; set; } = url;
        public HttpClient Client { get; set; } = client;
    }
}