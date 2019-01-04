using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using DotNetARX;

namespace DA_TendonTools
{
    public partial class TendonInfo : Form
    {
        List<ObjectId> idsInTbl = new List<ObjectId>();//存储进入表中的多段线ObjectId列表
        Dictionary<string,ObjectId> tdIdsInTable = new Dictionary<string, ObjectId>();//存储表中钢束的键值和ObjectId，键值存储于最后一个隐藏列中
        //初始化总体参数
        double kii = 0.0015;
        double miu = 0.16;
        double Ep = 1.95E5;
        double ctrlStress = 1395;
        double workLen = 800;
        public TendonInfo()
        {
            InitializeComponent();
        }
        #region 窗体加载
        private void TendonInfo_Load(object sender, EventArgs e)
        {
            #region 1.获取各总体参数
            //1.1管道偏差系数
            if (!double.TryParse(textBoxKii.Text, out kii))
            {
                MessageBox.Show("管道偏差系数输入有误！");
                this.Visible = true;
                return;
            }
            //1.2摩阻系数
            if (!double.TryParse(textBoxMiu.Text, out miu))
            {
                MessageBox.Show("摩阻系数输入有误！");
                this.Visible = true;
                return;
            }
            //1.3钢束弹模
            double Ep = 1.95e5;
            if (!double.TryParse(textBoxEp.Text, out Ep))
            {
                MessageBox.Show("钢束弹模输入有误！");
                this.Visible = true;
                return;
            }
            //1.4张拉控制应力
            if (!double.TryParse(textBoxCtrlStress.Text, out ctrlStress))
            {
                MessageBox.Show("张拉控制应力输入有误！");
                this.Visible = true;
                return;
            }
            //1.5工作长度
            if (!double.TryParse(textBoxWorkLen.Text, out workLen))
            {
                MessageBox.Show("工作长度输入有误！");
                this.Visible = true;
                return;
            }
            #endregion
        }
        #endregion
        #region 选择/添加钢束按钮
        private void buttonTendonSel_Click(object sender, EventArgs e)
        {
            buttonTendonSel.Text = "添加钢束";
            this.Visible = false;
            //启动CAD相关对象
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            bool isTdInTbl = false;//初始化判断所选钢束是否已在表中的布尔值
            //1.1管道偏差系数
            if (!double.TryParse(textBoxKii.Text, out kii))
            {
                MessageBox.Show("管道偏差系数输入有误！");
                this.Visible = true;
                return;
            }
            //1.2摩阻系数
            if (!double.TryParse(textBoxMiu.Text, out miu))
            {
                MessageBox.Show("摩阻系数输入有误！");
                this.Visible = true;
                return;
            }
            //1.3钢束弹模
            double Ep = 1.95e5;
            if (!double.TryParse(textBoxEp.Text, out Ep))
            {
                MessageBox.Show("钢束弹模输入有误！");
                this.Visible = true;
                return;
            }
            //1.4张拉控制应力
            if (!double.TryParse(textBoxCtrlStress.Text, out ctrlStress))
            {
                MessageBox.Show("张拉控制应力输入有误！");
                this.Visible = true;
                return;
            }
            //1.5工作长度
            if (!double.TryParse(textBoxWorkLen.Text, out workLen))
            {
                MessageBox.Show("工作长度输入有误！");
                this.Visible = true;
                return;
            }
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                #region 1.选择钢束
                List<Polyline> tds = new List<Polyline>();//初始化存储钢束的List
                for (;;)//无限循环
                {
                    tds = new List<Polyline>();//清空tds
                    PromptSelectionOptions tdsOpt = new PromptSelectionOptions();
                    tdsOpt.MessageForAdding = "\n选择钢束线，需为无折角的多段线";
                    PromptSelectionResult tdsRes = ed.GetSelection(tdsOpt);
                    if(tdsRes.Status == PromptStatus.Cancel)
                    {
                        this.Visible = true;//重新显示对话框
                        return;
                    }
                    bool isPolyline = true;//设置是否选择集中均为多段线的布尔参数
                    if (tdsRes.Status == PromptStatus.OK)
                    {
                        SelectionSet sSet = tdsRes.Value;
                        foreach (ObjectId tdId in sSet.GetObjectIds())
                        {
                            Polyline td = tdId.GetObject(OpenMode.ForRead) as Polyline;//获取钢束线
                            if (td == null)//选择集中有非多段线
                            {
                                AcadApp.ShowAlertDialog("选择集中含有非多段线，重新选择");
                                isPolyline = false;
                                break;//退出循环
                            }
                            tds.Add(td);
                        }
                        if (isPolyline == false) continue;//如果存在非多段线，则重新提示选择
                        break;//结束选择
                    }
                }
                #endregion
                #region 2.扩展字典的读取或添加，并将信息列入表格
                for (int i = 0; i < tds.Count; i++)
                {
                    if(idsInTbl.Contains(tds[i].ObjectId))
                    {
                        isTdInTbl = true;
                        break;//钢束已在表中，不继续添加，退出循环
                    }
                    idsInTbl.Add(tds[i].ObjectId);//将新的钢束加入列表中便于后续判断
                    int index = (int)(XrecordManipulate.ReadXRecordToRow(this, tds[i], kii, miu, Ep, ctrlStress,workLen));
                    string tdKey = "TendonHdl_" + tds[i].Handle.ToString();//钢束键值
                    dataGridViewTendons.Rows[index].Cells[10].Value = tdKey;//键值加入最后一列
                    tdIdsInTable.Add(tdKey, tds[i].ObjectId);//加入表中钢束字典列表便于后续操作
                }
                #endregion
                trans.Commit();//执行事务处理                
            }
            this.Visible = true;//重新显示对话框
            if (isTdInTbl) MessageBox.Show("\n选择集中有表中已有钢束，已自动剔除！");
        }
        #endregion
        #region 更新图形信息按钮
        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            Database db = HostApplicationServices.WorkingDatabase;
            if (dataGridViewTendons.Rows.Count == 0)
            {
                db.ReadDlgToNamedDic(this);
                MessageBox.Show("表中没有数据,仅更新总体信息！");
                return;
            }
            else
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
                {
                    //1.更新有名对象字典中的总体参数
                    db.ReadDlgToNamedDic(this);
                    //1.1管道偏差系数
                    if (!double.TryParse(textBoxKii.Text, out kii))
                    {
                        MessageBox.Show("管道偏差系数输入有误！");
                        this.Visible = true;
                        return;
                    }
                    //1.2摩阻系数
                    if (!double.TryParse(textBoxMiu.Text, out miu))
                    {
                        MessageBox.Show("摩阻系数输入有误！");
                        this.Visible = true;
                        return;
                    }
                    //1.3钢束弹模
                    double Ep = 1.95e5;
                    if (!double.TryParse(textBoxEp.Text, out Ep))
                    {
                        MessageBox.Show("钢束弹模输入有误！");
                        this.Visible = true;
                        return;
                    }
                    //1.4张拉控制应力
                    if (!double.TryParse(textBoxCtrlStress.Text, out ctrlStress))
                    {
                        MessageBox.Show("张拉控制应力输入有误！");
                        this.Visible = true;
                        return;
                    }
                    //1.5工作长度
                    if (!double.TryParse(textBoxWorkLen.Text, out workLen))
                    {
                        MessageBox.Show("工作长度输入有误！");
                        this.Visible = true;
                        return;
                    }
                    //2.更新钢束Xrecord信息,更新表中伸长量、总长信息（如果工作长度变化的话）
                    for (int i = 0; i < dataGridViewTendons.Rows.Count; i++)
                    {
                        ObjectId tdId = tdIdsInTable[dataGridViewTendons.Rows[i].Cells[10].Value.ToString()];
                        tdId.ReadRowToXrecord(dataGridViewTendons.Rows[i]);
                        XrecordManipulate.UpdateXRecordToRow(this,i,tdId, kii, miu, Ep, ctrlStress, workLen);
                    }
                    trans.Commit();//执行事务处理
                }
                MessageBox.Show("总体信息和钢束信息均已更新！");
            }
        }
        #endregion
        #region 取消并退出按钮
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
        #endregion
        #region 当钢束规格、张拉方式变化时的事件
        //1.处理钢束规格变化时的事件，自动改变管道直径
        //1.1 为ComboBox控件添加事件
        //经试验，该方法不适用于CheckBox，故钢束张拉方式改变事件采用不同方式
        private void dataGridViewTendons_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            //当前表格为“钢束规格”且非表头
            if(dataGridViewTendons.CurrentCell.ColumnIndex == 1 && dataGridViewTendons.CurrentCell.RowIndex != -1)
            {
                ComboBox comboBoxTendonStyle = e.Control as ComboBox;//获取表格控件
                //添加事件，添加前先去除事件，防止循环调用
                comboBoxTendonStyle.SelectedIndexChanged -= new EventHandler(comboBoxTendonStyle_SelectedIndexChanged);
                comboBoxTendonStyle.SelectedIndexChanged += new EventHandler(comboBoxTendonStyle_SelectedIndexChanged);
            }
        }
        //1.2 定义comboBoxTendonStyle_SelectedIndexChanged处理函数
        //当钢束规格切换时，自动改变管道直径
        private void comboBoxTendonStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(((ComboBox)sender).Text)
            {
                case "Φ15-7":
                case "Φ15-9":
                    dataGridViewTendons.CurrentRow.Cells[3].Value = "80";
                    break;
                case "Φ15-12":
                case "Φ15-15":
                    dataGridViewTendons.CurrentRow.Cells[3].Value = "90";
                    break;
            }
        }
        //2.定义张拉方式改变时的处理函数
        //通过将两列CheckBox设置为ReadOnly=true状态，通过程序实现点击后的变化，并添加对应的事件
        //事件主要包括两方面：一是不允许两侧均不张拉；二是更新伸长量数据
        private void dataGridViewTendons_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            #region 当修改左侧张拉方式
            if (dataGridViewTendons.CurrentCell.ColumnIndex == 4)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    ObjectId tdId = tdIdsInTable[dataGridViewTendons.CurrentRow.Cells[10].Value.ToString()];//获取本行钢束线的ObjectId
                    Polyline td = tdId.GetObject(OpenMode.ForRead) as Polyline;//获取本行钢束线
                    if ((bool)dataGridViewTendons.CurrentRow.Cells[4].Value == true)//如果左侧目前为张拉状态
                    {
                        dataGridViewTendons.CurrentRow.Cells[4].Value = false;//点击后变为不张拉状态
                        dataGridViewTendons.CurrentRow.Cells[5].Value = true;//右侧则必须为张拉状态
                        //左侧引伸量
                        dataGridViewTendons.CurrentRow.Cells[6].Value = "0";
                        //右侧引伸量
                        dataGridViewTendons.CurrentRow.Cells[7].Value =
                                td.SingleDrawAmount(ctrlStress, kii, miu, 1, Ep).ToString("F0");
                    }
                    else if ((bool)dataGridViewTendons.CurrentRow.Cells[4].Value == false)//左侧目前为不张拉状态
                    {
                        dataGridViewTendons.CurrentRow.Cells[4].Value = true;//左侧点击后变为张拉状态
                                                                             //左侧引伸量
                        dataGridViewTendons.CurrentRow.Cells[6].Value
                            = td.BothDrawAmount(ctrlStress, kii, miu, Ep)[0].ToString("F0");
                        //右侧引伸量
                        dataGridViewTendons.CurrentRow.Cells[7].Value
                            = td.BothDrawAmount(ctrlStress, kii, miu, Ep)[1].ToString("F0");
                    }
                    trans.Commit();
                }
            }
            #endregion
            #region 当修改右侧张拉方式
            if (dataGridViewTendons.CurrentCell.ColumnIndex == 5)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    ObjectId tdId = tdIdsInTable[dataGridViewTendons.CurrentRow.Cells[10].Value.ToString()];//获取本行钢束线的ObjectId
                    Polyline td = tdId.GetObject(OpenMode.ForRead) as Polyline;//获取本行钢束线
                    if ((bool)dataGridViewTendons.CurrentRow.Cells[5].Value == true)//如果右侧目前为张拉状态
                    {
                        dataGridViewTendons.CurrentRow.Cells[5].Value = false;//点击后变为不张拉状态
                        dataGridViewTendons.CurrentRow.Cells[4].Value = true;//左侧则必须为张拉状态
                        //左侧引伸量
                        dataGridViewTendons.CurrentRow.Cells[6].Value =
                                td.SingleDrawAmount(ctrlStress, kii, miu, -1, Ep).ToString("F0");
                        //右侧引伸量
                        dataGridViewTendons.CurrentRow.Cells[7].Value = "0";
                    }
                    else if ((bool)dataGridViewTendons.CurrentRow.Cells[5].Value == false)//右侧目前为不张拉状态
                    {
                        dataGridViewTendons.CurrentRow.Cells[5].Value = true;//右侧点击后变为张拉状态
                                                                             //左侧引伸量
                        dataGridViewTendons.CurrentRow.Cells[6].Value
                            = td.BothDrawAmount(ctrlStress, kii, miu, Ep)[0].ToString("F0");
                        //右侧引伸量
                        dataGridViewTendons.CurrentRow.Cells[7].Value
                            = td.BothDrawAmount(ctrlStress, kii, miu, Ep)[1].ToString("F0");
                    }
                    trans.Commit();
                }
            }
            #endregion
        }
        #endregion
        #region 输出表格按钮
        private void buttonExportTbl_Click(object sender, EventArgs e)
        {
            if (dataGridViewTendons.Rows.Count == 0)
            {
                MessageBox.Show("表中没有数据！");
                return;
            }
            this.Visible = false;//对话框消失
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            //1.提示用户输入插入点
            PromptPointResult ptRes = ed.GetPoint("\n选择表格插入点");
            if(ptRes.Status!= PromptStatus.OK)//如果选择不正确
            {
                this.Visible = true;//回到对话框
                return;
            }
            //2.先对钢束信息进行更新，类似于点击了更新并退出按钮，但显示更新成功对话框
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.更新有名对象字典中的总体参数
                db.ReadDlgToNamedDic(this);
                //1.1管道偏差系数
                if (!double.TryParse(textBoxKii.Text, out kii))
                {
                    MessageBox.Show("管道偏差系数输入有误！");
                    this.Visible = true;
                    return;
                }
                //1.2摩阻系数
                if (!double.TryParse(textBoxMiu.Text, out miu))
                {
                    MessageBox.Show("摩阻系数输入有误！");
                    this.Visible = true;
                    return;
                }
                //1.3钢束弹模
                double Ep = 1.95e5;
                if (!double.TryParse(textBoxEp.Text, out Ep))
                {
                    MessageBox.Show("钢束弹模输入有误！");
                    this.Visible = true;
                    return;
                }
                //1.4张拉控制应力
                if (!double.TryParse(textBoxCtrlStress.Text, out ctrlStress))
                {
                    MessageBox.Show("张拉控制应力输入有误！");
                    this.Visible = true;
                    return;
                }
                //1.5工作长度
                if (!double.TryParse(textBoxWorkLen.Text, out workLen))
                {
                    MessageBox.Show("工作长度输入有误！");
                    this.Visible = true;
                    return;
                }
                //2.更新钢束Xrecord信息,更新表中伸长量、总长信息（如果工作长度变化的话）
                for (int i = 0; i < dataGridViewTendons.Rows.Count; i++)
                {
                    ObjectId tdId = tdIdsInTable[dataGridViewTendons.Rows[i].Cells[10].Value.ToString()];
                    tdId.ReadRowToXrecord(dataGridViewTendons.Rows[i]);
                    XrecordManipulate.UpdateXRecordToRow(this, i, tdId, kii, miu, Ep, ctrlStress, workLen);
                }
                trans.Commit();//执行事务处理
            }
            //3.输出表格
            using (Transaction trans1 = db.TransactionManager.StartTransaction())//开始事务处理
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
                for (int i = 0; i < dataGridViewTendons.Rows.Count; i++)
                {
                    tdNames.Add(dataGridViewTendons.Rows[i].Cells[0].Value.ToString());
                    tdStyles.Add(dataGridViewTendons.Rows[i].Cells[1].Value.ToString());
                    tdNums.Add(int.Parse(dataGridViewTendons.Rows[i].Cells[2].Value.ToString()));
                    pipeDias.Add(double.Parse(dataGridViewTendons.Rows[i].Cells[3].Value.ToString()));
                    drawTypes.Add(
                        ((bool)(dataGridViewTendons.Rows[i].Cells[4].Value) ? -1 : 0)
                        + ((bool)(dataGridViewTendons.Rows[i].Cells[5].Value) ? 1 : 0)
                        );
                    leftDrawAmounts.Add(double.Parse(dataGridViewTendons.Rows[i].Cells[6].Value.ToString()));
                    rightDrawAmounts.Add(double.Parse(dataGridViewTendons.Rows[i].Cells[7].Value.ToString()));
                    clearLens.Add(double.Parse(dataGridViewTendons.Rows[i].Cells[8].Value.ToString()));
                    totalLens.Add(double.Parse(dataGridViewTendons.Rows[i].Cells[9].Value.ToString()));
                }
                //3.2 设置表格规格
                Table tb = new Table();//初始化钢束表格
                tb.SetSize(dataGridViewTendons.RowCount + 3, 14);//设置表格行数（表格行数、表头2行、合计1行）、列数
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
                var tdStylesDist = tdStyles.Select(c => c).Distinct().OrderBy(c => int.Parse(c.Remove(0,4))).ToList();
                //多一种钢束规格增加两列，分别为固定和张拉锚具，表格成形后如果本列锚具合计为零，再进行删除
                if (tdStylesDist.Count > 1) tb.InsertColumns(10 + pipeDiasDist.Count, 12, (tdStylesDist.Count - 1)*2);
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
                if(pipeDiasDist.Count > 1) tb.MergeCells(CellRange.Create(tb, 0, 9, 0, 8 + pipeDiasDist.Count));
                for (int i = 0; i < pipeDiasDist.Count; i++)
                {
                    tb.Cells[1, 9 + i].TextString = "Φ" + pipeDiasDist[i].ToString("F0");
                }
                //锚具
                tb.Cells[0, 9 + pipeDiasDist.Count].TextString = "锚具套数";
                tb.MergeCells(CellRange.Create(tb, 0, 9 + pipeDiasDist.Count, 0, 10 + pipeDiasDist.Count + (tdStylesDist.Count - 1) * 2));
                for (int i = 0; i < tdStylesDist.Count; i++)
                {
                    tb.Cells[1, 9 + pipeDiasDist.Count + 2 * i].TextString = tdStylesDist[i].Remove(0,1) + "张拉";
                    tb.Cells[1, 9 + pipeDiasDist.Count + 2 * i + 1].TextString = tdStylesDist[i].Remove(0, 1) + "固定";
                }
                //控制应力
                tb.Cells[0, tb.Columns.Count - 2].TextString = "控制应力\n(MPa)";
                tb.MergeCells(CellRange.Create(tb, 0, tb.Columns.Count - 2, 1, tb.Columns.Count - 2));
                //备注
                tb.Cells[0, tb.Columns.Count - 1].TextString = "备注";
                tb.MergeCells(CellRange.Create(tb, 0, tb.Columns.Count - 1, 1, tb.Columns.Count - 1));
                //3.4 填写表格内容
                for(int i = 0; i < dataGridViewTendons.Rows.Count; i++)//行迭代
                {
                    tb.Cells[2 + i, 0].TextString = tdNames[i];//名称
                    tb.Cells[2 + i, 1].TextString = tdStyles[i];//规格
                    tb.Cells[2 + i, 2].TextString = totalLens[i].ToString("F0");//钢束长度
                    tb.Cells[2 + i, 3].TextString = tdNums[i].ToString("F0");//钢束根数
                    tb.Cells[2 + i, 4].TextString = (totalLens[i]* tdNums[i] / 1000).ToString("F1");//钢束总长
                    tb.Cells[2 + i, 5].TextString = (int.Parse(tdStyles[i].Remove(0, 4)) * 1.101//总重
                        * (totalLens[i] * tdNums[i] / 1000)).ToString("F1");
                    tb.Cells[2 + i, 6].TextString = leftDrawAmounts[i].ToString("F0");//左侧引伸量
                    tb.Cells[2 + i, 7].TextString = rightDrawAmounts[i].ToString("F0");//右侧引伸量
                    tb.Cells[2 + i, 8].TextString = clearLens[i].ToString("F0");//波纹管长
                    for(int j = 0; j < pipeDiasDist.Count; j++)//管道总长
                    {
                        if (pipeDias[i] == pipeDiasDist[j])//找到对应直径列
                        {
                            tb.Cells[2 + i, 9 + j].TextString = (clearLens[i] * tdNums[i] / 1000).ToString("F1");
                            break;
                        }
                    }
                    for(int j = 0; j < tdStylesDist.Count; j++)//锚具套数
                    {
                        if (tdStyles[i] == tdStylesDist[j])//找到对应规格列
                        {
                            tb.Cells[2 + i, 9 + pipeDiasDist.Count + 2 * j].TextString//张拉套数
                                = (tdNums[i] * (2 - Math.Abs(drawTypes[i]))).ToString("F0");
                            tb.Cells[2 + i, 9 + pipeDiasDist.Count + 2 * j + 1].TextString//锚固套数
                                = (tdNums[i] * Math.Abs(drawTypes[i])).ToString("F0");
                        }
                    }
                    tb.Cells[2 + i, tb.Columns.Count - 2].TextString = ctrlStress.ToString("F0");//控制应力
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
                for(int j = 0; j < 2 * tdStylesDist.Count; j++)
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
                foreach(int iCol in voidCols)
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
            this.Dispose();
        }
        #endregion
    }
}