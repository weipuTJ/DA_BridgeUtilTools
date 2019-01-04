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

namespace DA_TendonTools
{
    public enum DrawType
    {
        LefeEnd = -1,
        RightEnd = 1,
        BothEnd = 0
    };
    public class Commands
    {
        internal double scale = 100;//绘图比例缺省值
        internal double kii = 0.0015;//局部偏差影响系数缺省值，1/m
        internal double miu = 0.16;//摩擦系数，无量纲
        internal double Ep = 1.95e5;//钢束弹性模量，MPa
        internal double ctrlStress = 1395;//张拉控制应力，MPa
        internal double workLen = 800;//默认工作长度,mm
        /// <summary>
        /// 单根钢束标注命令
        /// </summary>
        [CommandMethod("DA_TendonAnnotation")]
        public void TendonAnnotation()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            db.SetDefaultOverallParams();//设置默认总体参数，已有总体参数字典项则无动作
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                #region 1.选择梁顶缘线
                PromptEntityOptions tpLineOpt = new PromptEntityOptions("\n选择梁顶缘线");
                tpLineOpt.SetRejectMessage("\n顶缘线应为直线、圆弧或多段线");
                tpLineOpt.AddAllowedClass(typeof(Line), true);//可以选择直线
                tpLineOpt.AddAllowedClass(typeof(Polyline), true);//可以选择多段线
                tpLineOpt.AddAllowedClass(typeof(Arc), true);//可以选择圆弧线
                PromptEntityResult tpLineRes = ed.GetEntity(tpLineOpt);
                if (tpLineRes.Status != PromptStatus.OK) return;
                ObjectId tpLineId = tpLineRes.ObjectId;
                Curve tpLine = trans.GetObject(tpLineId, OpenMode.ForRead) as Curve;
                #endregion
                #region 2.选择钢束线
                PromptEntityOptions tdLineOpt = new PromptEntityOptions("\n选择钢束");
                tdLineOpt.SetRejectMessage("\n钢束应为多段线");
                tdLineOpt.AddAllowedClass(typeof(Polyline), true);//仅能选择多段线      
                PromptEntityResult tdLineRes = ed.GetEntity(tdLineOpt);
                if (tdLineRes.Status != PromptStatus.OK) return;
                ObjectId tdLineId = tdLineRes.ObjectId;
                Polyline tdLine = trans.GetObject(tdLineId, OpenMode.ForRead) as Polyline;
               
                //判断钢束线是否在顶缘线以内，否则报错返回
                if (tdLine.StartPoint.X < tpLine.StartPoint.X || tdLine.EndPoint.X > tpLine.EndPoint.X)
                {
                    Application.ShowAlertDialog("钢束线超出顶缘线，请检查！");
                    return;
                }
                tdLine.SetDefaultTendonParams();//设置钢束默认参数，如已有Xrecord信息则无动作
                #endregion
                #region 3.设置绘图参数（包括张拉方式和工作长度设置）
                //3.1 尺寸标注绘图位置及张拉方式和工作长度设置
                Point3d pos = new Point3d();//初始化标注点
                PromptPointOptions posOpt = new PromptPointOptions("\n设置标注线位置或设置[张拉方式(D)/工作长度(W)]");
                posOpt.Keywords.Add("D");
                posOpt.Keywords.Add("W");
                posOpt.AppendKeywordsToMessage = false;//提示信息中不显示关键字
                //获取钢束张拉方式
                int tdDrawStyle = 0;//默认为两端张拉
                if(!tdLine.ExtensionDictionary.IsNull && tdLine.ObjectId.GetXrecord("tdDrawStyle")!=null)//如果钢束线有扩展记录则取扩展记录数据
                {
                    tdDrawStyle = (Int16)tdLine.ObjectId.GetXrecord("tdDrawStyle")[0].Value;
                }
                //获取工作长度信息
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
                if (dicts.Contains("DA_Tendons"))//如果字典中含DA_Tendons的字典项
                {
                    ObjectId tdsDictId = dicts.GetAt("DA_Tendons");
                    DBDictionary tdsDict = tdsDictId.GetObject(OpenMode.ForRead) as DBDictionary; //获取DA_Tendons字典
                    ObjectId xrecId = tdsDict.GetAt("workLen");//获取字典中的工作长度项
                    Xrecord xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;//获取工作长度项中的Xrecird
                    TypedValueList vls = xrec.Data;//获取Xrecord中的TypedValueList数据
                    workLen = (double)vls[0].Value;//根据TypedValueList数据中的数值更新工作长度workLen
                }
                for (;;)
                {
                    PromptPointResult posRes = ed.GetPoint(posOpt);
                    if (posRes.Status == PromptStatus.Keyword)
                    {
                        switch(posRes.StringResult)
                        {
                            case "D"://选择修改张拉方式
                                PromptIntegerOptions drwOpt = new PromptIntegerOptions($"\n输入张拉方式[两端张拉(0)/左端张拉[-1]/右端张拉[1]<{tdDrawStyle}>");
                                drwOpt.AllowNone = true;//允许ESC退出
                                PromptIntegerResult drwRes = ed.GetInteger(drwOpt);
                                if (drwRes.Value == 0)
                                {
                                    tdDrawStyle = 0;
                                }
                                else if (drwRes.Value == -1)
                                {
                                    tdDrawStyle = -1;
                                }
                                else if (drwRes.Value == 1)
                                {
                                    tdDrawStyle = 1;
                                }
                                TypedValueList values = new TypedValueList();//根据输入更新钢束线的Xrecord记录
                                values.Add(DxfCode.Int16, tdDrawStyle);
                                tdLine.ObjectId.SetXrecord("tdDrawStyle", values);
                                break;
                            case "W"://修改工作长度
                                PromptDoubleOptions wklOpt = new PromptDoubleOptions($"\n输入工作长度<{workLen.ToString("F0")}>");
                                wklOpt.AllowNone = true;//允许ESC退出
                                PromptDoubleResult wklRes = ed.GetDouble(wklOpt);
                                if (wklRes.Status == PromptStatus.OK)
                                {
                                    workLen = wklRes.Value;
                                    ObjectId tdsDictId = dicts.GetAt("DA_Tendons");//更新DA_Tendons字典中的钢束总体参数
                                    DBDictionary tdsDict = tdsDictId.GetObject(OpenMode.ForRead) as DBDictionary; 
                                    ObjectId xrecId = tdsDict.GetAt("workLen");
                                    Xrecord xrec = xrecId.GetObject(OpenMode.ForWrite) as Xrecord;
                                    TypedValueList vls = new TypedValueList();
                                    vls.Add(DxfCode.Real, workLen);
                                    xrec.Data = vls;
                                    xrec.DowngradeOpen();
                                }
                                break;
                        }
                    }
                    else if (posRes.Status == PromptStatus.OK)
                    {
                        pos = posRes.Value;
                        break;
                    }
                }
                //3.2 绘图比例
                PromptDoubleOptions scaleOpt = new PromptDoubleOptions($"\n设置绘图比例<{scale}>");
                scaleOpt.AllowNone = true;//允许回车，则采用前次比例
                scaleOpt.AllowNegative = false;//不允许负值
                scaleOpt.AllowZero = false;//不允许零值
                PromptDoubleResult scaleRes = ed.GetDouble(scaleOpt);//获取比例
                if (scaleRes.Status != PromptStatus.OK && scaleRes.Status != PromptStatus.None) return;
                else if (scaleRes.Status == PromptStatus.OK) scale = scaleRes.Value;
                #endregion
                #region 4.建立各类标注
                List<Point3d> ptsH = new List<Point3d>();//创建水平标注点集
                List<Dimension> dims = new List<Dimension>();//创建标注集，存放各类标注
                for (int i = 0; i < tdLine.NumberOfVertices - 1; i++)
                {
                    //4.1 水平点集
                    ptsH.Add(tdLine.GetPoint3dAt(i));

                    //4.2 每段钢束线的长度
                    //4.3 直线标注角度
                    //4.4 圆弧线标注半径
                    if (tdLine.GetSegmentType(i) == SegmentType.Line)
                    {
                        LineSegment3d lineSeg = tdLine.GetLineSegmentAt(i);
                        //4.2 每段钢束线的长度
                        db.LineLengthDim(lineSeg, scale);
                        //4.3 直线标注角度
                        if (tdLine.StartPoint.X < tdLine.EndPoint.X)
                            db.LineAngelDim(lineSeg, !(i == tdLine.NumberOfVertices - 2),scale);
                        else
                            db.LineAngelDim(lineSeg, (i == tdLine.NumberOfVertices - 2), scale);
                    }
                    else if (tdLine.GetSegmentType(i) == SegmentType.Arc)
                    {
                        CircularArc3d arcSeg = tdLine.GetArcSegmentAt(i);
                        //4.2 每段钢束线的长度
                        db.ArcLengthDim(arcSeg, scale);
                        //4.3 圆弧标注半径
                        db.ArrowRadiusDim(arcSeg, scale);
                    }
                    //4.5 竖直距离标注
                    Ray vRay = new Ray();//建立竖直射线
                    vRay.BasePoint = tdLine.GetPoint3dAt(i);
                    vRay.UnitDir = new Vector3d(0, 1, 0);
                    Point3dCollection ptIntersects = new Point3dCollection();
                    tpLine.IntersectWith(vRay, Intersect.OnBothOperands, ptIntersects, IntPtr.Zero, IntPtr.Zero);
                    Point3d ptIntersect = ptIntersects[0];
                    RotatedDimension dimV = new RotatedDimension();
                    dimV.XLine1Point = tdLine.GetPoint3dAt(i);    //第一条尺寸边线
                    dimV.XLine2Point = ptIntersect;               //第二条尺寸边线
                    dimV.DimLinePoint = tdLine.GetPoint3dAt(i);   //尺寸线位置
                    dimV.Rotation = Math.PI / 2;                  //标注旋转90度
                    dimV.DimensionStyle = db.Dimstyle;            //尺寸样式为当前样式
                    dimV.Dimscale = scale;                        //设置尺寸全局比例
                    dims.Add(dimV);
                }
                //4.1 节点间距点集缺钢束最后一个点、梁顶缘线端点
                ptsH.Add(tdLine.EndPoint);
                ptsH.Add(tpLine.StartPoint);
                ptsH.Add(tpLine.EndPoint);
                db.ContinuedHorizontalDims(ptsH, pos, scale);//建立水平连续标注

                //4.5 竖直距离标注缺最后一个点
                Ray vRayLast = new Ray();//建立竖直射线
                vRayLast.BasePoint = tdLine.GetPoint3dAt(tdLine.NumberOfVertices - 1);
                vRayLast.UnitDir = new Vector3d(0, 1, 0);
                Point3dCollection ptIntersectsLast = new Point3dCollection();
                tpLine.IntersectWith(vRayLast, Intersect.OnBothOperands, ptIntersectsLast, IntPtr.Zero, IntPtr.Zero);
                Point3d ptIntersectLast = ptIntersectsLast[0];
                RotatedDimension dimVLast = new RotatedDimension();
                dimVLast.XLine1Point = tdLine.GetPoint3dAt(tdLine.NumberOfVertices - 1);    //第一条尺寸边线
                dimVLast.XLine2Point = ptIntersectLast;               //第二条尺寸边线
                dimVLast.DimLinePoint = tdLine.GetPoint3dAt(tdLine.NumberOfVertices - 1);   //尺寸线位置
                dimVLast.Rotation = Math.PI / 2;                  //标注旋转90度
                dimVLast.DimensionStyle = db.Dimstyle;            //尺寸样式为当前样式
                dimVLast.Dimscale = scale;                         //设置尺寸全局比例
                dims.Add(dimVLast);
                #endregion
                #region 5 绘制张拉端
                //5.1 获取张拉端几何特征
                //获取钢束线真实的起点和终点
                Point3d tdStart = (tdLine.StartPoint.X < tdLine.EndPoint.X) ? tdLine.StartPoint : tdLine.EndPoint;
                Point3d tdEnd = (tdLine.StartPoint.X < tdLine.EndPoint.X) ? tdLine.EndPoint : tdLine.StartPoint;
                //获取钢束线真实的起终点角度
                double iclStart = (tdLine.StartPoint.X < tdLine.EndPoint.X) ?
                    tdLine.GetLineSegmentAt(0).GetAngleOfLineSeg() : tdLine.GetLineSegmentAt(tdLine.NumberOfVertices - 2).GetAngleOfLineSeg();
                double iclEnd = (tdLine.StartPoint.X < tdLine.EndPoint.X) ?
                    tdLine.GetLineSegmentAt(tdLine.NumberOfVertices - 2).GetAngleOfLineSeg() : tdLine.GetLineSegmentAt(0).GetAngleOfLineSeg();
                //初始化张拉端图元
                Polyline leftDraw = new Polyline();
                Polyline rightDraw = new Polyline();
                MText lengthL = new MText();
                MText lengthR = new MText();
                //5.2 左侧张拉端
                //5.2.1 两侧张拉或左侧张拉时左端绘制工作长度线
                if (tdDrawStyle == 0 || tdDrawStyle == -1)
                {
                    //创建张拉端几何点
                    Point3d tdDrawL = GeTools.PolarPoint(tdStart, iclStart, -workLen);
                    //创建张拉段
                    leftDraw = new Polyline();
                    leftDraw.AddVertexAt(0, tdStart.ToPoint2d(), 0, 0, 0);
                    leftDraw.AddVertexAt(1, tdDrawL.ToPoint2d(), 0, 0, 0); 
                    leftDraw.Layer = tdLine.Layer;//张拉段与钢束线应该在同一层
                    //标注左侧张拉段
                    lengthL = new MText();
                    //长度
                    lengthL.Contents = "工作长度800";
                    //文字高度
                    lengthL.TextHeight = 3 * scale;
                    //样式为当前样式
                    lengthL.TextStyleId = db.Textstyle;
                    //旋转角度同直线段倾角
                    lengthL.Rotation = iclStart;
                    //对齐位置为右上
                    lengthL.Attachment = AttachmentPoint.TopRight;
                    //位置为中点垂线以下0.5个单位
                    lengthL.Location = GeTools.PolarPoint(GeTools.MidPoint(leftDraw.StartPoint,
                        leftDraw.EndPoint), iclStart - Math.PI / 2, 0.5 * scale);
                }
                //5.2.2 右侧张拉时绘制P锚标识
                else
                {
                    //创建P锚起终点
                    Point3d tdDrawL1 = GeTools.PolarPoint(tdStart, iclStart + Math.PI / 2, 0.75 * scale);
                    Point3d tdDrawL2 = GeTools.PolarPoint(tdStart, iclStart + Math.PI / 2, -0.75 * scale);
                    //创建P锚标志
                    leftDraw = new Polyline();
                    leftDraw.AddVertexAt(0, tdDrawL1.ToPoint2d(), 0, 0.35 * scale, 0.35 * scale);
                    leftDraw.AddVertexAt(1, tdDrawL2.ToPoint2d(), 0, 0.35 * scale, 0.35 * scale);
                    leftDraw.Layer = tdLine.Layer;//张拉段与钢束线应该在同一层
                    //标注左侧P锚
                    lengthL = new MText();
                    //长度
                    lengthL.Contents = "P锚";
                    //文字高度
                    lengthL.TextHeight = 3 * scale;
                    //样式为当前样式
                    lengthL.TextStyleId = db.Textstyle;
                    //旋转角度同直线段倾角
                    lengthL.Rotation = iclStart;
                    //对齐位置为右中
                    lengthL.Attachment = AttachmentPoint.MiddleRight;
                    //位置为P锚标志右侧0.5个单位
                    lengthL.Location = GeTools.PolarPoint(GeTools.MidPoint(leftDraw.StartPoint,
                        leftDraw.EndPoint), iclStart, -2 * scale);
                }
                //5.3 右侧张拉端绘制
                //5.3.1 两侧张拉或右侧张拉时右端绘制工作长度线
                if (tdDrawStyle == 0 || tdDrawStyle == 1)
                {
                    //创建张拉端几何点
                    Point3d tdDrawR = GeTools.PolarPoint(tdEnd, iclEnd, 800);
                    //创建张拉段
                    rightDraw = new Polyline();
                    rightDraw.AddVertexAt(0, tdEnd.ToPoint2d(), 0, 0, 0);
                    rightDraw.AddVertexAt(1, tdDrawR.ToPoint2d(), 0, 0, 0);
                    rightDraw.Layer = tdLine.Layer;//张拉段与钢束线应该在同一层
                    //标注右侧张拉段
                    lengthR = new MText();
                    //长度
                    lengthR.Contents = "工作长度800";
                    //文字高度
                    lengthR.TextHeight = 3 * scale;
                    //样式为当前样式
                    lengthR.TextStyleId = db.Textstyle;
                    //旋转角度同直线段倾角
                    lengthR.Rotation = iclEnd;
                    //对齐位置为左上
                    lengthR.Attachment = AttachmentPoint.TopLeft;
                    //位置为中点垂线以下0.5个单位
                    lengthR.Location = GeTools.PolarPoint(GeTools.MidPoint(rightDraw.StartPoint,
                        rightDraw.EndPoint), iclEnd - Math.PI / 2, 0.5 * scale);
                }
                //5.2.2 左侧张拉时绘制P锚标识
                else//绘制P锚
                {
                    //创建P锚起终点
                    Point3d tdDrawR1 = GeTools.PolarPoint(tdEnd, iclEnd + Math.PI / 2, 0.75 * scale);
                    Point3d tdDrawR2 = GeTools.PolarPoint(tdEnd, iclEnd + Math.PI / 2, -0.75 * scale);
                    //创建P锚标志
                    rightDraw = new Polyline();
                    rightDraw.AddVertexAt(0, tdDrawR1.ToPoint2d(), 0, 0.35 * scale, 0.35 * scale);
                    rightDraw.AddVertexAt(1, tdDrawR2.ToPoint2d(), 0, 0.35 * scale, 0.35 * scale);
                    rightDraw.Layer = tdLine.Layer;//张拉段与钢束线应该在同一层
                    //标注左侧P锚
                    lengthR = new MText();
                    //长度
                    lengthR.Contents = "P锚";
                    //文字高度
                    lengthR.TextHeight = 3 * scale;
                    //样式为当前样式
                    lengthR.TextStyleId = db.Textstyle;
                    //旋转角度同直线段倾角
                    lengthR.Rotation = iclEnd;
                    //对齐位置为左中
                    lengthR.Attachment = AttachmentPoint.MiddleLeft;
                    //位置为P锚标志右侧0.5个单位
                    lengthR.Location = GeTools.PolarPoint(GeTools.MidPoint(rightDraw.StartPoint,
                        rightDraw.EndPoint), iclEnd, 2 * scale);
                }
                #endregion
                #region 6 在截面顶缘标识“梁顶缘线”
                Point3d midPt = GeTools.MidPoint(tpLine.StartPoint, tpLine.EndPoint);//顶缘线起终点中点
                Point3d midPtInTp = tpLine.GetClosestPointTo(midPt, Vector3d.YAxis, true);//顶缘线上靠近中点的点
                MText tpAnno = new MText();
                tpAnno.Contents = "梁顶缘线";
                //文字高度
                tpAnno.TextHeight = 3 * scale;
                //样式为当前样式
                tpAnno.TextStyleId = db.Textstyle;
                //对齐位置为右上
                tpAnno.Attachment = AttachmentPoint.BottomLeft;
                //位置为中点以上0.5个单位
                tpAnno.Location = GeTools.PolarPoint(midPtInTp, Math.PI / 2, 0.5 * scale);
                #endregion
                db.AddToModelSpace(dims.ToArray());//添加各类标注
                db.AddToModelSpace(leftDraw, rightDraw, lengthL, lengthR);//添加张拉段线
                db.AddToModelSpace(tpAnno);//添加梁顶缘线标识
                trans.Commit();
            }
        }        
        /// <summary>
        /// 输出单根钢束引伸量
        /// </summary>
        [CommandMethod("DA_DrawingAmounts")]
        public void DrawingAmounts()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            int drwStyle = 0;//张拉方式，0为两端张拉，-1为左端张拉，1为右端张拉
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择钢束线
                PromptEntityOptions tdLineOpt = new PromptEntityOptions("\n选择钢束或[张拉方式(D)/管道偏差系数(K)/摩阻系数(U)/张拉控制应力(C)]");
                tdLineOpt.SetRejectMessage("\n钢束应为多段线");
                tdLineOpt.AddAllowedClass(typeof(Polyline), true);//仅能选择多段线
                tdLineOpt.Keywords.Add("D");
                tdLineOpt.Keywords.Add("K");
                tdLineOpt.Keywords.Add("U");
                tdLineOpt.Keywords.Add("C");
                tdLineOpt.AppendKeywordsToMessage = false;//提示信息中不显示关键字
                for (;;)//无限循环，直到选中钢束线为止
                {
                    PromptEntityResult tdLineRes = ed.GetEntity(tdLineOpt);
                    //2.各关键字下分别设置钢束张拉参数
                    if (tdLineRes.Status == PromptStatus.Keyword)
                    {
                        switch (tdLineRes.StringResult)
                        {
                            case "D":
                                PromptIntegerOptions drwOpt = new PromptIntegerOptions($"\n输入张拉方式[两端张拉(0)/左端张拉[-1]/右端张拉[1]<{drwStyle}>");
                                drwOpt.AllowNone = true;
                                PromptIntegerResult drwRes = ed.GetInteger(drwOpt);
                                if (drwRes.Value == 0)
                                {
                                    drwStyle = 0;
                                }
                                else if (drwRes.Value == -1)
                                {
                                    drwStyle = -1;
                                }
                                else if (drwRes.Value == 1)
                                {
                                    drwStyle = 1;
                                }
                                break;
                            case "K":
                                PromptDoubleOptions kiiOpt = new PromptDoubleOptions($"\n设置管道偏差系数(1/m)<{kii}>");
                                kiiOpt.AllowNone = true;
                                kiiOpt.AllowNegative = false;
                                kiiOpt.AllowZero = false;
                                PromptDoubleResult kiiRes = ed.GetDouble(kiiOpt);
                                if (kiiRes.Status == PromptStatus.OK)
                                {
                                    kii = kiiRes.Value;
                                }
                                break;
                            case "U":
                                PromptDoubleOptions miuOpt = new PromptDoubleOptions($"\n设置摩阻系数<{miu}>");
                                miuOpt.AllowNone = true;
                                miuOpt.AllowNegative = false;
                                miuOpt.AllowZero = false;
                                PromptDoubleResult miuRes = ed.GetDouble(miuOpt);
                                if (miuRes.Status == PromptStatus.OK)
                                {
                                    miu = miuRes.Value;
                                }
                                break;
                            case "C":
                                PromptDoubleOptions ctrOpt = new PromptDoubleOptions($"\n设置张拉控制应力(MPa)<{ctrlStress}>");
                                ctrOpt.AllowNone = true;
                                PromptDoubleResult ctrRes = ed.GetDouble(ctrOpt);
                                if (ctrRes.Status == PromptStatus.OK)
                                {
                                    ctrlStress = ctrRes.Value;
                                }
                                break;
                        }

                    }
                    //3.输出引伸量
                    else if (tdLineRes.Status == PromptStatus.OK)
                    {
                        ObjectId tdLineId = tdLineRes.ObjectId;
                        Polyline tdLine = trans.GetObject(tdLineId, OpenMode.ForRead) as Polyline;
                        if (drwStyle == 0)//两端张拉
                        {
                            double[] drawAmounts = tdLine.BothDrawAmount(ctrlStress, kii, miu, Ep);
                            ed.WriteMessage("\n左侧引伸量：" + drawAmounts[0].ToString("F0") + "; " +
                                "右侧引伸量：" + drawAmounts[1].ToString("F0") + "。");
                        }
                        else if (drwStyle == -1)//左侧张拉
                        {
                            double drawAmount = tdLine.SingleDrawAmount(ctrlStress, kii, miu, -1, Ep);
                            ed.WriteMessage("\n左侧引伸量：" + drawAmount.ToString("F0") + "。");
                        }
                        else if (drwStyle == 1)//右侧张拉
                        {
                            double drawAmount = tdLine.SingleDrawAmount(ctrlStress, kii, miu, 1, Ep);
                            ed.WriteMessage("\n右侧引伸量：" + drawAmount.ToString("F0") + "。");
                        }
                        break;
                    }
                    else
                    {
                        ed.WriteMessage("输入有误！");
                        return;
                    }
                }
                trans.Commit();//执行事务处理
            }
        }
        /// <summary>
        /// 操作和输出钢束表
        /// </summary>
        [CommandMethod("DA_TendonTable")]
        public void TendonTable()
        {
            //创建钢束信息界面
            TendonInfo tdInfo = new TendonInfo();            
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            db.SetDefaultOverallParams();////设置默认总体参数，已有总体参数字典项则无动作
            tdInfo.ReadNamedDicToDlg(db);            
            //显示钢束信息界面
            Application.ShowModalDialog(tdInfo);       
        }
        /// <summary>
        /// 显示存储在钢束多段线Xrecord内的钢束信息
        /// </summary>
        [CommandMethod("DA_ShowTendonInfo")]
        public void ShowTendonInfo()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //1.选择钢束线
                PromptEntityOptions tdLineOpt = new PromptEntityOptions("\n选择钢束");
                tdLineOpt.SetRejectMessage("\n钢束应为多段线");
                tdLineOpt.AddAllowedClass(typeof(Polyline), true);//仅能选择多段线      
                PromptEntityResult tdLineRes = ed.GetEntity(tdLineOpt);
                if (tdLineRes.Status != PromptStatus.OK) return;
                ObjectId tdId = tdLineRes.ObjectId;
                Polyline td = trans.GetObject(tdId, OpenMode.ForRead) as Polyline;
                //2.获取钢束线的Xrecord
                string info = "";
                #region 1.钢束名称
                if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdName") == null)
                {
                    info += "无钢束名称信息;";
                }
                else//如果存在该键值，采用Xrecord中记录的信息
                {
                    string tdName = (string)td.ObjectId.GetXrecord("tdName")[0].Value;
                    info += "钢束名称：" + tdName + ";";
                }
                #endregion
                #region 2.钢束规格
                if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdStyle") == null)
                {
                    info += "\n无钢束规格信息;";
                }
                else//如果存在该键值，采用Xrecord中记录的信息
                {
                    string tdStyle = (string)td.ObjectId.GetXrecord("tdStyle")[0].Value;
                    info += "\n钢束规格：" + tdStyle + ";";
                }
                #endregion
                #region 3.钢束根数
                if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdNum") == null)
                {
                    info += "\n无钢束根数信息;";
                }
                else//如果存在该键值，采用Xrecord中记录的信息
                {
                    Int16 tdNum = (Int16)td.ObjectId.GetXrecord("tdNum")[0].Value;
                    info += "\n钢束根数：" + tdNum.ToString("F0") + ";";
                }
                #endregion
                #region 4.管道直径
                if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdPipeDia") == null)
                {
                    info += "\n无管道直径信息;";
                }
                else//如果存在该键值，采用Xrecord中记录的信息
                {
                    double tdPipeDia = (double)td.ObjectId.GetXrecord("tdPipeDia")[0].Value;
                    info += "\n管道直径：" + tdPipeDia.ToString("F0") + " mm;";
                }
                #endregion
                #region 5.张拉方式
                if (td.ExtensionDictionary.IsNull || td.ObjectId.GetXrecord("tdDrawStyle") == null)
                {
                    info += "\n无张拉方式信息;";
                }
                else//如果存在该键值，采用Xrecord中记录的信息    
                {
                    int tdDrawStyle = (Int16)td.ObjectId.GetXrecord("tdDrawStyle")[0].Value;
                    switch (tdDrawStyle)
                    {
                        case -1://左侧张拉
                            info += "\n张拉方式：" + "左侧张拉" + ";";
                            break;
                        case 0://两侧张拉
                            info += "\n张拉方式：" + "两端张拉" + ";";
                            break;
                        case 1://右侧张拉
                            info += "\n张拉方式：" + "右侧张拉" + ";";
                            break;
                    }
                }
                #endregion
                Application.ShowAlertDialog(info);
                trans.Commit();
            }
        }
        [CommandMethod("DA_ShowGeneralTdInfo")]
        public void ShowGeneralTdInfo()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                string info = "";
                DBDictionary dicts = db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite) as DBDictionary;
                if (!dicts.Contains("DA_Tendons"))//如果字典中不含DA_Tendons的字典项
                {
                    info += "图形数据库中尚未加入钢束总体信息字典!默认值为："
                        + "\n  -管道偏差系数：kii = 0.0015 1/m;"
                        + "\n  -摩阻系数：miu = 0.16;"
                        + "\n  -钢束弹性模量：Ep = 1.95E5 MPa;"
                        + "\n  -张拉控制应力: ctrlStress = 1395 MPa;"
                        + "\n  -工作长度：workLen = 800 mm;";
                }
                else
                {
                    ObjectId tdsDictId = dicts.GetAt("DA_Tendons");
                    DBDictionary tdsDict = tdsDictId.GetObject(OpenMode.ForWrite) as DBDictionary; //获取DA_Tendons字典
                    info += "钢束总体信息如下：";
                    //管道偏差系数
                    ObjectId xrecId = tdsDict.GetAt("kii");
                    Xrecord xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    TypedValueList vls = xrec.Data;
                    info += "\n  -管道偏差系数：kii = " + vls[0].Value.ToString() + " 1/m;";
                    //摩阻系数
                    xrecId = tdsDict.GetAt("miu");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    info += "\n  -摩阻系数：miu = " + vls[0].Value.ToString()  +";";
                    //钢束弹性模量
                    xrecId = tdsDict.GetAt("Ep");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    info += "\n  -钢束弹性模量：Ep = " + vls[0].Value.ToString() + " MPa;";
                    //张拉控制应力
                    xrecId = tdsDict.GetAt("ctrlStress");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    info += "\n  -张拉控制应力: ctrlStress = " + vls[0].Value.ToString() + " MPa;";
                    //张拉端工作长度
                    xrecId = tdsDict.GetAt("workLen");
                    xrec = xrecId.GetObject(OpenMode.ForRead) as Xrecord;
                    vls = xrec.Data;
                    info += "\n  -工作长度：workLen = " + vls[0].Value.ToString() + " mm;";
                }
                Application.ShowAlertDialog(info);
                trans.Commit();
            }
        }

    }
}
