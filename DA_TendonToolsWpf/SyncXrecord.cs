using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
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
    public static class SyncXrecord
    {
        /// <summary>
        /// 将tdGenParas与图形数据库中的数据同步
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="tdGenParas">程序中的tdGenParas类</param>
        public static void SyncTdGenParasToDwg(this TendonGeneralParameters tdGenParas,Database db)
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
                    values.Add(DxfCode.Real, tdGenParas.Kii);
                    tdsDictNewId.AddXrecord2DBDict("kii", values);
                    //摩阻系数
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, tdGenParas.Miu);
                    tdsDictNewId.AddXrecord2DBDict("miu", values);
                    //钢束弹性模量
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, tdGenParas.Ep);
                    tdsDictNewId.AddXrecord2DBDict("Ep", values);
                    //张拉控制应力
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, tdGenParas.CtrlStress);
                    tdsDictNewId.AddXrecord2DBDict("ctrlStress", values);
                    //张拉端工作长度
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, tdGenParas.WorkLen);
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
                    tdGenParas.Kii = (double)vls[0].Value;
                    //摩阻系数
                    xrecId = tdsDict.GetAt("miu");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    tdGenParas.Miu = (double)vls[0].Value;
                    //钢束弹性模量
                    xrecId = tdsDict.GetAt("Ep");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    tdGenParas.Ep = (double)vls[0].Value;
                    //张拉控制应力
                    xrecId = tdsDict.GetAt("ctrlStress");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    tdGenParas.CtrlStress = (double)vls[0].Value;
                    //张拉端工作长度
                    xrecId = tdsDict.GetAt("workLen");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    tdGenParas.WorkLen = (double)vls[0].Value;                   
                }
                trans.Commit();//执行事务处理
            }
        }
        /// <summary>
        /// 将图形数据库与tdGenParas中的数据同步
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="tdGenParas">程序中的tdGenParas类</param>
        public static void SyncDwgToTdGenParas(this Database db, TendonGeneralParameters tdGenParas)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
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
                values.Add(DxfCode.Real, tdGenParas.Kii);
                tdsDictId.AddXrecord2DBDict("kii", values);
                //摩阻系数
                values = new TypedValueList();
                values.Add(DxfCode.Real, tdGenParas.Miu);
                tdsDictId.AddXrecord2DBDict("miu", values);
                //钢束弹性模量
                values = new TypedValueList();
                values.Add(DxfCode.Real, tdGenParas.Ep);
                tdsDictId.AddXrecord2DBDict("Ep", values);
                //张拉控制应力
                values = new TypedValueList();
                values.Add(DxfCode.Real, tdGenParas.CtrlStress);
                tdsDictId.AddXrecord2DBDict("ctrlStress", values);
                //张拉端工作长度
                values = new TypedValueList();
                values.Add(DxfCode.Real, tdGenParas.WorkLen);
                tdsDictId.AddXrecord2DBDict("workLen", values);
                trans.Commit();//执行事务处理
            }
        }
        /// <summary>
        /// 将tdGenParas与对话框中的内容同步
        /// </summary>
        /// <param name="tdGenParas">程序中的tdGenParas类</param>
        /// <param name="tdInfo">对话框</param>
        public static void SyncTdGenParasToDlg(this TendonGeneralParameters tdGenParas, TendonInfo tdInfo)
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
            double Ep = 1.95e5;
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
            tdGenParas.Kii = kii;
            tdGenParas.Miu = miu;
            tdGenParas.Ep = Ep;
            tdGenParas.CtrlStress = ctrlStress;
            tdGenParas.WorkLen = workLen;
        }

    }
}
