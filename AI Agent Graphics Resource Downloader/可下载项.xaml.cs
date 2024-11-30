namespace AI_Agent_Graphics_Resource_Downloader
{
    /// <summary>
    /// 可下载项.xaml 的交互逻辑
    /// </summary>
    public partial class 可下载项 : System.Windows.Controls.UserControl
    {
        public 可下载项() => InitializeComponent();
        public string Url { get; set; } = string.Empty;
        public Int64 原始大小 { get; set; } = 0;
        public string 名称
        {
            get => 可下载项名称.Text;
            set => 可下载项名称.Text = value;
        }
        public Int64 大小
        {
            set
            {
                原始大小 = value;
                可下载项大小.Text = Utils.ConvertBytesToReadableString(value);
            }
        }
        public string 时间
        {
            set => 可下载项时间.Text = value;
        }
    }
}
