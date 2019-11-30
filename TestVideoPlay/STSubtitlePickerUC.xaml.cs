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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestVideoPlay
{
    /// <summary>
    /// Interaction logic for STSubtitlePickerUC.xaml
    /// </summary>
    public partial class STSubtitlePickerUC : UserControl
    {
        public STSubtitlePickerUC()
        {
            InitializeComponent();
        }
        public STSubtitlePickerUC(StreamPlayer.STFoundSubtitle subtitle)
        {
            InitializeComponent();
            StreanSubtitle = subtitle;
        }
        public StreamPlayer.STFoundSubtitle StreanSubtitle { get; set; }
    }
}
