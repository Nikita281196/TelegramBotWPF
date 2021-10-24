using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace TelegramBotWPF
{     
    public partial class MainWindow : Window
    {
        
        TelegramMessageClient client;
        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            InitializeComponent();
            client = new TelegramMessageClient(this);

            logList.ItemsSource = client.Messages;
        }

        private void btnMsgSendClick(object sender, RoutedEventArgs e)
        {
            client.SendMessage(txtMsgSend.Text, TargetSend.Text);
            txtMsgSend.Clear();
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void saveHistoryMsg_Click(object sender, RoutedEventArgs e)
        {            
            client.SerialisedMessage();
        }

        private void showFiles_Click(object sender, RoutedEventArgs e)
        {
            client.StartProcess(TargetSend.Text);
            
        }

        public ChartValues<MeasureModel> Values { get; set; }
        private void showChart_Click(object sender, RoutedEventArgs e)
        {
            string Symbol = SymbolForBinance.Text;
            long ChatId = long.Parse(TargetSend.Text);
            CartesianMapper<MeasureModel> mapper = Mappers.Xy<MeasureModel>()
                .X(x => x.Time)
                .Y(x => x.Value);

            Charting.For<MeasureModel>(mapper);
            Values = new ChartValues<MeasureModel>();
            Values.Clear();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            Task taskA = Task.Run(() =>
            {
                
                while (true)
                {
                    
                    float price = client.RequestforBinance(Symbol, ChatId);
                    Thread.Sleep(100);
                    Values.Add(new MeasureModel
                    {
                        Time = sw.ElapsedMilliseconds,

                        Value = price
                    });
                    
                }
                
            });

            DataContext = this;
        }
    }
}
