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


namespace DA_PolylineTools
{
    /// <summary>
    /// 多段线操作类
    /// </summary>
    public class DA_Polylinetools
    {
        /// <summary>
        /// 用多段直线拟合空间曲线，空间曲线由平面多段线和立面多段线表示。
        /// 该命令利用平面轴线和立面轴线生成完整空间曲线，方便空间建模等工作。
        /// 需要注意平面图中曲线的自然坐标s与立面图中的横坐标x一致。
        /// </summary>
        [CommandMethod("DA_LineSegs3d")]
        public void LineSegs3d()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择空间轴线的平面视图多段线
                PromptEntityOptions plxyOpt = new PromptEntityOptions("\n选择空间轴线的平面视图多段线");
                plxyOpt.SetRejectMessage("\n并非多段线！");
                plxyOpt.AddAllowedClass(typeof(Polyline), true);//仅能选择多段线      
                PromptEntityResult plxyRes = ed.GetEntity(plxyOpt);
                if (plxyRes.Status != PromptStatus.OK) return;
                ObjectId plxyId = plxyRes.ObjectId;
                Polyline plxy = trans.GetObject(plxyId, OpenMode.ForWrite) as Polyline;

                //2.选择空间轴线的立面视图多段线
                PromptEntityOptions plxzOpt = new PromptEntityOptions("\n选择空间轴线的立面视图多段线");
                plxzOpt.SetRejectMessage("\n并非多段线！");
                plxzOpt.AddAllowedClass(typeof(Polyline), true);//仅能选择多段线      
                PromptEntityResult plxzRes = ed.GetEntity(plxzOpt);
                if (plxzRes.Status != PromptStatus.OK) return;
                ObjectId plxzId = plxzRes.ObjectId;
                Polyline plxz = trans.GetObject(plxzId, OpenMode.ForWrite) as Polyline;

                //3.判断平面视图和立面视图中的长度是否一致
                double lenXY = plxy.Length;
                double lenXZ = Math.Abs(plxz.StartPoint.X - plxz.EndPoint.X);
                if (Math.Abs(lenXY- lenXZ) >10)
                {
                    ed.WriteMessage("\n轴线平面长度与立面长度不一致，请检查！");
                    return;
                }
                double totalLen = lenXY;//记录轴线总长

                //4.输入拟合直线段的长度
                PromptDoubleOptions slOpt = new PromptDoubleOptions("\n输入拟合直线段的长度<1000>");
                slOpt.AllowNegative = false;//不允许负值
                slOpt.AllowZero = false;//不允许零值
                slOpt.DefaultValue = 1000;//默认值为1000
                PromptDoubleResult slRes = ed.GetDouble(slOpt);
                if (slRes.Status != PromptStatus.OK) return;
                double segLen = slRes.Value;//或得拟合线段长度
                if(segLen > (totalLen-10))//节段过长（仅比总长小1cm或更长）
                {
                    ed.WriteMessage("\n节段长度过长！");
                    return;
                }

                //5.获取每个节段点的坐标
                //节段需要包括所有平面曲线节点、立面曲线节点和节段长度端点
                List<Point3d> verts = new List<Point3d>();//初始化节段点List
                //5.1 添加平面曲线节点
                for(int i = 0; i < plxy.NumberOfVertices; i++)
                {
                    Point3d vertInPlxy = plxy.GetPoint3dAt(i);//平面图节点
                    double prmt = plxy.GetParameterAtPoint(vertInPlxy);//该节点参数
                    double distance = plxy.GetDistanceAtParameter(prmt);//该节点到起点距离，即为立面图上到起点的横坐标差
                    Point3d xlBase = plxz.StartPoint + new Vector3d(distance, 0, 0);//立面图起点偏移distance后，作为Xline基点
                    Xline xl = new Xline();//新建xline
                    xl.BasePoint = xlBase;//基点如前所述
                    xl.UnitDir = new Vector3d(0, 1, 0);//竖直方向
                    Point3dCollection ptIntersects = new Point3dCollection();//建立交点组
                    plxz.IntersectWith(xl, Intersect.OnBothOperands, ptIntersects, IntPtr.Zero, IntPtr.Zero);//与立面轴线相交
                    Point3d vertInPlxz = ptIntersects[0];//获得交点
                    //将节点添加至节点列表，X,Y坐标由平面图确定，Z坐标由立面图确定
                    Point3d vert = new Point3d(vertInPlxy.X, vertInPlxy.Y, vertInPlxz.Y-plxz.StartPoint.Y);
                    verts.Add(vert);
                    xl.Dispose();
                }
                //5.2 添加立面曲线节点
                for (int i = 1; i < plxz.NumberOfVertices-1; i++)
                {
                    Point3d vertInPlxz = plxz.GetPoint3dAt(i);//立面图节点
                    double distance = vertInPlxz.X - plxz.StartPoint.X;//该节点到起点距离，即为立面图上到起点的横坐标差
                    Point3d vertInPlxy = plxy.GetPointAtDist(distance);//获取该点在平面图上的对应点
                    //将节点添加至节点列表，X,Y坐标由平面图确定，Z坐标由立面图确定
                    Point3d vert = new Point3d(vertInPlxy.X, vertInPlxy.Y, vertInPlxz.Y - plxz.StartPoint.Y);
                    if(!IsPtInList(vert,verts,10))//如果该点尚不在点群中
                        verts.Add(vert);
                }
                //5.3 节段端点
                double dist = segLen;
                do
                {
                    Point3d vertInPlxy = plxy.GetPointAtDist(dist);//平面图节点
                    //立面图节点
                    Point3d xlBase = plxz.StartPoint + new Vector3d(dist, 0, 0);//立面图起点偏移dist后，作为Xline基点
                    Xline xl = new Xline();//新建xline
                    xl.BasePoint = xlBase;//基点如前所述
                    xl.UnitDir = new Vector3d(0, 1, 0);//竖直方向
                    Point3dCollection ptIntersects = new Point3dCollection();//建立交点组
                    plxz.IntersectWith(xl, Intersect.OnBothOperands, ptIntersects, IntPtr.Zero, IntPtr.Zero);//与立面轴线相交
                    Point3d vertInPlxz = ptIntersects[0];//获得交点
                    //将节点添加至节点列表，X,Y坐标由平面图确定，Z坐标由立面图确定
                    Point3d vert = new Point3d(vertInPlxy.X, vertInPlxy.Y, vertInPlxz.Y - plxz.StartPoint.Y);
                    if (!IsPtInList(vert, verts, 10))//如果该点尚不在点群中
                        verts.Add(vert);
                    dist += segLen;
                }
                while (dist < totalLen - 10) ;//前进长度小于总长度

                //6.对所有节点进行排序
                var sortedPts = (from pt in verts
                                let p = plxy.GetParameterAtPoint(pt)
                                let d = plxy.GetDistanceAtParameter(p)
                                orderby d
                                select pt).ToList();

                //7.指定插入点
                Point3d insertPt = new Point3d();
                for(;;)
                {
                    PromptPointOptions ptOpt = new PromptPointOptions("\n指定空间拟合段插入点");
                    PromptPointResult ptRes = ed.GetPoint(ptOpt);
                    if(ptRes.Status == PromptStatus.OK)
                    {
                        insertPt = ptRes.Value;
                        break;
                    }
                }
                Vector3d vect = insertPt - plxy.StartPoint;//建立平移向量，从曲线平面图起点至选择的插入点
                Matrix3d mt = Matrix3d.Displacement(vect);//建立对应的变换矩阵
                
                //7.依次建立线段
                for(int i = 0; i < sortedPts.Count-1; i++)
                {
                    Line line = new Line(sortedPts[i], sortedPts[i + 1]);
                    line.TransformBy(mt);//平移到位
                    db.AddToModelSpace(line);//添加至模型空间
                }

                trans.Commit();//执行事务处理
            }
            

        }

        /// <summary>
        /// 将多段线按用户输入的距离节线化，方便导入有限元模型
        /// </summary>
        [CommandMethod("DA_LineSegs2d")]
        public void LineSegs2d()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Polyline pline = new Polyline();//初始化钢束线
            Point3d startPt = new Point3d();//初始化起点
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择空间多段线
                PromptEntityOptions plineOpt = new PromptEntityOptions("\n选择多段线");
                plineOpt.SetRejectMessage("\n并非多段线！");
                plineOpt.AddAllowedClass(typeof(Polyline), true);//仅能选择多段线      
                PromptEntityResult plineRes = ed.GetEntity(plineOpt);
                if (plineRes.Status != PromptStatus.OK) return;
                ObjectId plineId = plineRes.ObjectId;
                pline = trans.GetObject(plineId, OpenMode.ForWrite) as Polyline;
                //2.选择起始点
                PromptPointOptions startPtOpt = new PromptPointOptions("\n选择曲线上的点作为起点");
                PromptPointResult startPtRes = ed.GetPoint(startPtOpt);
                if (startPtRes.Status != PromptStatus.OK) return;
                startPt = pline.GetClosestPointTo(startPtRes.Value, false);//选择点到多段线的最近点  
                trans.Commit();//执行事务处理
            }
            //3.输入分段距离并节线化
            Point3d newStartPt = startPt;//初始化每次绘制起点
            double distOfStPt = pline.GetDistanceToStartPt(startPt);//获取起点距曲线起点的距离
            for (;;)
            {
                PromptStringOptions segsOpt = new PromptStringOptions("\n输入分段距离或按ESC退出（各段长度以空格分隔，等间距采用N@d格式）");
                segsOpt.AllowSpaces = true;//允许输入空格
                PromptResult segsRes = ed.GetString(segsOpt);
                if (segsRes.Status == PromptStatus.OK)//如果输入正确且不为回车
                {
                    string input = segsRes.StringResult;//获得输入字符串
                    List<double> segLengths = new List<double>();//初始化节段长List
                    if (ReadSegsInput(ref segLengths, input))//读入距离数据成功
                    {
                        distOfStPt += segLengths.Sum();//计算累计距离
                        if (distOfStPt > pline.Length)//累计距离已超过多段线长
                        {
                            ed.WriteMessage("\n输入间距值已超出多段线范围，请重新输入！");
                            continue;
                        }
                        CreateLineSegs(pline, newStartPt, segLengths);//绘制直线段
                        newStartPt = pline.GetPointAtDist(distOfStPt);
                    }
                    else//读入距离数据有误
                    {
                        ed.WriteMessage("\n输入有误，请重新输入！");
                        continue;
                    }
                }
                else break;//输入回车、ESC等则退出
            }
        }

        /// <summary>
        /// 将两点间的多段线段等分后节线化，方便导入有限元模型
        /// </summary>
        [CommandMethod("DA_EqualLineSegs2d")]
        public void EqualLineSegs2d()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择空间多段线
                PromptEntityOptions plineOpt = new PromptEntityOptions("\n选择多段线");
                plineOpt.SetRejectMessage("\n并非多段线！");
                plineOpt.AddAllowedClass(typeof(Polyline), true);//仅能选择多段线      
                PromptEntityResult plineRes = ed.GetEntity(plineOpt);
                if (plineRes.Status != PromptStatus.OK) return;
                ObjectId plineId = plineRes.ObjectId;
                Polyline pline = trans.GetObject(plineId, OpenMode.ForWrite) as Polyline;
                //2.选择起点
                PromptPointOptions startPtOpt = new PromptPointOptions("\n选择曲线上的点作为起点");
                PromptPointResult startPtRes = ed.GetPoint(startPtOpt);
                if (startPtRes.Status != PromptStatus.OK) return;
                Point3d startPt = pline.GetClosestPointTo(startPtRes.Value, false);//选择点到多段线的最近点  
                //3.选择终点
                PromptPointOptions endPtOpt = new PromptPointOptions("\n选择曲线上的点作为终点");
                PromptPointResult endPtRes = ed.GetPoint(endPtOpt);
                if (endPtRes.Status != PromptStatus.OK) return;
                Point3d endPt = pline.GetClosestPointTo(endPtRes.Value, false);//选择点到多段线的最近点  
                //4.输入分段数
                PromptIntegerOptions intOpt = new PromptIntegerOptions("\n输入分段数");
                intOpt.AllowNegative = false;//不予许负值
                intOpt.AllowZero = false;//不允许0
                PromptIntegerResult intRes = ed.GetInteger(intOpt);
                if(intRes.Status == PromptStatus.OK)//输入成功
                {
                    int nSegs = intRes.Value;
                    double distOfStPt = pline.GetDistanceToStartPt(startPt);//获取起点距曲线起点的距离
                    double distOfEdPt = pline.GetDistanceToStartPt(endPt);//获取终点距曲线起点的距离
                    List<double> segLengths = new List<double>();
                    for (int i = 1; i <= nSegs; i++) segLengths.Add((distOfEdPt - distOfStPt) / nSegs);
                    CreateLineSegs(pline, startPt, segLengths);//绘制直线段
                }
                trans.Commit();//执行事务处理
            }
        }

        /// <summary>
        /// 逆转曲线方向
        /// </summary>
        [CommandMethod("DA_PolylineReverse")]
        public void PolylineReverse()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择需要逆转的曲线
                PromptEntityOptions cvOpt = new PromptEntityOptions("\n选择需要逆转方向的曲线");
                cvOpt.SetRejectMessage("\n并非曲线对象！");
                cvOpt.AddAllowedClass(typeof(Curve), true);//仅能选择曲线对象      
                PromptEntityResult cvRes = ed.GetEntity(cvOpt);
                if (cvRes.Status != PromptStatus.OK) return;
                ObjectId cvId = cvRes.ObjectId;
                Curve cv = trans.GetObject(cvId, OpenMode.ForWrite) as Curve;

                //2.逆转曲线
                cv.ReverseCurve();
                cv.DowngradeOpen();//安全起见，将曲线降为读模式

                trans.Commit();//执行事务处理
            }
        }
        
        /// <summary>
        /// 连续获得多段线上从某点起沿曲线一定距离处的点
        /// 选择起点时，如果所选点在曲线外，则默认取曲线上最靠近该点的点
        /// “沿曲线距离”系指沿曲线正向距离，若要获得反向距离点，输入负值或反转曲线（DA_PolylineReverse）
        /// </summary>
        private double scale_GPAD = 1;//初始化本命令中的绘制比例
        private string isDistDenote = "Y";//初始化是否标注沿曲线距离的关键字
        [CommandMethod("DA_GetPointsAtDists")]
        public void GetPointsAtDists()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Polyline pline = new Polyline();//初始化多段线
            Point3d startPt = new Point3d();//初始化起点
            double distOfStPt = 0;//初始化起点距
            //初始化各标注图元
            Vector3d drvt = new Vector3d();//标注点切线
            double len = 10 * scale_GPAD;//标注点长度，为10单位
            Point3d pt1 = new Point3d();//标准线起点
            Point3d pt2 = new Point3d();//标准线终点
            Line line = new Line();//标注线
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择多段线
                PromptEntityOptions plineOpt = new PromptEntityOptions("\n选择多段线");
                plineOpt.SetRejectMessage("\n并非多段线！");
                plineOpt.AddAllowedClass(typeof(Polyline), true);
                PromptEntityResult plineRes = ed.GetEntity(plineOpt);
                if (plineRes.Status != PromptStatus.OK) return;
                pline = plineRes.ObjectId.GetObject(OpenMode.ForRead) as Polyline;

                //2.选择绘图比例
                PromptDoubleOptions scaleOpt = new PromptDoubleOptions($"\n设置绘图比例<{scale_GPAD}>");
                scaleOpt.AllowNone = true;//允许回车，则采用默认比例
                scaleOpt.AllowNegative = false;//不允许负值
                scaleOpt.AllowZero = false;//不允许零值
                PromptDoubleResult scaleRes = ed.GetDouble(scaleOpt);//获取比例
                if (scaleRes.Status == PromptStatus.OK)
                {
                    scale_GPAD = scaleRes.Value;//获取绘图比例
                    len = 10 * scale_GPAD;//重设标注线长度
                }
                
                //3.选择起点
                PromptPointOptions startPtOpt = new PromptPointOptions("\n选择曲线上的点作为起点");
                PromptPointResult startPtRes = ed.GetPoint(startPtOpt);
                if (startPtRes.Status != PromptStatus.OK) return;
                startPt = pline.GetClosestPointTo(startPtRes.Value, false);//选择点到多段线的最近点
                distOfStPt = pline.GetDistanceToStartPt(startPt);//获取起点距曲线起点的距离
                //绘制起点线
                drvt = pline.GetFirstDerivative(startPt);//获取切线
                pt1 = startPt + len / 2 * drvt.GetUnitVector().RotateBy(Math.PI / 2, Vector3d.ZAxis);
                pt2 = startPt - len / 2 * drvt.GetUnitVector().RotateBy(Math.PI / 2, Vector3d.ZAxis);
                line = new Line(pt1, pt2);
                db.AddToModelSpace(line);

                //4.是否标注距离
                PromptKeywordOptions kwOpt = new PromptKeywordOptions($"\n是否标注沿曲线长度[Y/N]<{isDistDenote}>");
                kwOpt.Keywords.Add("Y");
                kwOpt.Keywords.Add("N");
                kwOpt.AllowNone = true;//可回车
                kwOpt.AllowArbitraryInput = false;//不可随意输入
                kwOpt.AppendKeywordsToMessage = false;//提示信息中不显示关键字
                PromptResult kwRes = ed.GetKeywords(kwOpt);
                if (kwRes.Status == PromptStatus.OK) isDistDenote = kwRes.StringResult;

                trans.Commit();//执行事务处理
            }

            //5.输入沿曲线距离
            //无限循环
            double distCum = distOfStPt;//初始化累计长度
            Point3d newStartPt = startPt;//初始化当前前进起始点
            for (;;)
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
                {
                    PromptStringOptions distsOpt = new PromptStringOptions("\n输入分段距离或按ESC退出（各段长度以空格分隔，等间距采用N@d格式）");
                    distsOpt.AllowSpaces = true;//允许输入空格
                    PromptResult distsRes = ed.GetString(distsOpt);
                    if (distsRes.Status == PromptStatus.OK)//输入正确
                    {
                        List<double> dists = new List<double>();
                        if (ReadSegsInput(ref dists, distsRes.StringResult))//读入距离数据成功
                        {
                            if (distCum + dists.Sum() > pline.Length)
                            {
                                ed.WriteMessage("超出多段线范围,请重新输入！");
                                trans.Abort();
                                continue;
                            }

                            CreateSpanLines(pline, newStartPt, dists, isDistDenote, scale_GPAD);
                            distCum += dists.Sum();//重置前进距离
                            newStartPt = pline.GetPointAtDist(distCum);//重置当前前进起始点
                            trans.Commit();//执行事务处理
                        }
                        else//读入距离数据有误
                        {
                            ed.WriteMessage("\n输入有误，请重新输入！");
                            trans.Abort();
                            continue;
                        }
                    }
                    else         
                    {
                        trans.Abort();
                        break;//输入回车、ESC等则退出         
                    }
                }
            }
        }

        /// <summary>
        /// 获得多段线上两点间等分后的跨度线
        /// </summary>
        [CommandMethod("DA_GetPointsAtEqualDists")]
        public void GetPointsAtEqualDists()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            //初始化各标注图元
            Vector3d drvt = new Vector3d();//标注点切线
            double len = 10 * scale_GPAD;//标注点长度，为10单位
            Point3d pt1 = new Point3d();//标准线起点
            Point3d pt2 = new Point3d();//标准线终点
            Line line = new Line();//标注线
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择多段线
                PromptEntityOptions plineOpt = new PromptEntityOptions("\n选择多段线");
                plineOpt.SetRejectMessage("\n并非多段线！");
                plineOpt.AddAllowedClass(typeof(Polyline), true);
                PromptEntityResult plineRes = ed.GetEntity(plineOpt);
                if (plineRes.Status != PromptStatus.OK) return;
                Polyline pline = plineRes.ObjectId.GetObject(OpenMode.ForRead) as Polyline;

                //2.选择绘图比例
                PromptDoubleOptions scaleOpt = new PromptDoubleOptions($"\n设置绘图比例<{scale_GPAD}>");
                scaleOpt.AllowNone = true;//允许回车，则采用默认比例
                scaleOpt.AllowNegative = false;//不允许负值
                scaleOpt.AllowZero = false;//不允许零值
                PromptDoubleResult scaleRes = ed.GetDouble(scaleOpt);//获取比例
                if (scaleRes.Status == PromptStatus.OK)
                {
                    scale_GPAD = scaleRes.Value;//获取绘图比例
                    len = 10 * scale_GPAD;//重设标注线长度
                }

                //3.选择起点
                PromptPointOptions startPtOpt = new PromptPointOptions("\n选择曲线上的点作为起点");
                PromptPointResult startPtRes = ed.GetPoint(startPtOpt);
                if (startPtRes.Status != PromptStatus.OK) return;
                Point3d startPt = pline.GetClosestPointTo(startPtRes.Value, false);//选择点到多段线的最近点
                
                //4.选择终点
                PromptPointOptions endPtOpt = new PromptPointOptions("\n选择曲线上的点作为终点");
                PromptPointResult endPtRes = ed.GetPoint(endPtOpt);
                if (endPtRes.Status != PromptStatus.OK) return;
                Point3d endPt = pline.GetClosestPointTo(endPtRes.Value, false);//选择点到多段线的最近点
                
                //5.是否标注距离
                PromptKeywordOptions kwOpt = new PromptKeywordOptions($"\n是否标注沿曲线长度[Y/N]<{isDistDenote}>");
                kwOpt.Keywords.Add("Y");
                kwOpt.Keywords.Add("N");
                kwOpt.AllowNone = true;//可回车
                kwOpt.AllowArbitraryInput = false;//不可随意输入
                kwOpt.AppendKeywordsToMessage = false;//提示信息中不显示关键字
                PromptResult kwRes = ed.GetKeywords(kwOpt);
                if (kwRes.Status == PromptStatus.OK) isDistDenote = kwRes.StringResult;

                //6.输入分段数量
                PromptIntegerOptions intOpt = new PromptIntegerOptions("\n输入分段数");
                intOpt.AllowNegative = false;//不予许负值
                intOpt.AllowZero = false;//不允许0
                PromptIntegerResult intRes = ed.GetInteger(intOpt);
                if (intRes.Status == PromptStatus.OK)//输入成功
                {
                    int nSegs = intRes.Value;

                    Double distOfStPt = pline.GetDistanceToStartPt(startPt);//获取起点距曲线起点的距离
                    //绘制起点线
                    drvt = pline.GetFirstDerivative(startPt);//获取切线
                    pt1 = startPt + len / 2 * drvt.GetUnitVector().RotateBy(Math.PI / 2, Vector3d.ZAxis);
                    pt2 = startPt - len / 2 * drvt.GetUnitVector().RotateBy(Math.PI / 2, Vector3d.ZAxis);
                    line = new Line(pt1, pt2);
                    db.AddToModelSpace(line);

                    double distOfEdPt = pline.GetDistanceToStartPt(endPt);//获取终点距曲线起点的距离
                    //绘制终点线
                    drvt = pline.GetFirstDerivative(endPt);//获取切线
                    pt1 = endPt + len / 2 * drvt.GetUnitVector().RotateBy(Math.PI / 2, Vector3d.ZAxis);
                    pt2 = endPt - len / 2 * drvt.GetUnitVector().RotateBy(Math.PI / 2, Vector3d.ZAxis);
                    line = new Line(pt1, pt2);
                    db.AddToModelSpace(line);

                    
                    List<double> dists = new List<double>();
                    for (int i = 1; i <= nSegs; i++) dists.Add((distOfEdPt - distOfStPt) / nSegs);
                    CreateSpanLines(pline, startPt, dists, isDistDenote, scale_GPAD);//绘制分跨线
                }
                trans.Commit();//执行事务处理
            }
        }

        /// <summary>
        /// 获得多段线在两点间距离
        /// </summary>
        [CommandMethod("DA_DistanceBetweenPts")]
        public void DistanceBetweenPts()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择多段线
                PromptEntityOptions plineOpt = new PromptEntityOptions("\n选择多段线");
                plineOpt.SetRejectMessage("\n并非多段线！");
                plineOpt.AddAllowedClass(typeof(Polyline), true);
                PromptEntityResult plineRes = ed.GetEntity(plineOpt);
                if (plineRes.Status != PromptStatus.OK) return;
                Polyline pline = plineRes.ObjectId.GetObject(OpenMode.ForRead) as Polyline;

                //2.选择测量距离的两点
                PromptPointOptions startPtOpt = new PromptPointOptions("\n选择曲线上的点作为第一点");
                PromptPointResult startPtRes = ed.GetPoint(startPtOpt);
                if (startPtRes.Status != PromptStatus.OK) return;
                Point3d startPt = pline.GetClosestPointTo(startPtRes.Value, false);//选择点到多段线的最近点
                double distOfStPt = pline.GetDistanceToStartPt(startPt);//获取起点距曲线起点的距离

                PromptPointOptions endPtOpt = new PromptPointOptions("\n选择曲线上的点作为第二点");
                PromptPointResult endPtRes = ed.GetPoint(endPtOpt);
                if (endPtRes.Status != PromptStatus.OK) return;
                Point3d endPt = pline.GetClosestPointTo(endPtRes.Value, false);//选择点到多段线的最近点
                double distOfEdPt = pline.GetDistanceToStartPt(endPt);//获取起点距曲线起点的距离

                double dist = distOfEdPt - distOfStPt;
                ed.WriteMessage("\n沿曲线距离为" + dist.ToString("F4"));

                trans.Commit();//执行事务处理
            }
        }
        
        /// <summary>
        /// 判断某个点是否已在点群中
        /// </summary>
        /// <param name="pt">判断点</param>
        /// <param name="list">点群列表</param>
        /// <param name="tol">距离容差</param>
        /// <returns>是否在点群中</returns>
        private bool IsPtInList(Point3d pt,List<Point3d> list,double tol)
        {
            for(int i = 0; i < list.Count; i++)
            {
                //现在理解才鸟为什么采用下述判断格式而非求解距离：
                //因为&&运算采用“有解即离”模式，点群数量较多时，下述判断方式明显运算量会小于各个求解距离
                if (Math.Abs(pt.X - list[i].X) < tol
                    && Math.Abs(pt.Y - list[i].Y) < tol
                    && Math.Abs(pt.Z - list[i].Z) < tol)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 将输入的字符串转换为节段长度数组，输入格式为"15.9 4@20.3"
        /// </summary>
        /// <param name="segLengths">存储节段长度</param>
        /// <param name="input">输入的字符串</param>
        /// <returns></returns>
        private bool ReadSegsInput(ref List<double> segLengths, string input)
        {
            segLengths = new List<double>();//初始化节段长度List
            string[] segArray = input.Split(' ');//将输入的字符串按空格分隔成字符串数组
            foreach (string seg in segArray)
            {
                int N = 1;//初始化数量
                double dist = 0;//初始化长度
                if (seg.Contains("@"))//如果含有@,应为多段等间距数据
                {
                    string[] nAndDist = seg.Split('@');//将该字符串以@拆分
                    if (int.TryParse(nAndDist[0], out N) && double.TryParse(nAndDist[1], out dist))
                    {
                        //如果第一个字符串可解析为int值，第二个字符串可解析为double值：
                        //则在segLengths中添加N各dist值
                        for (int i = 1; i <= N; i++) segLengths.Add(dist);                        
                    }
                    else//如果解析失败，说明输入有误
                    {
                        segLengths.Clear();//清空segLengths中的全部元素
                        return false;//返回false
                    }
                }
                else//如果不含@，应为单个数字
                {
                    if(double.TryParse(seg, out dist))//如果解析为double值成功
                    {
                        segLengths.Add(dist);
                    }
                    else//如果解析失败，说明输入有误
                    {
                        segLengths.Clear();//清空segLengths中的全部元素
                        return false;//返回false
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 绘制多段线节线段
        /// </summary>
        /// <param name="pline">多段线</param>
        /// <param name="startPt">起点</param>
        /// <param name="segLengths">各节线端点距离组成的List</param>
        private void CreateLineSegs(Polyline pline, Point3d startPt, List<double> segLengths)
        {
            Database db = pline.Database;
            double startDist = pline.GetDistAtPoint(startPt);//startPoint到曲线起点的长度
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                Point3d p1 = startPt;//初始化线段第一点
                Point3d p2 = new Point3d();//初始化线段第二点
                double distAcum = startDist;//初始化累计距离
                foreach (double segLen in segLengths)
                {
                    distAcum += segLen;//累计距离前进
                    p2 = pline.GetPointAtDist(distAcum);//获得线段第二点
                    Line line = new Line(p1, p2);//建立直线
                    db.AddToModelSpace(line);//将直线加入数据库
                    p1 = p2;//更新线段起点
                }
                trans.Commit();//执行事务处理
            }
        }

        /// <summary>
        /// 按照距离绘制分段线并依据用户选择进行跨度标注
        /// </summary>
        /// <param name="pline">多段线</param>
        /// <param name="startPt">起点</param>
        /// <param name="dists">沿曲线长度组成的List</param>
        /// <param name="yOrN">是否标注跨度</param>
        /// <param name="scale">标注比例</param>
        private void CreateSpanLines(Polyline pline, Point3d startPt, List<double> dists, string yOrN, double scale)
        {
            Database db = pline.Database;
            double distCum = pline.GetDistAtPoint(startPt);//startPoint到曲线起点的长度
            double len = 10 * scale;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                foreach (double dist in dists)
                {
                    distCum += dist;//累积距离前进
                    Point3d destPt = pline.GetPointAtDist(distCum);
                    Vector3d drvt = pline.GetFirstDerivative(destPt);//获取切线
                    Point3d pt1 = destPt + len / 2 * drvt.GetUnitVector().RotateBy(Math.PI / 2, Vector3d.ZAxis);
                    Point3d pt2 = destPt - len / 2 * drvt.GetUnitVector().RotateBy(Math.PI / 2, Vector3d.ZAxis);
                    Line line = new Line(pt1, pt2);//绘制分段线
                    db.AddToModelSpace(line);
                    if (yOrN == "Y")//如果选择标注沿曲线长度
                    {
                        //在分段线旁标注距离
                        MText txt = new MText();
                        txt.SetTextStyle(
                            contents: dist.ToString("F3"),
                            textHeight: 3 * scale,
                            textStyleId: db.Textstyle,
                            attachment: AttachmentPoint.BottomCenter,
                            rotation: drvt.GetAngleTo(Vector3d.XAxis),
                            location: pline.GetPointAtDist(distCum - dist / 2) + 2 * scale * drvt.GetUnitVector().RotateBy(Math.PI / 2, Vector3d.ZAxis)
                        );
                        db.AddToModelSpace(txt);
                    }
                }
                trans.Commit();
            }
        }
    }
}
