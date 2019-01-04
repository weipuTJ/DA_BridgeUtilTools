using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using DotNetARX;

using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using AcadWnd = Autodesk.AutoCAD.Windows;
using Excel = NetOffice.ExcelApi;

namespace DA_Excel2CadTools
{
    /// <summary>
    /// Interaction logic for Excel2CADSettings.xaml
    /// </summary>
    public partial class Excel2CADSettings : System.Windows.Window
    {
        public static E2COptions e2cOptions;//初始化总设置数据类
        public Excel2CADSettings()
        {
            if(e2cOptions == null)
            {
                if (File.Exists("E2COptions.xml"))
                {
                    using (var stream = File.OpenRead("E2COptions.xml"))
                    {
                        var serializer = new XmlSerializer(typeof(E2COptions));
                        e2cOptions = serializer.Deserialize(stream) as E2COptions;
                        e2cOptions.ColumnAuto = true;
                        e2cOptions.ColOptList.Clear();
                    }
                }
                else
                    e2cOptions = new E2COptions();
            }
            InitializeComponent();

            //绑定标题
            textBoxTitle.SetBinding(TextBox.TextProperty, new Binding("Title") { Source = e2cOptions, UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});
            //绑定制表比例
            textBoxScale.SetBinding(TextBox.TextProperty, new Binding("Scale") { Source = e2cOptions, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //绑定插入点
            comboBoxInsertPt.SetBinding(ComboBox.TextProperty, new Binding("InsertPt") { Source = e2cOptions, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //绑定字高
            textBoxHeight.SetBinding(TextBox.TextProperty, new Binding("TextHeight") { Source = e2cOptions, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //绑定文字宽度系数
            textBoxWidthFactor.SetBinding(TextBox.TextProperty, new Binding("TextWidthFactor") { Source = e2cOptions, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //绑定行自动
            checkBoxRowAuto.SetBinding(CheckBox.IsCheckedProperty, new Binding("RowAuto") { Source = e2cOptions,UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //绑定列自动
            checkBoxColumnAuto.SetBinding(CheckBox.IsCheckedProperty, new Binding("ColumnAuto") { Source = e2cOptions, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            //绑定行高
            textBoxHeaderRowHeight.SetBinding(TextBox.TextProperty, new Binding("HeaderRowHeight") { Source = e2cOptions, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            textBoxContentRowHeight.SetBinding(TextBox.TextProperty, new Binding("ContentRowHeight") { Source = e2cOptions, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
        }
        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                #region 线型
                LinetypeTable lt = db.LinetypeTableId.GetObject(OpenMode.ForRead) as LinetypeTable;//获取线型表
                foreach(ObjectId ltrId in lt)//遍历线型表记录
                {
                    LinetypeTableRecord ltr = ltrId.GetObject(OpenMode.ForRead) as LinetypeTableRecord;
                    if(ltr !=null)
                    {
                        this.comboBoxInnerLineStyle.Items.Add(ltr.Name);//将线型名加入内框comboBox
                        this.comboBoxOuterLineStyle.Items.Add(ltr.Name);//将线型名加入外框comboBox
                    }
                }
                //设置默认选择为"ByLayer"
                this.comboBoxInnerLineStyle.SelectedItem = "ByLayer";
                this.comboBoxOuterLineStyle.SelectedItem = "ByLayer";
                #endregion
                #region 字体
                TextStyleTable tst = db.TextStyleTableId.GetObject(OpenMode.ForRead) as TextStyleTable;//获取文字样式表
                foreach(ObjectId tstrId in tst)//遍历文字样式表记录
                {
                    TextStyleTableRecord tstr = tstrId.GetObject(OpenMode.ForRead) as TextStyleTableRecord;
                    if (tstr != null) this.comboBoxTextStyle.Items.Add(tstr.Name);
                }
                //设置默认选择为当前字体
                TextStyleTableRecord curTstr = db.Textstyle.GetObject(OpenMode.ForRead) as TextStyleTableRecord;
                this.comboBoxTextStyle.SelectedItem = curTstr.Name;
                #endregion
                trans.Commit();
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            e2cOptions = null;
            this.Close();
        }

        private void buttonOuterLineColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDlg = new ColorDialog();
            if(colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.buttonOuterLineColor.Background = new SolidColorBrush(Color.FromRgb(
                    colorDlg.Color.Red, colorDlg.Color.Green, colorDlg.Color.Blue));
            }
            e2cOptions.OuterLineColor = colorDlg.Color;
        }

        private void buttonInnerLineColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDlg = new ColorDialog();
            if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.buttonInnerLineColor.Background = new SolidColorBrush(Color.FromRgb(
                    colorDlg.Color.Red, colorDlg.Color.Green, colorDlg.Color.Blue));
            }
            e2cOptions.InnerLineColor = colorDlg.Color;
        }

        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            //1.将设置保存在xml文件中
            using (var stream = File.Open("E2COptions.xml", FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(E2COptions));
                serializer.Serialize(stream, e2cOptions);
            }
            //2.绘制表格
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            //获取当前Excel选择区域
            Excel.Application excelApp = Excel.Application.GetActiveInstance();//获取当前运行的Excel
            Excel.Range selRange = excelApp.Selection as Excel.Range;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //2.1 选择插入点
                PromptPointOptions ptOpt = new PromptPointOptions("选择插入点");
                PromptPointResult ptRes = ed.GetPoint(ptOpt);
                if(ptRes.Status == PromptStatus.OK)
                {
                    Point3d sourcePt = Point3d.Origin;//全局移动起点
                    Point3d targetPt = ptRes.Value;//全局移动终点为所选点
                    double tableHeight = selRange.GetTableHeights().Sum();//表格总高
                    double tableWidth = selRange.GetTableWidths().Sum();//表格总宽
                    //根据插入点设置的不同，确定不同的全局移动起点
                    switch(Excel2CADSettings.e2cOptions.InsertPt)
                    {
                        case "左上": sourcePt = Point3d.Origin; break;
                        case "中上": sourcePt = new Point3d(tableWidth / 2, 0 ,0); break;
                        case "右上": sourcePt = new Point3d(tableWidth, 0, 0); break;
                        case "左中": sourcePt = new Point3d(0, -tableHeight / 2, 0); break;
                        case "正中": sourcePt = new Point3d(tableWidth / 2, -tableHeight / 2, 0); break;
                        case "右中": sourcePt = new Point3d(tableWidth, -tableHeight / 2, 0); break;
                        case "左下": sourcePt = new Point3d(0, -tableHeight, 0); break;
                        case "中下": sourcePt = new Point3d(tableWidth / 2, -tableHeight, 0); break;
                        case "右下": sourcePt = new Point3d(tableWidth, -tableHeight, 0); break;
                    }
                    //将所有表格文字和线段并移动到位
                    ObjectIdCollection entIds = db.DrawTextsAndLines(selRange);
                    foreach (ObjectId id in entIds)
                        id.Move(sourcePt, targetPt);
                }
                trans.Commit();//执行事务处理
            }
            this.Close();
        }

        private void checkBoxColumnAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            Excel.Application excelApp = Excel.Application.GetActiveInstance();//获取当前运行的Excel
            e2cOptions.ColOptList.Clear();//清空原有列设置数据
            if (excelApp != null)
            {
                Excel.Range selRange = excelApp.Selection as Excel.Range;
                if(selRange != null)
                {
                    double[] tableWidths = selRange.GetTableWidths(true);
                    for (int i = 1; i <= selRange.Columns.Count; i++)
                    {
                        ColumnOptions colOpts = new ColumnOptions(
                            colName: selRange.Columns[i].Cells[1].Address,
                            colWidth: tableWidths[i - 1] / e2cOptions.Scale);
                        e2cOptions.ColOptList.Add(colOpts);
                    }
                }
            }
            this.listViewColumnSetting.ItemsSource = e2cOptions.ColOptList;
        }
    }
}
