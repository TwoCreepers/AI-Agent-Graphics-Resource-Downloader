using System.IO;

namespace AI_Agent_Graphics_Resource_Downloader
{
    /// <summary>
    /// 下载队列项.xaml 的交互逻辑
    /// </summary>
    public partial class 下载队列项 : System.Windows.Controls.UserControl
    {
        public 下载队列项() => InitializeComponent();
        public string Url { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string 名称
        {
            get => 下载项名称.Text;
            set => 下载项名称.Text = value;
        }
        public double 进度
        {
            get => 下载项进度.Value;
            set
            { // 设置值时确保它在0到100之间
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                下载项进度.Value = value;
            }
        }
        private Int64 m_已下载;
        public Int64 已下载
        {
            get => m_已下载;
            set
            {
                下载项已下载.Text = Utils.ConvertBytesToReadableString(value);
                if (总大小 != 0)
                {
                    下载项进度.Value = (double)value / (double)总大小 * 100.0;
                }
                m_已下载 = value;
            }
        }
        private Int64 m_总大小;
        public Int64 总大小
        {
            get => m_总大小;
            set
            {
                下载项总大小.Text = Utils.ConvertBytesToReadableString(value);
                m_总大小 = value;
            }
        }
    }
}
