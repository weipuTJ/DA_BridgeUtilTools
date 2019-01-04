using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;

namespace DA_SteelTable
{   
    public class DA_SteelTable
    {
        private double stDens = 7.85;//钢材密度为7.85e-3g/mm3
        [CommandMethod("DA_StTbl")]
        public void SteelTable()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            double textHeight = 3.5;              //初始字高
            ObjectId textStyleId = db.Textstyle;  //初始样式
            double textWidthFactor = 0.75;        //初始宽度因子
            double[] yCdnts;//存放选择集的y坐标
            double[] thk;//存放厚度
            double[] wid;//存放宽度
            double[] len;//存放长度
            double[] num;//存放块数
            double[] unitWeight;//存放单件重
            double[] totalWeight;//存放总重
            double sumWeight;//存放合计重量
            using (Transaction trans1 = db.TransactionManager.StartTransaction())
            {
                //1.选择钢板厚度
                PromptSelectionOptions thkOpt = new PromptSelectionOptions();
                thkOpt.MessageForAdding = "\n选择钢板厚度";
                PromptSelectionResult thkRes = ed.GetSelection(thkOpt);
                if (thkRes.Status != PromptStatus.OK) return;
                SelectionSet thkSS = thkRes.Value;
                ObjectId[] thkIds = thkSS.GetObjectIds();
                if (NumberTest(thkIds) == 1)
                {
                    ed.WriteMessage("\n选择集中包含非文字对象，请重新选择！");
                    return;
                }
                else if (NumberTest(thkIds) == 2 || NumberTest(thkIds) == 3)
                {
                    ed.WriteMessage("\n选择集中包含非数字对象，请重新选择！");
                    return;
                }
                //2.选择钢板宽度
                PromptSelectionOptions widOpt = new PromptSelectionOptions();
                widOpt.MessageForAdding = "\n选择钢板宽度";
                PromptSelectionResult widRes = ed.GetSelection(widOpt);
                if (widRes.Status != PromptStatus.OK) return;
                SelectionSet widSS = widRes.Value;
                ObjectId[] widIds = widSS.GetObjectIds();
                if (NumberTest(widIds) == 1)
                {
                    ed.WriteMessage("\n选择集中包含非文字对象，请重新选择！");
                    return;
                }
                else if (NumberTest(widIds) == 2 || NumberTest(widIds) == 3)
                {
                    ed.WriteMessage("\n选择集中包含非数字对象，请重新选择！");
                    return;
                }
                //3.选择钢板长度
                PromptSelectionOptions lenOpt = new PromptSelectionOptions();
                lenOpt.MessageForAdding = "\n选择钢板长度";
                PromptSelectionResult lenRes = ed.GetSelection(lenOpt);
                if (lenRes.Status != PromptStatus.OK) return;
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
                //4.选择钢板数量
                PromptSelectionOptions numOpt = new PromptSelectionOptions();
                numOpt.MessageForAdding = "\n选择钢板数量";
                PromptSelectionResult numRes = ed.GetSelection(numOpt);
                if (numRes.Status != PromptStatus.OK) return;
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
                //5.检验四者数量是否相等
                if (lenIds.Length != thkIds.Length || widIds.Length != thkIds.Length || numIds.Length != thkIds.Length)
                {
                    ed.WriteMessage("\n各列数量不等，请重新选择！");
                    return;
                }
                //6.对各列数据按照Y坐标排序               
                var thkIdsSorted =
                   from n in thkIds
                   orderby GetYCdnt(n)
                   select n;
                var widIdsSorted =
                   from n in widIds
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
                //7.提取排序后的数字，并进行运算
                thk = GetNumber(thkIdsSorted.ToArray());
                wid = GetNumber(widIdsSorted.ToArray());
                len = GetNumber(lenIdsSorted.ToArray());
                num = GetNumber(numIdsSorted.ToArray());
                unitWeight = new double[thk.Length];//单件重
                totalWeight = new double[thk.Length];//总重
                for (int i = 0; i < thk.Length; i++)
                {
                    unitWeight[i] = thk[i] * wid[i] * len[i] * stDens / 1e6;
                    totalWeight[i] = num[i] * unitWeight[i];
                }
                sumWeight = totalWeight.Sum();
                //8.输出到指定位置,X由用户选取,Y与len位置相同,字体自高与len第一个文字相同
                yCdnts = GetYCdnt(thkIdsSorted.ToArray());
                DBObject fstObj = (thkIdsSorted.ToArray())[0].GetObject(OpenMode.ForRead);
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
                trans1.Commit();
            }
            //9.以此输出结果数据
            //9.1 输出单件重量
            using (Transaction trans2 = db.TransactionManager.StartTransaction())
            {
                PromptPointResult uniwRes = ed.GetPoint("\n指定单件重量对齐位置");
                for (int i = 0; i < thk.Length; i++)
                {
                    //单件重
                    DBText uniwTxt = new DBText();
                    uniwTxt.TextStyleId = textStyleId;
                    uniwTxt.Height = textHeight;
                    uniwTxt.WidthFactor = textWidthFactor;
                    uniwTxt.TextString = unitWeight[i].ToString("F1");
                    uniwTxt.Position = new Point3d(uniwRes.Value.X, yCdnts[i], 0);
                    db.AddToModelSpace(uniwTxt);
                }
                trans2.Commit();
            }
            //9.2 输出总重
            using (Transaction trans3 = db.TransactionManager.StartTransaction())
            {
                PromptPointResult ttwRes = ed.GetPoint("\n指定总重对齐位置");
                for (int i = 0; i < thk.Length; i++)
                {
                    //总重
                    DBText ttwTxt = new DBText();
                    ttwTxt.TextStyleId = textStyleId;
                    ttwTxt.Height = textHeight;
                    ttwTxt.WidthFactor = textWidthFactor;
                    ttwTxt.TextString = totalWeight[i].ToString("F0");
                    ttwTxt.Position = new Point3d(ttwRes.Value.X, yCdnts[i], 0);
                    db.AddToModelSpace(ttwTxt);
                }
                trans3.Commit();
            }
            //9.3 输出合计重量
            using (Transaction trans4 = db.TransactionManager.StartTransaction())
            {
                PromptPointResult smwRes = ed.GetPoint("\n输入合计重量位置");
                DBText smwTxt = new DBText();
                smwTxt.TextStyleId = textStyleId;
                smwTxt.Height = textHeight;
                smwTxt.WidthFactor = textWidthFactor;
                smwTxt.TextString = "合计：  " + sumWeight.ToString("F0") + "  kg";
                smwTxt.Position = smwRes.Value;
                db.AddToModelSpace(smwTxt);
                trans4.Commit();
            }
        }
        #region DA_AddNumbers
        /// <summary>
        /// 对所选文字进行求和（文字或多行文字），将求和结果输出为多行文字
        /// </summary>
        [CommandMethod("DA_AddNumbers")]
        public void AddNumbers()
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
        #region Util functions
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
            for (int i = 0; i < num; i++)
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
            else if (obj is DBText)
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
