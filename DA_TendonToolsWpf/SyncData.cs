using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using DotNetARX;

namespace DA_TendonToolsWpf
{
    /// <summary>
    /// 将程序中的类、表格内容等与dwg中记录的Xrecord信息同步
    /// </summary>
    public static class SyncData
    {
        /// <summary>
        /// 将tdGenParas与对话框中的内容同步
        /// </summary>
        /// <param name="tdGenParas">程序中的tdGenParas类</param>
        /// <param name="tdInfo">对话框</param>
        public static void SyncTdGenParasToDlg(TendonInfo tdInfo)
        {
            double kii, miu, Ep, ctrlStress, workLen;
            //1.1管道偏差系数
            if (!double.TryParse(tdInfo.textBoxKii.Text, out kii))
            {
                MessageBox.Show("管道偏差系数输入有误！");
                tdInfo.Show();
                return;
            }
            //1.2摩阻系数
            if (!double.TryParse(tdInfo.textBoxMiu.Text, out miu))
            {
                MessageBox.Show("摩阻系数输入有误！");
                tdInfo.Show();
                return;
            }
            //1.3钢束弹模
            if (!double.TryParse(tdInfo.textBoxEp.Text, out Ep))
            {
                MessageBox.Show("钢束弹模输入有误！");
                tdInfo.Show();
                return;
            }
            //1.4张拉控制应力
            if (!double.TryParse(tdInfo.textBoxCtrlStress.Text, out ctrlStress))
            {
                MessageBox.Show("张拉控制应力输入有误！");
                tdInfo.Show();
                return;
            }
            //1.5工作长度
            if (!double.TryParse(tdInfo.textBoxWorkLen.Text, out workLen))
            {
                MessageBox.Show("工作长度输入有误！");
                tdInfo.Show();
                return;
            }
            TendonGeneralParameters.Kii = kii;
            TendonGeneralParameters.Miu = miu;
            TendonGeneralParameters.Ep = Ep;
            TendonGeneralParameters.CtrlStress = ctrlStress;
            TendonGeneralParameters.WorkLen = workLen;
        }
        /// <summary>
        /// 将tdsInTbl与对话框同步
        /// </summary>
        /// <param name="tdsInTbl">程序中的tdsInTbl列表</param>
        /// <param name="idsInTbl">所选的钢束线的ObjectId所组成的List</param>
        public static void SyncTdsToDlg(ref ObservableCollection<Tendon> tdsInTbl, TendonInfo dlg)
        {
            for(int i = 0; i < tdsInTbl.Count; i++)//遍历各行并依次设置属性
            {
                //钢束名称
                tdsInTbl[i].TdName = (dlg.dataGridTdInfo.Columns[0].GetCellContent(dlg.dataGridTdInfo.Items[i]) as TextBlock).Text;
                //钢束规格
                tdsInTbl[i].TdStyle = (dlg.dataGridTdInfo.GetControl(i, 1, "comboBoxTdStyles") as ComboBox).Text;
                //钢束数量
                tdsInTbl[i].TdNum = int.Parse((dlg.dataGridTdInfo.Columns[2].GetCellContent(dlg.dataGridTdInfo.Items[i]) as TextBlock).Text);
                //管道直径
                tdsInTbl[i].TdPipeDia = double.Parse((dlg.dataGridTdInfo.Columns[3].GetCellContent(dlg.dataGridTdInfo.Items[i]) as TextBlock).Text);
                //张拉方式
                bool leftDraw = (bool)(dlg.dataGridTdInfo.GetControl(i, 4, "checkBoxLeftDraw") as CheckBox).IsChecked;
                bool rightDraw = (bool)(dlg.dataGridTdInfo.GetControl(i, 5, "checkBoxRightDraw") as CheckBox).IsChecked;
                if(leftDraw && !rightDraw)
                    tdsInTbl[i].TdDrawStyle = TendonDrawStyle.Left;
                else if(!leftDraw && rightDraw)
                    tdsInTbl[i].TdDrawStyle = TendonDrawStyle.Right;
                else
                    tdsInTbl[i].TdDrawStyle = TendonDrawStyle.Both;
            }

        }        
        /// <summary>
        /// 将钢束总体参数与图形数据库中的数据同步
        /// </summary>
        /// <param name="db">图形数据库</param>
        public static void SyncTdGenParasToDwg(Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                // 获取当前图形数据库的有名对象字典
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
                if (!dicts.Contains("DA_Tendons"))//如果字典中不含DA_Tendons的字典项
                {
                    ObjectId tdsDictNewId = db.AddNamedDictionary("DA_Tendons");//则添加该字典项
                    //将字典项内容与tdGenParas同步
                    //管道偏差系数
                    TypedValueList values = new TypedValueList();
                    values.Add(DxfCode.Real, TendonGeneralParameters.Kii);
                    tdsDictNewId.AddXrecord2DBDict("kii", values);
                    //摩阻系数
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, TendonGeneralParameters.Miu);
                    tdsDictNewId.AddXrecord2DBDict("miu", values);
                    //钢束弹性模量
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, TendonGeneralParameters.Ep);
                    tdsDictNewId.AddXrecord2DBDict("Ep", values);
                    //张拉控制应力
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, TendonGeneralParameters.CtrlStress);
                    tdsDictNewId.AddXrecord2DBDict("ctrlStress", values);
                    //张拉端工作长度
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, TendonGeneralParameters.WorkLen);
                    tdsDictNewId.AddXrecord2DBDict("workLen", values);
                }
                else//如果存在该字典项，则把tdGenParas各属性与图形数据库同步
                {
                    ObjectId tdsDictId = dicts.GetAt("DA_Tendons");
                    DBDictionary tdsDict = tdsDictId.GetObject(OpenMode.ForWrite) as DBDictionary; //获取DA_Tendons字典
                    //管道偏差系数
                    ObjectId xrecId = tdsDict.GetAt("kii");
                    Xrecord xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    TypedValueList vls = xrec.Data;
                    TendonGeneralParameters.Kii = (double)vls[0].Value;
                    //摩阻系数
                    xrecId = tdsDict.GetAt("miu");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    TendonGeneralParameters.Miu = (double)vls[0].Value;
                    //钢束弹性模量
                    xrecId = tdsDict.GetAt("Ep");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    TendonGeneralParameters.Ep = (double)vls[0].Value;
                    //张拉控制应力
                    xrecId = tdsDict.GetAt("ctrlStress");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    TendonGeneralParameters.CtrlStress = (double)vls[0].Value;
                    //张拉端工作长度
                    xrecId = tdsDict.GetAt("workLen");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    TendonGeneralParameters.WorkLen = (double)vls[0].Value;
                }
                dicts.DowngradeOpen();
                trans.Commit();//执行事务处理
            }
        }
        /// <summary>
        /// 将程序中的tdsInTbl和图形数据库同步
        /// </summary>
        /// <param name="tdsInTbl">程序中的tdsInTbl列表</param>
        /// <param name="idsInTbl">所选的钢束线的ObjectId所组成的List</param>
        public static void SyncTdsToDwg(ref ObservableCollection<Tendon> tdsInTbl, List<ObjectId> idsInTbl)
        {
            tdsInTbl = new ObservableCollection<Tendon>();//初始化tds
            int index = 0;
            foreach (ObjectId tdId in idsInTbl)
            {
                Tendon td = new Tendon();
                td.TdId = tdId;
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    //不含该参数则为钢束线增加默认值的Xrecord
                    //初始化各属性值
                    string tdName = $"F{1 + index}";//钢束名称
                    string tdStyle = "Φ15-12";//钢束规格
                    int tdNum = 1;//钢束根数
                    double tdPipeDia = 90;//管道直径
                    TendonDrawStyle tdDrawStyle = TendonDrawStyle.Both;//张拉方式
                    if (tdId.GetXrecord("DA_Tendons") == null)
                    {
                        TypedValueList values = new TypedValueList();
                        values.Add(DxfCode.Text, tdName);
                        values.Add(DxfCode.Text, tdStyle);
                        values.Add(DxfCode.Int16, (Int16)(int)tdNum);
                        values.Add(DxfCode.Real, tdPipeDia);
                        values.Add(DxfCode.Int16, (Int16)(int)tdDrawStyle);
                        tdId.AddXrecord("DA_Tendons", values);
                        index++;
                    }
                    else//如果存在该键值，采用Xrecord中记录的信息
                    {
                        tdName = (string)tdId.GetXrecord("DA_Tendons")[0].Value;
                        tdStyle = (string)tdId.GetXrecord("DA_Tendons")[1].Value;
                        tdNum = (int)(Int16)tdId.GetXrecord("DA_Tendons")[2].Value;
                        tdPipeDia = (double)tdId.GetXrecord("DA_Tendons")[3].Value;
                        tdDrawStyle = (TendonDrawStyle)(int)(Int16)tdId.GetXrecord("DA_Tendons")[4].Value;
                    }
                    td.TdName = tdName;
                    td.TdStyle = tdStyle;
                    td.TdNum = tdNum;
                    td.TdPipeDia = tdPipeDia;
                    td.TdDrawStyle = tdDrawStyle;
                    trans.Commit();
                }
                tdsInTbl.Add(td);
            }
        }
        
        /// <summary>
        /// 将图形数据库与tdsInTbl同步
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="tdsInTbl"></param>
        public static void SyncDwgToTds(this Database db, ObservableCollection<Tendon> tdsInTbl)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            using (DocumentLock loc = db.GetDocument().LockDocument())
            {
                for (int i = 0; i < tdsInTbl.Count; i++)//遍历各行并依次更新CAD中记录的Xrecord
                {
                    ObjectId tdId = tdsInTbl[i].TdId;
                    TypedValueList values = new TypedValueList();
                    values.Add(DxfCode.Text, tdsInTbl[i].TdName);
                    values.Add(DxfCode.Text, tdsInTbl[i].TdStyle);
                    values.Add(DxfCode.Int16, (Int16)(int)tdsInTbl[i].TdNum);
                    values.Add(DxfCode.Real, tdsInTbl[i].TdPipeDia);
                    values.Add(DxfCode.Int16, (Int16)(int)tdsInTbl[i].TdDrawStyle);
                    tdId.SetXrecord("DA_Tendons", values);
                }
                trans.Commit();//执行事务处理
            }
        }
        /// <summary>
        /// 将图形数据库与tdGenParas中的数据同步
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="tdGenParas">程序中的tdGenParas类</param>
        public static void SyncDwgToTdGenParas(this Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            using (DocumentLock loc = db.GetDocument().LockDocument())
            {
                // 获取当前图形数据库的有名对象字典
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
                ObjectId tdsDictId = new ObjectId();//
                if (!dicts.Contains("DA_Tendons"))//如果字典中不含DA_Tendons的字典项
                {
                    tdsDictId = db.AddNamedDictionary("DA_Tendons");//则添加该字典项
                }
                else//如果字典中含有DA_Tendons的字典项
                {
                    tdsDictId = dicts.GetAt("DA_Tendons");//则获取该字典项
                }
                //将字典项内容与tdGenParas同步
                //管道偏差系数
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Real, TendonGeneralParameters.Kii);
                tdsDictId.UpdateXrecord2DBDict("kii", values);
                //摩阻系数
                values = new TypedValueList();
                values.Add(DxfCode.Real, TendonGeneralParameters.Miu);
                tdsDictId.UpdateXrecord2DBDict("miu", values);
                //钢束弹性模量
                values = new TypedValueList();
                values.Add(DxfCode.Real, TendonGeneralParameters.Ep);
                tdsDictId.UpdateXrecord2DBDict("Ep", values);
                //张拉控制应力
                values = new TypedValueList();
                values.Add(DxfCode.Real, TendonGeneralParameters.CtrlStress);
                tdsDictId.UpdateXrecord2DBDict("ctrlStress", values);
                //张拉端工作长度
                values = new TypedValueList();
                values.Add(DxfCode.Real, TendonGeneralParameters.WorkLen);
                tdsDictId.UpdateXrecord2DBDict("workLen", values);
                dicts.DowngradeOpen();
                trans.Commit();//执行事务处理
            }
        }
        /// <summary>
        /// 为钢束线设置默认参数
        /// </summary>
        /// <param name="td"></param>
        public static void SetDefaultTendonParams(this Polyline td)
        {
            using (Transaction trans = td.Database.TransactionManager.StartTransaction())
            {
                //如果钢束线没有有名为DA_Tendons的扩展记录，则添加默认参数
                if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("DA_Tendons") == null)
                {
                    ObjectId tdId = td.ObjectId;
                    TypedValueList values = new TypedValueList();
                    values.Add(DxfCode.Text, "F1");
                    values.Add(DxfCode.Text, "Φ15-12");
                    values.Add(DxfCode.Int16, 1);
                    values.Add(DxfCode.Real, 90);
                    values.Add(DxfCode.Int16, (Int16)(int)TendonDrawStyle.Both);
                    tdId.SetXrecord("DA_Tendons", values);
                }
                trans.Commit();
            }
        }
    }
}
