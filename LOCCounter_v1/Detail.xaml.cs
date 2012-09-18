using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AmCharts.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace LOCCounter_v1
{    
    public partial class Detail : Window
    {
        ObservableCollection<TestChartDataItem> ChartData = new ObservableCollection<TestChartDataItem>();
            
        public Detail(Dictionary<string,int> CodeBreakup)
        {
            InitializeComponent();
            AddData(CodeBreakup);
            InitializeChart();
        }

        private void AddData(Dictionary<string, int> CodeBreakup)
        {
            foreach (KeyValuePair<string,int> kvp in CodeBreakup)
            {
                ChartData.Add(new TestChartDataItem() { Title = kvp.Key, Value = kvp.Value });
            }            
        }

        private void InitializeChart()
        {
            Binding slicesBinding = new Binding();
            slicesBinding.Source = ChartData;
            pieChart1.SetBinding(PieChart.SlicesSourceProperty, slicesBinding);

            pieChart1.ValueMemberPath = "Value";
            pieChart1.TitleMemberPath = "Title";
        }        
    }


    public class TestChartDataItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title { get; set; }
        private double _value;
        public double Value
        {
            get { return this._value; }
            set
            {
                this._value = value;
                // notify subscribers that Value property has changed
                if (this.PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
        }
    }
}
