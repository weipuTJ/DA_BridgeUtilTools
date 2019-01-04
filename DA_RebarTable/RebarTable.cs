using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using DotNetARX;

namespace DA_RebarTable
{
    public class RebarTable
    {
        /// <summary>
        /// 钢筋参数表，各列分别为直径（mm）、截面积（mm2）、重量（kg/m)、外径（mm）
        /// </summary>
        private double[,] rebarParams = new double[15, 4]
        {
            {6,28.27,0.222,6 },
            {8,50.27,0.395,8 },
            {10,78.54,0.617,11.5},
            {12,113.10,0.888,13.5},
            {14,153.90,1.210,15.5},
            {16,201.10,1.580,18},
            {18,254.50,2.000,20},
            {20,314.20,2.470,22},
            {22,380.10,2.980,24},
            {25,490.90,3.850,27},
            {28,615.80,4.830,30.5},
            {32,804.20,6.310,34.5},
            {36,1018.00,7.990,36},
            {40,1257.00,9.870,40},
            {50,1964.00,15.420,50},
        };
        #region DA_AddNumbers
        /// <summary>
        /// 对所选文字进行求和（文字或多行文字），将求和结果输出为多行文字
        /// </summary>
        [CommandMethod("DA_AddNumbers")]
        public void DA_AddNumbers()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            double sumTotal = 0;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                PromptSelectionOptions selOpt = new PromptSelectionOptions();
                selOpt.MessageForAdding = "\n选择要相加的数字";
                PromptSelectionResult selRes = ed.GetSelection(selOpt);
                if (selRes.Status == PromptStatus.OK)
                {
                    SelectionSet ssTest = selRes.Value;
                    double sumTemp = 0;
                    foreach (ObjectId id in ssTest.GetObjectIds())
                    {
                        if (!(id.GetObject(OpenMode.ForRead) is MText) && !(id.GetObject(OpenMode.ForRead) is DBText))
                        {
                            ed.WriteMessage("\n选择集中包含非文字对象，请重新选择！");
                            return;
                        }
                        else if (id.GetObject(OpenMode.ForRead) is MText)
                        {
                            string contentTemp = (id.GetObject(OpenMode.ForRead) as MText).Text;
                            double numberTemp = 0;
                            if (double.TryParse(contentTemp, out numberTemp))
                            {
                                sumTemp += numberTemp;
                            }
                            else
                            {
                                ed.WriteMessage("\n选择集中包含非数字对象，请重新选择！");
                                sumTemp = 0;
                                return;
                            }
                        }
                        else if (id.GetObject(OpenMode.ForRead) is DBText)
                        {
                            string contentTemp = (id.GetObject(OpenMode.ForRead) as DBText).TextString;
                            double numberTemp = 0;
                            if (double.TryParse(contentTemp, out numberTemp))
                            {
                                sumTemp += numberTemp;
                            }
                            else
                            {
                                ed.WriteMessage("\n选择集中包含非数字对象，请重新选择！");
                                sumTemp = 0;
                                return;
                            }
                        }
                    }
                    sumTotal += sumTemp;
                }
                PromptPointOptions ptOpt = new PromptPointOptions("\n输入结果插入点");
                PromptPointResult ptRes = ed.GetPoint(ptOpt);
                SelectionSet ss = selRes.Value;
                if (ptRes.Status == PromptStatus.OK)
                {
                    MText sumResult = new MText();
                    sumResult.Location = ptRes.Value;
                    sumResult.Contents = sumTotal.ToString();
                    //文字样式、高度与所选文字中第一个相同
                    ObjectId fstId = ss.GetObjectIds()[0];
                    DBObject fstObj = fstId.GetObject(OpenMode.ForRead);
                    if (fstObj is DBText)
                    {
                        sumResult.TextHeight = ((DBText)fstObj).Height;
                        sumResult.TextStyleId = ((DBText)fstObj).TextStyleId;
                    }
                    else if (fstObj is MText)
                    {
                        sumResult.TextHeight = ((MText)fstObj).TextHeight;
                        sumResult.TextStyleId = ((MText)fstObj).TextStyleId;
                    }    
                    db.AddToModelSpace(sumResult);
                }
                trans.Commit();
            }
        }
        #endregion
        #region DA_RbTbl
        [CommandMethod("DA_RbTbl")]
        public void DA_RbTbl()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            double textHeight = 3.5;              //初始字高
            ObjectId textStyleId = db.Textstyle;  //初始样式
            double textWidthFactor = 0.75;         //初始宽度因子
            double[] yCdnts;//存放选择集的y坐标
            double[] dia;//存放直径
            double[] len;//存放长度
            double[] num;//存放根数
            double[] totalLen;//存放总长
            double[] unitWeight;//存放单位重
            double[] totalWeight;//存放总重
            double sumWeight;//存放合计重量
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //1.选择钢筋直径
                PromptSelectionOptions diaOpt = new PromptSelectionOptions();
                diaOpt.MessageForAdding = "\n选择钢筋直径";
                PromptSelectionResult diaRes = ed.GetSelection(diaOpt);
                SelectionSet diaSS = diaRes.Value;
                ObjectId[] diaIds = diaSS.GetObjectIds();
                if (NumberTest(diaIds) == 1)
                {
                    ed.WriteMessage("\n选择集中包含非文字对象，请重新选择！");
                    return;
                }
                else if (NumberTest(diaIds) == 2 || NumberTest(diaIds) == 3)
                {
                    ed.WriteMessage("\n选择集中包含非数字对象，请重新选择！");
                    return;
                }
                //2.选择钢筋长度
                PromptSelectionOptions lenOpt = new PromptSelectionOptions();
                lenOpt.MessageForAdding = "\n选择钢筋长度";
                PromptSelectionResult lenRes = ed.GetSelection(lenOpt);
                SelectionSet lenSS = lenRes.Value;
                ObjectId[] lenIds = lenSS.GetObjectIds();
                if (NumberTest(lenIds) == 1)
                {
                    ed.WriteMessage("\n选择集中包含非文字对象，请重新选择！");
                    return;
                }
                else if (NumberTest(lenIds) == 2 || NumberTest(lenIds) == 3)
                {
                    ed.WriteMessage("\n选择集中包含非数字对象，请重新选择！");
                    return;
                }
                //3.选择钢筋根数
                PromptSelectionOptions numOpt = new PromptSelectionOptions();
                numOpt.MessageForAdding = "\n选择钢筋根数";
                PromptSelectionResult numRes = ed.GetSelection(numOpt);
                SelectionSet numSS = numRes.Value;
                ObjectId[] numIds = numSS.GetObjectIds();
                if (NumberTest(numIds) == 1)
                {
                    ed.WriteMessage("\n选择集中包含非文字对象，请重新选择！");
                    return;
                }
                else if (NumberTest(numIds) == 2 || NumberTest(numIds) == 3)
                {
                    ed.WriteMessage("\n选择集中包含非数字对象，请重新选择！");
                    return;
                }
                //4.检验三者数量是否相等
                if (lenIds.Length != diaIds.Length || numIds.Length != diaIds.Length)
                {
                    ed.WriteMessage("\n各列数量不等，请重新选择！");
                    return;
                }
                //5.对各列数据按照Y坐标排序
                var diaIdsSorted =
                    from n in diaIds
                    orderby GetYCdnt(n)
                    select n;
                var lenIdsSorted =
                    from n in lenIds
                    orderby GetYCdnt(n)
                    select n;
                var numIdsSorted =
                    from n in numIds
                    orderby GetYCdnt(n)
                    select n;
                //6.提取排序后的数字，并进行运算
                dia = GetNumber(diaIdsSorted.ToArray());
                len = GetNumber(lenIdsSorted.ToArray());
                num = GetNumber(numIdsSorted.ToArray());
                totalLen = new double[dia.Length];//总长
                unitWeight = new double[dia.Length];//单位重
                totalWeight = new double[dia.Length];//总重
                for (int i = 0; i < dia.Length; i++)
                {
                    totalLen[i] = len[i] * num[i] / 1000;
                    unitWeight[i] = Math.Pow(dia[2], 2) * Math.PI / 4 * 7.85 / 1000;
                    totalWeight[i] = totalLen[i] * unitWeight[i];
                }
                sumWeight = totalWeight.Sum();
                //7.输出到指定位置,X由用户选取,Y与dia位置相同,字体自高与dia第一个文字相同
                yCdnts = GetYCdnt(diaIdsSorted.ToArray());
                DBObject fstObj = (diaIdsSorted.ToArray())[0].GetObject(OpenMode.ForRead);
                if (fstObj is DBText)
                {
                    textHeight = ((DBText)fstObj).Height;
                    textStyleId = ((DBText)fstObj).TextStyleId;
                    textWidthFactor = ((DBText)fstObj).WidthFactor;
                }
                else if (fstObj is MText)
                {
                    textHeight = ((MText)fstObj).TextHeight;
                    textStyleId = ((MText)fstObj).TextStyleId;
                    TextStyleTableRecord tstr = textStyleId.GetObject(OpenMode.ForRead) as TextStyleTableRecord;
                    textWidthFactor = tstr.XScale;
                }
                trans.Commit();
            }
            //8.1 输出总长
            using (Transaction trans1 = db.TransactionManager.StartTransaction())
            {
                PromptPointResult ttlRes = ed.GetPoint("\n指定总长对齐位置");
                for (int i = 0; i < dia.Length; i++)
                {
                    //总长
                    DBText ttlTxt = new DBText();
                    ttlTxt.TextStyleId = textStyleId;
                    ttlTxt.Height = textHeight;
                    ttlTxt.WidthFactor = textWidthFactor;
                    ttlTxt.TextString = totalLen[i].ToString("F3");
                    ttlTxt.Position = new Point3d(ttlRes.Value.X, yCdnts[i], 0);
                    db.AddToModelSpace(ttlTxt);
                }
                trans1.Commit();
            }
            //8.2 输出单位重量
            using (Transaction trans2 = db.TransactionManager.StartTransaction())
            {
                PromptPointResult uniwRes = ed.GetPoint("\n指定单位重量对齐位置");
                for (int i = 0; i < dia.Length; i++)
                {
                    //单位重
                    DBText uniwTxt = new DBText();
                    uniwTxt.TextStyleId = textStyleId;
                    uniwTxt.Height = textHeight;
                    uniwTxt.WidthFactor = textWidthFactor;
                    uniwTxt.TextString = unitWeight[i].ToString("F3");
                    uniwTxt.Position = new Point3d(uniwRes.Value.X, yCdnts[i], 0);
                    db.AddToModelSpace(uniwTxt);
                }
                trans2.Commit();
            }
            //8.3 输出总重
            using (Transaction trans3 = db.TransactionManager.StartTransaction())
            {
                PromptPointResult ttwRes = ed.GetPoint("\n指定总重对齐位置");
                for (int i = 0; i < dia.Length; i++)
                {
                    //总重
                    DBText ttwTxt = new DBText();
                    ttwTxt.TextStyleId = textStyleId;
                    ttwTxt.Height = textHeight;
                    ttwTxt.WidthFactor = textWidthFactor;
                    ttwTxt.TextString = totalWeight[i].ToString("F1");
                    ttwTxt.Position = new Point3d(ttwRes.Value.X, yCdnts[i], 0);
                    db.AddToModelSpace(ttwTxt);
                }
                trans3.Commit();
            }
            //8.4 输出合计重量
            using (Transaction trans4 = db.TransactionManager.StartTransaction())
            {
                PromptPointResult smwRes = ed.GetPoint("\n输入合计重量位置");
                DBText smwTxt = new DBText();
                smwTxt.TextStyleId = textStyleId;
                smwTxt.Height = textHeight;
                smwTxt.WidthFactor = textWidthFactor;
                smwTxt.TextString = "合计：  " + sumWeight.ToString("F1") + "  kg";
                smwTxt.Position = smwRes.Value;
                db.AddToModelSpace(smwTxt);
                trans4.Commit();
            }
        }
        /// <summary>
        /// 检验选择集中的实体是否可以转换为数字
        /// </summary>
        /// <param name="ids">选择集中实体的ObjectId</param>
        /// <returns>
        /// 0：可以转换为数字；
        /// 1：不是DBText或MText对象；
        /// 2：是DBText，但不能转换为数字；
        /// 3：是MText，但不能转换为数字。
        /// </returns>
        private int NumberTest(ObjectId[] ids)
        {
            foreach (ObjectId id in ids)
            {
                if (!(id.GetObject(OpenMode.ForRead) is MText) && !(id.GetObject(OpenMode.ForRead) is DBText))
                {
                    return 1;
                }
                else if (id.GetObject(OpenMode.ForRead) is MText)
                {
                    string contentTemp = (id.GetObject(OpenMode.ForRead) as MText).Text;
                    double numberTemp = 0;
                    if (!double.TryParse(contentTemp, out numberTemp))
                    {
                        return 2;
                    }
                }
                else if (id.GetObject(OpenMode.ForRead) is DBText)
                {
                    string contentTemp = (id.GetObject(OpenMode.ForRead) as DBText).TextString;
                    double numberTemp = 0;
                    if (!double.TryParse(contentTemp, out numberTemp))
                    {
                        return 3;
                    }
                }
            }
            return 0;
        }
        /// <summary>
        /// 返回选择文字的Y坐标
        /// </summary>
        /// <param name="id">选择文字的ObjectId</param>
        /// <returns>选择文字的Y坐标</returns>
        private double GetYCdnt(ObjectId id)
        {
            DBObject obj = id.GetObject(OpenMode.ForRead);
            if (obj is MText)
                return ((MText)obj).Location.Y;
            else if (obj is DBText)
                return ((DBText)obj).Position.Y;
            else
                return -99999;
        }
        /// <summary>
        /// 返回选择集文字的Y坐标
        /// </summary>
        /// <param name="ids">选择集文字的ObjectId数组</param>
        /// <returns>选择集文字的Y坐标数组</returns>
        private double[] GetYCdnt(ObjectId[] ids)
        {
            int num = ids.Length;
            double[] yCdnt = new double[num];
            for(int i = 0; i < num; i++)
            {
                yCdnt[i] = GetYCdnt(ids[i]);
            }
            return yCdnt;
        }
        /// <summary>
        /// 返回选择文字中包含的数字
        /// </summary>
        /// <param name="id">选择文字的ObjectId</param>
        /// <returns>选择文字中包含的数字</returns>
        private double GetNumber(ObjectId id)
        {
            double numberInString = -99999;
            DBObject obj = id.GetObject(OpenMode.ForRead);
            if (obj is MText)
            {
                string st = ((MText)obj).Text;
                double.TryParse(st, out numberInString);
            }
            else if(obj is DBText)
            {
                string st = ((DBText)obj).TextString;
                double.TryParse(st, out numberInString);
            }
            return numberInString;
        }
        /// <summary>
        /// 返回选择集文字中包含的数字
        /// </summary>
        /// <param name="ids">选择集文字的ObjectId数组</param>
        /// <returns>选择集文字中包含的数字数组</returns>
        private double[] GetNumber(ObjectId[] ids)
        {
            int num = ids.Length;
            double[] numberInString = new double[num];
            for (int i = 0; i < num; i++)
            {
                numberInString[i] = GetNumber(ids[i]);
            }
            return numberInString;
        }
        #endregion
    }
}
