using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using DotNetARX;

namespace DA_TendonTools
{
    /// <summary>
    /// 实体的Xrecord与DataGridView.Rows交互类
    /// </summary>
    public static class XrecordManipulate
    {
        /// <summary>
        /// 将多段线钢束中的Xrecord信息读入到表格中
        /// 没有Xrecord则按默认值输入
        /// </summary>
        /// <param name="tdInfo">窗体</param>
        /// <param name="td">钢束多段线</param>
        /// <param name="kii">管道偏差系数(1/m)</param>
        /// <param name="miu">摩阻系数</param>
        /// <param name="Ep">钢束弹模(MPa)</param>
        /// <param name="ctrlStress">张拉控制应力(MPa)</param>
        /// <param name="workLen">工作长度(mm)</param>
        /// <returns>新加行的行号</returns>
        public static int? ReadXRecordToRow(TendonInfo tdInfo,Polyline td,
            double kii,double miu,double Ep,double ctrlStress,double workLen)
        {
            int index = tdInfo.dataGridViewTendons.Rows.Add();//添加新行
            int tdDrawStyle = 0;//张拉方式
            #region 1.钢束名称
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdName") == null)
            {                
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Text, $"F{1 + index}");
                td.ObjectId.AddXrecord("tdName", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[0].Value = $"F{1 + index}";
            }
            else//如果存在该键值，采用Xrecord中记录的信息
            {
                string tdName = (string)td.ObjectId.GetXrecord("tdName")[0].Value;
                tdInfo.dataGridViewTendons.Rows[index].Cells[0].Value = tdName;
            }
            #endregion
            #region 2.钢束规格
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdStyle") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Text, "Φ15-12");
                td.ObjectId.AddXrecord("tdStyle", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[1].Value = "Φ15-12";
            }
            else//如果存在该键值，采用Xrecord中记录的信息
            {
                string tdStyle = (string)td.ObjectId.GetXrecord("tdStyle")[0].Value;
                tdInfo.dataGridViewTendons.Rows[index].Cells[1].Value = tdStyle;
            }
            #endregion
            #region 3.钢束根数
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdNum") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Int16, 1);
                td.ObjectId.AddXrecord("tdNum", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[2].Value = "1";
            }
            else//如果存在该键值，采用Xrecord中记录的信息
            {
                Int16 tdNum = (Int16)td.ObjectId.GetXrecord("tdNum")[0].Value;
                tdInfo.dataGridViewTendons.Rows[index].Cells[2].Value = tdNum.ToString();
            }
            #endregion
            #region 4.管道直径
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdPipeDia") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Real, 90);
                td.ObjectId.AddXrecord("tdPipeDia", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[3].Value = "90";
            }
            else//如果存在该键值，采用Xrecord中记录的信息
            {
                double tdPipeDia = (double)td.ObjectId.GetXrecord("tdPipeDia")[0].Value;
                tdInfo.dataGridViewTendons.Rows[index].Cells[3].Value = tdPipeDia.ToString("F0");
            }
            #endregion
            #region 5.张拉方式
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdDrawStyle") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Int16, 0);
                td.ObjectId.AddXrecord("tdDrawStyle", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[4].Value = true;
                tdInfo.dataGridViewTendons.Rows[index].Cells[5].Value = true;
            }
            else//如果存在该键值，采用Xrecord中记录的信息    
            {
                tdDrawStyle = (Int16)td.ObjectId.GetXrecord("tdDrawStyle")[0].Value;
                switch (tdDrawStyle)
                {
                    case -1://左侧张拉
                        tdInfo.dataGridViewTendons.Rows[index].Cells[4].Value = true;
                        tdInfo.dataGridViewTendons.Rows[index].Cells[5].Value = false;
                        break;
                    case 0://两侧张拉
                        tdInfo.dataGridViewTendons.Rows[index].Cells[4].Value = true;
                        tdInfo.dataGridViewTendons.Rows[index].Cells[5].Value = true;
                        break;
                    case 1://右侧张拉
                        tdInfo.dataGridViewTendons.Rows[index].Cells[4].Value = false;
                        tdInfo.dataGridViewTendons.Rows[index].Cells[5].Value = true;
                        break;
                }
            }
            #endregion
            #region 6.引伸量
            switch (tdDrawStyle)
            {
                case -1:
                    //左侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[6].Value
                       = td.SingleDrawAmount(ctrlStress, kii, miu, -1, Ep).ToString("F0");
                    //右侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[7].Value = "0";
                    break;
                case 0:
                    //左侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[6].Value
                        = td.BothDrawAmount(ctrlStress, kii, miu, Ep)[0].ToString("F0");
                    //右侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[7].Value
                        = td.BothDrawAmount(ctrlStress, kii, miu, Ep)[1].ToString("F0");
                    break;
                case 1:
                    //左侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[6].Value = "0";
                    //右侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[7].Value
                       = td.SingleDrawAmount(ctrlStress, kii, miu, 1, Ep).ToString("F0");
                    break;
            }
            #endregion
            #region 7.钢束长度
            //钢束净长
            tdInfo.dataGridViewTendons.Rows[index].Cells[8].Value = td.Length.ToString("F0");
            //钢束总长
            tdInfo.dataGridViewTendons.Rows[index].Cells[9].Value
                = (td.Length + (2 - Math.Abs(tdDrawStyle)) * workLen).ToString("F0");
            #endregion
            return index;
        }
        /// <summary>
        /// 重载ReadXRecordToRow方法，利用钢束线的ObjectId进行操作
        /// </summary>
        /// <param name="tdInfo">窗体</param>
        /// <param name="tdId">钢束多段线的ObjectId</param>
        /// <param name="kii">管道偏差系数(1/m)</param>
        /// <param name="miu">摩阻系数</param>
        /// <param name="Ep">钢束弹模(MPa)</param>
        /// <param name="ctrlStress">张拉控制应力(MPa)</param>
        /// /// <param name="workLen">工作长度(mm)</param>
        /// <returns>新加行的行号</returns>
        public static int? ReadXRecordToRow(TendonInfo tdInfo, ObjectId tdId,
            double kii, double miu, double Ep, double ctrlStress, double workLen)
        {
            Polyline td = tdId.GetObject(OpenMode.ForRead) as Polyline;
            if (td == null) return null;
            return ReadXRecordToRow(tdInfo, td, kii, miu, Ep, ctrlStress, workLen);
        }
        /// <summary>
        /// 根据钢束线td的Xrecord信息更新tdInfo中的第index行
        /// </summary>
        /// <param name="tdInfo">窗体</param>
        /// <param name="index">更新的行号</param>
        /// <param name="td">钢束线</param>
        /// <param name="kii">管道偏差系数(1/m)</param>
        /// <param name="miu">摩阻系数</param>
        /// <param name="Ep">钢束弹模(MPa)</param>
        /// <param name="ctrlStress">张拉控制应力(MPa)</param>
        /// <param name="workLen">工作长度(mm)</param> 
        public static void UpdateXRecordToRow(TendonInfo tdInfo, int index, Polyline td,
            double kii, double miu, double Ep, double ctrlStress, double workLen)
        {
            if (index < 0 || index > tdInfo.dataGridViewTendons.Rows.Count - 1) return;
            int tdDrawStyle = 0;//张拉方式
            #region 1.钢束名称
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdName") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Text, $"F{1 + index}");
                td.ObjectId.AddXrecord("tdName", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[0].Value = $"F{1 + index}";
            }
            else//如果存在该键值，采用Xrecord中记录的信息
            {
                string tdName = (string)td.ObjectId.GetXrecord("tdName")[0].Value;
                tdInfo.dataGridViewTendons.Rows[index].Cells[0].Value = tdName;
            }
            #endregion
            #region 2.钢束规格
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdStyle") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Text, "Φ15-12");
                td.ObjectId.AddXrecord("tdStyle", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[1].Value = "Φ15-12";
            }
            else//如果存在该键值，采用Xrecord中记录的信息
            {
                string tdStyle = (string)td.ObjectId.GetXrecord("tdStyle")[0].Value;
                tdInfo.dataGridViewTendons.Rows[index].Cells[1].Value = tdStyle;
            }
            #endregion
            #region 3.钢束根数
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdNum") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Int16, 1);
                td.ObjectId.AddXrecord("tdNum", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[2].Value = "1";
            }
            else//如果存在该键值，采用Xrecord中记录的信息
            {
                Int16 tdNum = (Int16)td.ObjectId.GetXrecord("tdNum")[0].Value;
                tdInfo.dataGridViewTendons.Rows[index].Cells[2].Value = tdNum.ToString();
            }
            #endregion
            #region 4.管道直径
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdPipeDia") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Real, 90);
                td.ObjectId.AddXrecord("tdPipeDia", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[3].Value = "90";
            }
            else//如果存在该键值，采用Xrecord中记录的信息
            {
                double tdPipeDia = (double)td.ObjectId.GetXrecord("tdPipeDia")[0].Value;
                tdInfo.dataGridViewTendons.Rows[index].Cells[3].Value = tdPipeDia.ToString("F0");
            }
            #endregion
            #region 5.张拉方式
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdDrawStyle") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Int16, 0);
                td.ObjectId.AddXrecord("tdDrawStyle", values);
                tdInfo.dataGridViewTendons.Rows[index].Cells[4].Value = true;
                tdInfo.dataGridViewTendons.Rows[index].Cells[5].Value = true;
            }
            else//如果存在该键值，采用Xrecord中记录的信息    
            {
                tdDrawStyle = (Int16)td.ObjectId.GetXrecord("tdDrawStyle")[0].Value;
                switch (tdDrawStyle)
                {
                    case -1://左侧张拉
                        tdInfo.dataGridViewTendons.Rows[index].Cells[4].Value = true;
                        tdInfo.dataGridViewTendons.Rows[index].Cells[5].Value = false;
                        break;
                    case 0://两侧张拉
                        tdInfo.dataGridViewTendons.Rows[index].Cells[4].Value = true;
                        tdInfo.dataGridViewTendons.Rows[index].Cells[5].Value = true;
                        break;
                    case 1://右侧张拉
                        tdInfo.dataGridViewTendons.Rows[index].Cells[4].Value = false;
                        tdInfo.dataGridViewTendons.Rows[index].Cells[5].Value = true;
                        break;
                }
            }
            #endregion
            #region 6.引伸量
            switch (tdDrawStyle)
            {
                case -1:
                    //左侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[6].Value
                       = td.SingleDrawAmount(ctrlStress, kii, miu, -1, Ep).ToString("F0");
                    //右侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[7].Value = "0";
                    break;
                case 0:
                    //左侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[6].Value
                        = td.BothDrawAmount(ctrlStress, kii, miu, Ep)[0].ToString("F0");
                    //右侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[7].Value
                        = td.BothDrawAmount(ctrlStress, kii, miu, Ep)[1].ToString("F0");
                    break;
                case 1:
                    //左侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[6].Value = "0";
                    //右侧引伸量
                    tdInfo.dataGridViewTendons.Rows[index].Cells[7].Value
                       = td.SingleDrawAmount(ctrlStress, kii, miu, 1, Ep).ToString("F0");
                    break;
            }
            #endregion
            #region 7.钢束长度
            //钢束净长
            tdInfo.dataGridViewTendons.Rows[index].Cells[8].Value = td.Length.ToString("F0");
            //钢束总长
            tdInfo.dataGridViewTendons.Rows[index].Cells[9].Value
                = (td.Length + (2 - Math.Abs(tdDrawStyle)) * workLen).ToString("F0");
            #endregion
        }
        /// <summary>
        /// 重载UpdateXRecordToRow方法，利用钢束线的ObjectId进行操作
        /// </summary>
        /// <param name="tdInfo">窗体</param>
        /// <param name="index">更新的行号</param>
        /// <param name="td">钢束线</param>
        /// <param name="kii">管道偏差系数(1/m)</param>
        /// <param name="miu">摩阻系数</param>
        /// <param name="Ep">钢束弹模(MPa)</param>
        /// <param name="ctrlStress">张拉控制应力(MPa)</param>
        /// <param name="workLen">工作长度(mm)</param> 
        public static void UpdateXRecordToRow(TendonInfo tdInfo, int index, ObjectId tdId,
            double kii, double miu, double Ep, double ctrlStress, double workLen)
        {
            Polyline td = tdId.GetObject(OpenMode.ForRead) as Polyline;
            if (td == null) return;
            UpdateXRecordToRow(tdInfo, index, td, kii, miu, Ep, ctrlStress, workLen);
        }
        /// <summary>
        /// 获取数据库中有名对象字典存储的总体参数kii、miu、Ep、ctrlStress、workLen
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <returns></returns>
        public static double[] GetOverallParams(this Database db)
        {
            double[] results = new double[5] {0.0015,0.16,1.95e5,1395,800};//输入kii、miu、Ep、ctrlStress、workLen默认值
            // 获取当前数据库的有名对象字典
            DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
            if (dicts.Contains("DA_Tendons"))//如果字典中含DA_Tendons的字典项
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
                {
                    // 如果已有名为DA_Tendons的字典项，则将其中数据读入界面中
                    dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForRead) as DBDictionary;//获取当前数据库有名对象字典
                    ObjectId tdsDictId = dicts.GetAt("DA_Tendons");
                    DBDictionary tdsDict = tdsDictId.GetObject(OpenMode.ForWrite) as DBDictionary; //获取DA_Tendons字典
                    //管道偏差系数
                    ObjectId xrecId = tdsDict.GetAt("kii");
                    Xrecord xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    TypedValueList vls = xrec.Data;
                    results[0] = (double)vls[0].Value;
                    //摩阻系数
                    xrecId = tdsDict.GetAt("miu");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    results[1] = (double)vls[0].Value;
                    //钢束弹性模量
                    xrecId = tdsDict.GetAt("Ep");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    results[2] = (double)vls[0].Value;
                    //张拉控制应力
                    xrecId = tdsDict.GetAt("ctrlStress");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    results[3] = (double)vls[0].Value;
                    //张拉端工作长度
                    xrecId = tdsDict.GetAt("workLen");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    results[4] = (double)vls[0].Value;
                    trans.Commit();//执行事务处理
                }
            }
            return results;
        }
        /// <summary>
        /// 设置默认钢束总体参数，即创建新的DA_Tendons有名对象字典项
        /// 如已有总体参数字典项则不做修改
        /// </summary>
        /// <param name="db">图形数据库</param>
        public static void SetDefaultOverallParams(this Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                // 获取当前数据库的有名对象字典
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
                if (!dicts.Contains("DA_Tendons"))//如果字典中不含DA_Tendons的字典项
                {
                    ObjectId tdsDictNewId = db.AddNamedDictionary("DA_Tendons");//则添加该字典项
                    //管道偏差系数
                    TypedValueList values = new TypedValueList();
                    values.Add(DxfCode.Real, 0.0015);
                    tdsDictNewId.AddXrecord2DBDict("kii", values);
                    //摩阻系数
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, 0.16);
                    tdsDictNewId.AddXrecord2DBDict("miu", values);
                    //钢束弹性模量
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, 1.95E5);
                    tdsDictNewId.AddXrecord2DBDict("Ep", values);
                    //张拉控制应力
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, 1395);
                    tdsDictNewId.AddXrecord2DBDict("ctrlStress", values);
                    //张拉端工作长度
                    values = new TypedValueList();
                    values.Add(DxfCode.Real, 800);
                    tdsDictNewId.AddXrecord2DBDict("workLen", values);
                }
            }
        }
        public static void SetDefaultTendonParams(this Polyline td)
        {
            //钢束名称默认为F1
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdName") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Text, "F1");
                td.ObjectId.AddXrecord("tdName", values);
            }
            //钢束规格默认为Φ15-12
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdStyle") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Text, "Φ15-12");
                td.ObjectId.AddXrecord("tdStyle", values);
            }
            //3.钢束根数默认为1
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdNum") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Int16, 1);
                td.ObjectId.AddXrecord("tdNum", values);
            }
            //4.管道直径默认为90
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdPipeDia") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Real, 90);
                td.ObjectId.AddXrecord("tdPipeDia", values);
            }
            //5.张拉方式默认为两端张拉，0
            if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdDrawStyle") == null)
            {
                TypedValueList values = new TypedValueList();
                values.Add(DxfCode.Int16, 0);
                td.ObjectId.AddXrecord("tdDrawStyle", values);
            }
        }
        /// <summary>
        /// 将表格数据读入至钢束线的Xrecord中
        /// </summary>
        /// <param name="row">表格</param>
        /// <param name="tdId">钢束多段线的ObjectId</param>
        public static void ReadRowToXrecord(this ObjectId tdId, DataGridViewRow row)
        {
            //钢束名称
            TypedValueList values = new TypedValueList();
            values.Add(DxfCode.Text, row.Cells[0].Value.ToString());
            tdId.SetXrecord("tdName", values);
            //钢束规格
            values = new TypedValueList();
            values.Add(DxfCode.Text, row.Cells[1].Value.ToString());
            tdId.SetXrecord("tdStyle", values);
            //钢束根数
            values = new TypedValueList();
            values.Add(DxfCode.Int16,Int16.Parse(row.Cells[2].Value.ToString()));
            tdId.SetXrecord("tdNum", values);
            //管道直径
            values = new TypedValueList();
            values.Add(DxfCode.Real, double.Parse(row.Cells[3].Value.ToString()));
            tdId.SetXrecord("tdPipeDia", values);
            //张拉方式
            values = new TypedValueList();
            if ((bool)(row.Cells[4].Value) == true && (bool)(row.Cells[5].Value) == false)//左侧张拉
            {
                values.Add(DxfCode.Int16, -1);
                tdId.SetXrecord("tdDrawStyle", values);
            }
            else if((bool)(row.Cells[4].Value) == true && (bool)(row.Cells[5].Value) == true)//两端张拉
            {
                values.Add(DxfCode.Int16, 0);
                tdId.SetXrecord("tdDrawStyle", values);
            }
            else if ((bool)(row.Cells[4].Value) == false && (bool)(row.Cells[5].Value) == true)//右侧张拉
            {
                values.Add(DxfCode.Int16, 1);
                tdId.SetXrecord("tdDrawStyle", values);
            }
        }
        /// <summary>
        /// 重载ReadRowToXrecord，将表格数据读入至钢束线的Xrecord中
        /// </summary>
        /// <param name="row">数据行</param>
        /// <param name="td">钢束多段线</param>
        public static void ReadRowToXrecord(this Polyline td,DataGridViewRow row)
        {
            ObjectId tdId = td.ObjectId;
            ReadRowToXrecord(tdId,row);
        }
        /// <summary>
        /// 将表格中的数据读入图形数据库中，更新有名对象字典中存储的钢束总体信息
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="tdinfo">钢束信息对话框</param>
        public static void ReadDlgToNamedDic(this Database db, TendonInfo tdInfo)
        {           
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                // 获取当前数据库的有名对象字典
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
                ObjectId tdsDictId = dicts.GetAt("DA_Tendons");
                DBDictionary tdsDict = tdsDictId.GetObject(OpenMode.ForWrite) as DBDictionary; //获取DA_Tendons字典
                //依次修改各总体参数
                //1.管道偏差系数
                ObjectId xrecId = tdsDict.GetAt("kii");
                Xrecord xrec = xrecId.GetObject(OpenMode.ForWrite) as Xrecord;
                TypedValueList vls = new TypedValueList();
                vls.Add(DxfCode.Real, double.Parse(tdInfo.textBoxKii.Text));
                xrec.Data = vls;
                xrec.DowngradeOpen();
                //2.摩阻系数
                xrecId = tdsDict.GetAt("miu");
                xrec = xrecId.GetObject(OpenMode.ForWrite) as Xrecord;
                vls = new TypedValueList();
                vls.Add(DxfCode.Real, double.Parse(tdInfo.textBoxMiu.Text));
                xrec.Data = vls;
                xrec.DowngradeOpen();
                //3.钢束弹性模量
                xrecId = tdsDict.GetAt("Ep");
                xrec = xrecId.GetObject(OpenMode.ForWrite) as Xrecord;
                vls = new TypedValueList();
                vls.Add(DxfCode.Real, double.Parse(tdInfo.textBoxEp.Text));
                xrec.Data = vls;
                xrec.DowngradeOpen();
                //4.张拉控制应力
                xrecId = tdsDict.GetAt("ctrlStress");
                xrec = xrecId.GetObject(OpenMode.ForWrite) as Xrecord;
                vls = new TypedValueList();
                vls.Add(DxfCode.Real, double.Parse(tdInfo.textBoxCtrlStress.Text));
                xrec.Data = vls;
                xrec.DowngradeOpen();
                //5.工作长度
                xrecId = tdsDict.GetAt("workLen");
                xrec = xrecId.GetObject(OpenMode.ForWrite) as Xrecord;
                vls = new TypedValueList();
                vls.Add(DxfCode.Real, double.Parse(tdInfo.textBoxWorkLen.Text));
                xrec.Data = vls;
                xrec.DowngradeOpen();
                trans.Commit();//执行事务处理
            }
        }
        /// <summary>
        /// 将图形数据库中的有名对象字典数据读入对话框中，显示钢束总体信息
        /// </summary>
        /// <param name="db">图形数据库</param>
        /// <param name="tdinfo">钢束信息对话框</param>
        public static void ReadNamedDicToDlg(this TendonInfo tdInfo,Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.操作有名对象字典，获取钢束总体信息
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
                //如果已有名为DA_Tendons的字典项，则将其中数据读入界面中
                ObjectId tdsDictId = dicts.GetAt("DA_Tendons");
                DBDictionary tdsDict = tdsDictId.GetObject(OpenMode.ForWrite) as DBDictionary; //获取DA_Tendons字典
                if(tdsDict != null)
                {
                    //管道偏差系数
                    ObjectId xrecId = tdsDict.GetAt("kii");
                    Xrecord xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    TypedValueList vls = xrec.Data;
                    tdInfo.textBoxKii.Text = vls[0].Value.ToString();
                    //摩阻系数
                    xrecId = tdsDict.GetAt("miu");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    tdInfo.textBoxMiu.Text = vls[0].Value.ToString();
                    //钢束弹性模量
                    xrecId = tdsDict.GetAt("Ep");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    tdInfo.textBoxEp.Text = vls[0].Value.ToString();
                    //张拉控制应力
                    xrecId = tdsDict.GetAt("ctrlStress");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    tdInfo.textBoxCtrlStress.Text = vls[0].Value.ToString();
                    //张拉端工作长度
                    xrecId = tdsDict.GetAt("workLen");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    tdInfo.textBoxWorkLen.Text = vls[0].Value.ToString();
                    trans.Commit();//执行事务处理
                }
            }
        }
    }
}
