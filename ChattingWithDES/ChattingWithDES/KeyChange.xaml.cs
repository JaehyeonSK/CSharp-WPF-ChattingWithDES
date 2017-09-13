using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChattingWithDES
{
    /// <summary>
    /// KeyChange.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KeyChange : Window
    {
        public long[] Subkeys { get; set; } = null;

        public KeyChange()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(textKey.Text))
            {
                return;
            }

            // DES 알고리즘 객체를 이용해 16개의 서브키를 생성한다.
            DESAlgorithm.DESAlgorithm des = new DESAlgorithm.DESAlgorithm();
            Subkeys = des.GenerateSubkeys(BitConverter.ToInt64(Encoding.Unicode.GetBytes(textKey.Text), 0));

            this.Hide();
        }
    }
}
