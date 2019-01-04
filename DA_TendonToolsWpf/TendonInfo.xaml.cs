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
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using DotNetARX;
using System.Collections.ObjectModel;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using AcadPolyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

namespace DA_TendonToolsWpf
{
    /// <summary>
    /// Interaction logic for TendonInfo.xaml
    /// </summary>
    public partial class TendonInfo : Window
    {
        internal ObservableCollection<Tendon> tdsInTbl = new ObservableCollection<Tendon>();//存储进入表中的钢束Tendon列表
        internal List<ObjectId> idsInTbl = new List<ObjectId>();//存储进入表中的多段线ObjectId列表
        internal Dictionary<string, ObjectId> tdIdsInTable = new Dictionary<string, ObjectId>();//存储表中钢束的键值和ObjectId，键值存储于最后一个隐藏列中
        /// <summary>
        /// 窗体加载
        /// </summary>
        public TendonInfo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            /*调试用，删除错误的DA_Tendons字典
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
                dicts.Remove("DA_Tendons");
                trans.Commit();//执行事务处理
            }*/
            SyncData.SyncTdGenParasToDwg(db);
            InitializeComponent();
        }
        /// <summary>
        /// 选择钢束按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTendonSel_Click(object sender, RoutedEventArgs e)
        {
            buttonTendonSel.Content = "添加钢束";
            Hide();
            //启动CAD相关对象
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            bool isTdInTbl = false;//初始化判断所选钢束是否已在表中的布尔值            
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            using (DocumentLock loc = doc.LockDocument()) 
            {
                SyncData.SyncTdGenParasToDlg(this);//将tdGenParas对象与对话框数据同步
                db.SyncDwgToTdGenParas();//在将图形数据库数据与tdGenParas对象同步
                #region 1.选择钢束
                List<AcadPolyline> tdLines = new List<AcadPolyline>();//初始化存储钢束线的List
                for (;;)//无限循环
                {
                    tdLines = new List<AcadPolyline>();//清空tds
                    PromptSelectionOptions tdsOpt = new PromptSelectionOptions();
                    tdsOpt.MessageForAdding = "\n选择钢束线，需为无折角的多段线";
                    PromptSelectionResult tdsRes = ed.GetSelection(tdsOpt);
                    if (tdsRes.Status == PromptStatus.Cancel)
                    {
                        this.Show();//重新显示对话框
                        return;
                    }
                    bool isPolyline = true;//设置是否选择集中均为多段线的布尔参数
                    if (tdsRes.Status == PromptStatus.OK)
                    {
                        SelectionSet sSet = tdsRes.Value;
                        foreach (ObjectId tdId in sSet.GetObjectIds())
                        {
                            AcadPolyline tdLine = tdId.GetObject(OpenMode.ForRead) as AcadPolyline;//获取钢束线
                            if (tdLine == null)//选择集中有非多段线
                            {
                                AcadApp.ShowAlertDialog("选择集中含有非多段线，重新选择");
                                isPolyline = false;
                                break;//退出循环
                            }
                            tdLines.Add(tdLine);
                        }
                        if (isPolyline == false) continue;//如果存在非多段线，则重新提示选择
                        break;//结束选择
                    }
                }
                #endregion
                #region 2.扩展字典的读取或添加，并将信息列入表格
                for (int i = 0; i < tdLines.Count; i++)
                {
                    if (idsInTbl.Contains(tdLines[i].ObjectId))
                    {
                        isTdInTbl = true;
                        break;//钢束已在表中，不继续添加，退出循环
                    }
                    idsInTbl.Add(tdLines[i].ObjectId);//将新的钢束加入列表中便于后续判断      
                    string tdKey = "TendonHdl_" + tdLines[i].Handle.ToString();//
                    tdIdsInTable.Add(tdKey, tdLines[i].ObjectId);//加入表中钢束字典列表便于后续操作             
                }
                SyncData.SyncTdsToDwg(ref tdsInTbl,idsInTbl);                                
                #endregion
                trans.Commit();//执行事务处理
            }           
            dataGridTdInfo.ItemsSource = tdsInTbl;
            if (isTdInTbl) MessageBox.Show("选择集中有表中已有钢束，已自动剔除！");
            Show();
        }
        /// <summary>
        /// 左侧张拉CheckBox点击事件
        /// </summary>
        /// <param name="sender">左侧张拉对应CheckBox</param>
        /// <param name="e">左侧张拉对应CheckBox的事件参数</param>
        private void checkBoxLeftDraw_Click(object sender, RoutedEventArgs e)
        {
            SyncData.SyncTdGenParasToDlg(this);//将tdGenParas对象与对话框数据同步
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = db.GetEditor();
            int iRow = dataGridTdInfo.SelectedIndex;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //获得当前行               
                ObjectId tdId = tdIdsInTable[(dataGridTdInfo.Columns[10].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text];//获取本行钢束线的ObjectId
                AcadPolyline td = tdId.GetObject(OpenMode.ForRead) as AcadPolyline;//获取本行钢束线
                if ((sender as CheckBox).IsChecked == false)//如果左侧为不张拉状态，右侧必须张拉
                {
                    (dataGridTdInfo.GetControl(iRow,5, "checkBoxRightDraw") as CheckBox).IsChecked = true;//右侧则必须为张拉状态
                    //左侧引伸量
                    (dataGridTdInfo.Columns[6].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text = "0";
                    //右侧引伸量
                    (dataGridTdInfo.Columns[7].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text =
                            td.SingleDrawAmount(
                                TendonGeneralParameters.CtrlStress,
                                TendonGeneralParameters.Kii,
                                TendonGeneralParameters.Miu, 
                                1,
                                TendonGeneralParameters.Ep).ToString("F0");
                }
                else if ((sender as CheckBox).IsChecked == true)//左侧目前为张拉状态
                {
                    //左侧引伸量
                    (dataGridTdInfo.Columns[6].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text
                        = td.BothDrawAmount(
                                TendonGeneralParameters.CtrlStress,
                                TendonGeneralParameters.Kii,
                                TendonGeneralParameters.Miu,
                                TendonGeneralParameters.Ep)[0].ToString("F0");
                    //右侧引伸量
                    (dataGridTdInfo.Columns[7].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text
                        = td.BothDrawAmount(
                                TendonGeneralParameters.CtrlStress,
                                TendonGeneralParameters.Kii,
                                TendonGeneralParameters.Miu,
                                TendonGeneralParameters.Ep)[1].ToString("F0");
                }
                trans.Commit();
            }            
        }
        /// <summary>
        /// 右侧张拉CheckBox点击事件
        /// </summary>
        /// <param name="sender">右侧张拉对应CheckBox</param>
        /// <param name="e">右侧张拉对应CheckBox的事件参数</param>
        private void checkBoxRightDraw_Click(object sender, RoutedEventArgs e)
        {
            SyncData.SyncTdGenParasToDlg(this);//将tdGenParas对象与对话框数据同步
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = db.GetEditor();
            int iRow = dataGridTdInfo.SelectedIndex;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //获得当前行               
                ObjectId tdId = tdIdsInTable[(dataGridTdInfo.Columns[10].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text];//获取本行钢束线的ObjectId
                AcadPolyline td = tdId.GetObject(OpenMode.ForRead) as AcadPolyline;//获取本行钢束线
                if ((sender as CheckBox).IsChecked == false)//如果右侧为不张拉状态，左侧必须张拉
                {
                    (dataGridTdInfo.GetControl(iRow, 4, "checkBoxLeftDraw") as CheckBox).IsChecked = true;//左侧则必须为张拉状态
                    //左侧引伸量
                    (dataGridTdInfo.Columns[6].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text =
                            td.SingleDrawAmount(
                                TendonGeneralParameters.CtrlStress,
                                TendonGeneralParameters.Kii,
                                TendonGeneralParameters.Miu,
                                1,
                                TendonGeneralParameters.Ep).ToString("F0");
                    //右侧引伸量
                    (dataGridTdInfo.Columns[7].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text = "0";
                }
                else if ((sender as CheckBox).IsChecked == true)//右侧目前为张拉状态
                {
                    //左侧引伸量
                    (dataGridTdInfo.Columns[6].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text
                        = td.BothDrawAmount(
                                TendonGeneralParameters.CtrlStress,
                                TendonGeneralParameters.Kii,
                                TendonGeneralParameters.Miu,
                                TendonGeneralParameters.Ep)[0].ToString("F0");
                    //右侧引伸量
                    (dataGridTdInfo.Columns[7].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text
                        = td.BothDrawAmount(
                                TendonGeneralParameters.CtrlStress,
                                TendonGeneralParameters.Kii,
                                TendonGeneralParameters.Miu,
                                TendonGeneralParameters.Ep)[1].ToString("F0");
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 钢束规格变化时的事件，主要用于修改默认的波纹管直径
        /// </summary>
        /// <param name="sender">钢束规格ComboBox</param>
        /// <param name="e">钢束规格ComboBox的事件参数</param>
        private void comboBoxTdStyles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBoxTdStyle = sender as ComboBox;
            string tdStyle = comboBoxTdStyle.SelectedValue as string;
            int iRow = dataGridTdInfo.SelectedIndex;
            if (iRow != -1)
            {
                (dataGridTdInfo.Columns[3].GetCellContent(dataGridTdInfo.Items[iRow]) as TextBlock).Text
                = GetDefaultPipeDia(tdStyle).ToString("F0");
            }
        }
        /// <summary>
        /// 获得钢束的钢绞线数
        /// </summary>
        /// <param name="tdStyle">钢束规格</param>
        /// <returns>钢绞线数</returns>
        private int GetStrandNum(string tdStyle)
        {
            int strandNum = 0;
            if (int.TryParse(tdStyle.Remove(0, 4), out strandNum)) return strandNum;
            else return 0;
        }
        /// <summary>
        /// 获取钢束规格为tdStyle时的默认波纹管直径
        /// </summary>
        /// <param name="tdStyle">钢束规格</param>
        /// <returns>默认波纹管直径</returns>
        private double GetDefaultPipeDia(string tdStyle)
        {
            int strandNum = GetStrandNum(tdStyle);
            if (strandNum == 2)
                return 45.0;
            else if (strandNum == 3)
                return 50.0;
            else if (strandNum >= 4 && strandNum <= 5)
                return 55.0;
            else if (strandNum >= 6 && strandNum <= 7)
                return 70.0;
            else if (strandNum >= 8 && strandNum <= 9)
                return 80.0;
            else if (strandNum >= 10 && strandNum <= 17)
                return 90.0;
            else if (strandNum >= 18 && strandNum <= 19)
                return 100.0;
            else if (strandNum >= 20 && strandNum <= 27)
                return 120.0;
            else if (strandNum >= 28 && strandNum <= 31)
                return 130.0;
            else if (strandNum >= 32 && strandNum <= 37)
                return 140.0;
            else if (strandNum >= 33 && strandNum <= 55)
                return 160.0;
            else//其余数据异常情况
                return 0;
        }
        /// <summary>
        /// 取消并退出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// 更新图形信息按钮事件，将点击按钮是对话框中的数据更新至图形数据库中。
        /// 先更新tdsInTbl、idsInTbl、tdIdsInTable以及TendonGeneralParameters，
        /// 然后再将新的数据更新至图形数据库中。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConfirm_Click(object sender, RoutedEventArgs e)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            //1.将TendonGeneralParameters、tdsInTbl、idsInTbl、tdIdsInTable等
            //内置参数与对话框中的数据同步
            SyncData.SyncTdGenParasToDlg(this);//将tdGenParas对象与对话框数据同步
            SyncData.SyncTdsToDlg(ref tdsInTbl, this);//将tdsInTbl集合与对话框数据同步
            db.SyncDwgToTdGenParas();//图形数据库总体参数更新
            db.SyncDwgToTds(tdsInTbl);//图形数据库钢束信息更新
            MessageBox.Show("数据已更新！");
        }
        /// <summary>
        /// 删除表格行时更新tdIdsInTbl和idsInTbl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridTdInfo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //如果选中数据行且按下删除键
            if (dataGridTdInfo.SelectedIndex != -1 && e.Key == Key.Delete)
            {
                foreach(var tdItem in  dataGridTdInfo.SelectedItems)
                {
                    //获取选择行中键值
                    string tdKey = (dataGridTdInfo.Columns[10].GetCellContent(tdItem) as TextBlock).Text;
                    foreach(Tendon td in tdsInTbl)
                    {
                        if(td.TdKey == tdKey)
                        {
                            tdsInTbl.Remove(td);
                            break;
                        }
                    }
                    if(tdIdsInTable.ContainsKey(tdKey))
                    {
                        idsInTbl.Remove(tdIdsInTable[tdKey]);
                        tdIdsInTable.Remove(tdKey);  
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonExportTbl_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridTdInfo.Items.Count == 0)
            {
                MessageBox.Show("表中没有数据！");
                return;
            }
            this.Hide();//对话框消失
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            //1.提示用户输入插入点
            PromptPointResult ptRes = ed.GetPoint("\n选择表格插入点");
            if(ptRes.Status!= PromptStatus.OK)//如果选择不正确
            {
                this.Show();//回到对话框
                return;
            }
            //2.先对钢束信息进行更新，类似于点击了更新并退出按钮，但显示更新成功对话框
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            using(DocumentLock loc = doc.LockDocument())
            {
                //1.更新图形数据库总体信息
                SyncData.SyncTdGenParasToDlg(this);
                db.SyncDwgToTdGenParas();
                //2.更新钢束Xrecord信息
                SyncData.SyncTdsToDlg(ref tdsInTbl, this);
                db.SyncDwgToTds(tdsInTbl);
                trans.Commit();//执行事务处理
            }
            //3.输出表格
            using (Transaction trans1 = db.TransactionManager.StartTransaction())//开始事务处理
            using (DocumentLock loc = doc.LockDocument())
            {
                //3.1 将表格内容读入列表中便于操作
                List<string> tdNames = new List<string>();
                List<string> tdStyles = new List<string>();
                List<int> tdNums = new List<int>();
                List<double> pipeDias = new List<double>();
                List<int> drawTypes = new List<int>();
                List<double> leftDrawAmounts = new List<double>();
                List<double> rightDrawAmounts = new List<double>();
                List<double> clearLens = new List<double>();
                List<double> totalLens = new List<double>();
                for (int i = 0; i < dataGridTdInfo.Items.Count; i++)
                {
                    tdNames.Add((dataGridTdInfo.Columns[0].GetCellContent(dataGridTdInfo.Items[i]) as TextBlock).Text);
                    tdStyles.Add((dataGridTdInfo.GetControl(i, 1, "comboBoxTdStyles") as ComboBox).Text);
                    tdNums.Add(int.Parse((dataGridTdInfo.Columns[2].GetCellContent(dataGridTdInfo.Items[i]) as TextBlock).Text));
                    pipeDias.Add(double.Parse((dataGridTdInfo.Columns[3].GetCellContent(dataGridTdInfo.Items[i]) as TextBlock).Text));
                    drawTypes.Add(
                        ((bool)((dataGridTdInfo.GetControl(i, 4, "checkBoxLeftDraw") as CheckBox).IsChecked) ? -1 : 0)
                        + ((bool)((dataGridTdInfo.GetControl(i, 5, "checkBoxRightDraw") as CheckBox).IsChecked) ? 1 : 0)
                        );
                    leftDrawAmounts.Add(double.Parse((dataGridTdInfo.Columns[6].GetCellContent(dataGridTdInfo.Items[i]) as TextBlock).Text));
                    rightDrawAmounts.Add(double.Parse((dataGridTdInfo.Columns[7].GetCellContent(dataGridTdInfo.Items[i]) as TextBlock).Text));
                    clearLens.Add(double.Parse((dataGridTdInfo.Columns[8].GetCellContent(dataGridTdInfo.Items[i]) as TextBlock).Text));
                    totalLens.Add(double.Parse((dataGridTdInfo.Columns[9].GetCellContent(dataGridTdInfo.Items[i]) as TextBlock).Text));
                }
                //3.2 设置表格规格
                Autodesk.AutoCAD.DatabaseServices.Table tb = new Autodesk.AutoCAD.DatabaseServices.Table();//初始化钢束表格
                tb.SetSize(dataGridTdInfo.Items.Count + 3, 14);//设置表格行数（表格行数、表头2行、合计1行）、列数
                //设置列宽
                tb.SetColumnWidth(12);//全部设为12
                tb.Columns[1].Width = 22;//将钢束规格列设为22
                //设置行高
                tb.SetRowHeight(4);
                tb.Position = ptRes.Value;//插入点
                //根据所使用的波纹管直径种类增加新列
                var pipeDiasDist = pipeDias.Select(c => c).Distinct().OrderBy(c => c).ToList();
                //多一种管道直径增加一列
                if (pipeDiasDist.Count > 1) tb.InsertColumns(9, 12, pipeDiasDist.Count - 1);
                //根据所使用的钢束规格增加新列
                var tdStylesDist = tdStyles.Select(c => c).Distinct().OrderBy(c => int.Parse(c.Remove(0, 4))).ToList();
                //多一种钢束规格增加两列，分别为固定和张拉锚具，表格成形后如果本列锚具合计为零，再进行删除
                if (tdStylesDist.Count > 1) tb.InsertColumns(10 + pipeDiasDist.Count, 12, (tdStylesDist.Count - 1) * 2);
                //设置表格样式
                tb.TableStyle = db.Tablestyle;//表格样式为当前表格样式
                tb.Cells.TextStyleId = db.Textstyle;//表格字体样式为当前样式
                tb.Cells.TextHeight = 3;//表格字高为3
                tb.Cells.Alignment = CellAlignment.MiddleCenter;//表格对其方式为对中
                tb.SetMargin(-1, -1, CellMargins.Left, 0);//左侧边距
                tb.SetMargin(-1, -1, CellMargins.Right, 0);//右侧边距   
                tb.SetMargin(-1, -1, CellMargins.Top, 0);//上侧边距
                tb.SetMargin(-1, -1, CellMargins.Bottom, 0);//下侧边距              
                //3.3 表头合并并填写表头
                //将标题行拆分
                tb.UnmergeCells(tb.Cells[0, 0].GetMergeRange());
                //名称
                tb.Cells[0, 0].TextString = "名称";
                tb.MergeCells(CellRange.Create(tb, 0, 0, 1, 0));
                //规格
                tb.Cells[0, 1].TextString = "规格";
                tb.MergeCells(CellRange.Create(tb, 0, 1, 1, 1));
                //钢束长度
                tb.Cells[0, 2].TextString = "钢束长度\n(mm)";
                tb.MergeCells(CellRange.Create(tb, 0, 2, 1, 2));
                //根数
                tb.Cells[0, 3].TextString = "根数";
                tb.MergeCells(CellRange.Create(tb, 0, 3, 1, 3));
                //钢束总长
                tb.Cells[0, 4].TextString = "钢束总长\n(m)";
                tb.MergeCells(CellRange.Create(tb, 0, 4, 1, 4));
                //总重
                tb.Cells[0, 5].TextString = "总重(kg)";
                tb.MergeCells(CellRange.Create(tb, 0, 5, 1, 5));
                //引伸量
                tb.Cells[0, 6].TextString = "张拉端引伸量(mm)";
                tb.MergeCells(CellRange.Create(tb, 0, 6, 0, 7));
                tb.Cells[1, 6].TextString = "左端";
                tb.Cells[1, 7].TextString = "右端";
                //波纹管长
                tb.Cells[0, 8].TextString = "波纹管长\n(mm)";
                tb.MergeCells(CellRange.Create(tb, 0, 8, 1, 8));
                //管道总长
                tb.Cells[0, 9].TextString = "管道总长(m)";
                if (pipeDiasDist.Count > 1) tb.MergeCells(CellRange.Create(tb, 0, 9, 0, 8 + pipeDiasDist.Count));
                for (int i = 0; i < pipeDiasDist.Count; i++)
                {
                    tb.Cells[1, 9 + i].TextString = "Φ" + pipeDiasDist[i].ToString("F0");
                }
                //锚具
                tb.Cells[0, 9 + pipeDiasDist.Count].TextString = "锚具套数";
                tb.MergeCells(CellRange.Create(tb, 0, 9 + pipeDiasDist.Count, 0, 10 + pipeDiasDist.Count + (tdStylesDist.Count - 1) * 2));
                for (int i = 0; i < tdStylesDist.Count; i++)
                {
                    tb.Cells[1, 9 + pipeDiasDist.Count + 2 * i].TextString = tdStylesDist[i].Remove(0, 1) + "张拉";
                    tb.Cells[1, 9 + pipeDiasDist.Count + 2 * i + 1].TextString = tdStylesDist[i].Remove(0, 1) + "固定";
                }
                //控制应力
                tb.Cells[0, tb.Columns.Count - 2].TextString = "控制应力\n(MPa)";
                tb.MergeCells(CellRange.Create(tb, 0, tb.Columns.Count - 2, 1, tb.Columns.Count - 2));
                //备注
                tb.Cells[0, tb.Columns.Count - 1].TextString = "备注";
                tb.MergeCells(CellRange.Create(tb, 0, tb.Columns.Count - 1, 1, tb.Columns.Count - 1));
                //3.4 填写表格内容
                for (int i = 0; i < dataGridTdInfo.Items.Count; i++)//行迭代
                {
                    tb.Cells[2 + i, 0].TextString = tdNames[i];//名称
                    tb.Cells[2 + i, 1].TextString = tdStyles[i];//规格
                    tb.Cells[2 + i, 2].TextString = totalLens[i].ToString("F0");//钢束长度
                    tb.Cells[2 + i, 3].TextString = tdNums[i].ToString("F0");//钢束根数
                    tb.Cells[2 + i, 4].TextString = (totalLens[i] * tdNums[i] / 1000).ToString("F1");//钢束总长
                    tb.Cells[2 + i, 5].TextString = (int.Parse(tdStyles[i].Remove(0, 4)) * 1.101//总重
                        * (totalLens[i] * tdNums[i] / 1000)).ToString("F1");
                    tb.Cells[2 + i, 6].TextString = leftDrawAmounts[i].ToString("F0");//左侧引伸量
                    tb.Cells[2 + i, 7].TextString = rightDrawAmounts[i].ToString("F0");//右侧引伸量
                    tb.Cells[2 + i, 8].TextString = clearLens[i].ToString("F0");//波纹管长
                    for (int j = 0; j < pipeDiasDist.Count; j++)//管道总长
                    {
                        if (pipeDias[i] == pipeDiasDist[j])//找到对应直径列
                        {
                            tb.Cells[2 + i, 9 + j].TextString = (clearLens[i] * tdNums[i] / 1000).ToString("F1");
                            break;
                        }
                    }
                    for (int j = 0; j < tdStylesDist.Count; j++)//锚具套数
                    {
                        if (tdStyles[i] == tdStylesDist[j])//找到对应规格列
                        {
                            tb.Cells[2 + i, 9 + pipeDiasDist.Count + 2 * j].TextString//张拉套数
                                = (tdNums[i] * (2 - Math.Abs(drawTypes[i]))).ToString("F0");
                            tb.Cells[2 + i, 9 + pipeDiasDist.Count + 2 * j + 1].TextString//锚固套数
                                = (tdNums[i] * Math.Abs(drawTypes[i])).ToString("F0");
                        }
                    }
                    tb.Cells[2 + i, tb.Columns.Count - 2].TextString = TendonGeneralParameters.CtrlStress.ToString("F0");//控制应力
                    tb.Cells[2 + i, tb.Columns.Count - 1].TextString = (drawTypes[i] == 0) ? "两端张拉" : "单端张拉";
                }
                //3.5 填写最后一行合计内容并删除合计为0的锚具列
                tb.Cells[tb.Rows.Count - 1, 0].TextString = "合计";
                //总重合计
                tb.Cells[tb.Rows.Count - 1, 5].TextString = tb.Columns[5]
                    .Where(c => c.Row > 1 && c.Row < tb.Rows.Count - 1)
                    .Sum(c => double.Parse(tb.Cells[c.Row, c.Column].TextString)).ToString("F1");
                //管道总长合计
                for (int j = 0; j < pipeDiasDist.Count; j++)//管道总长
                {
                    tb.Cells[tb.Rows.Count - 1, 9 + j].TextString = tb.Columns[9 + j]
                        .Where(c => c.Row > 1 && c.Row < tb.Rows.Count - 1 && (bool)!tb.Cells[c.Row, c.Column].IsEmpty)
                        .Sum(c => double.Parse(tb.Cells[c.Row, c.Column].TextString)).ToString("F1");
                }
                //锚具套数合计
                for (int j = 0; j < 2 * tdStylesDist.Count; j++)
                {
                    tb.Cells[tb.Rows.Count - 1, 9 + pipeDiasDist.Count + j].TextString = tb.Columns[9 + pipeDiasDist.Count + j]
                        .Where(c => c.Row > 1 && c.Row < tb.Rows.Count - 1 && (bool)!tb.Cells[c.Row, c.Column].IsEmpty)
                        .Sum(c => int.Parse(tb.Cells[c.Row, c.Column].TextString)).ToString("F0");
                }
                //删除锚具套数合计为0的列，减小表格规模
                var voidCols = tb.Rows[tb.Rows.Count - 1]
                    .Where(c => c.Column >= 9 + pipeDiasDist.Count && c.Column <= tb.Columns.Count - 3)
                    .Where(c => int.Parse(tb.Cells[c.Row, c.Column].TextString) == 0)
                    .Select(c => c.Column)
                    .OrderByDescending(c => c);
                foreach (int iCol in voidCols)
                {
                    tb.DeleteColumns(iCol, 1);//依次删除套数为0的列
                }
                //3.6 将表格按照输入比例缩放
                double scale = double.Parse(textBoxScale.Text);
                Matrix3d mt = Matrix3d.Scaling(scale, ptRes.Value);
                tb.TransformBy(mt);
                tb.GenerateLayout();//更新表格
                db.AddToModelSpace(tb);//将表格加入数据库
                trans1.Commit();//执行事务处理
            }
            //4.输出表格并查看后重新返回对话框还是离开程序
            PromptKeywordOptions kwOpt = new PromptKeywordOptions("\n返回对话框还是退出程序[返回对话框(R)/退出程序(任意键)]");
            kwOpt.AllowArbitraryInput = true;
            kwOpt.AllowNone = false;
            PromptResult keRes = ed.GetKeywords(kwOpt);
            if (keRes.Status == PromptStatus.OK && keRes.StringResult == "R")
                this.Show();
            else
                this.Close();
        }
    }
}
