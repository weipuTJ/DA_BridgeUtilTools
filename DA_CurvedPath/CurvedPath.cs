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

namespace DA_CurvedPath
{
    public class CurvedPath
    {
        /// <summary>
        /// 图元沿曲线复制命令
        /// 操作顺序：选择图元、选择路径、选择起点、终点（起点终点未选择在曲线上时，取其到曲线最近的点）
        /// 复制后图元位置从起点移至终点、且图元与起终点位置曲线切线的夹角保持不变
        /// </summary>
        [CommandMethod("DA_CopyAlongPath")]
        public void CopyAlongPath()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择要复制的图元
                PromptSelectionOptions entsOpt = new PromptSelectionOptions();
                entsOpt.MessageForAdding = "\n选择要复制的图元";
                PromptSelectionResult entsRes = ed.GetSelection(entsOpt);
                if (entsRes.Status != PromptStatus.OK) return;
                SelectionSet entSS = entsRes.Value;
                ObjectId[] entIds = entSS.GetObjectIds();
                //2.选择路径线
                PromptEntityOptions pathOpt = new PromptEntityOptions("\n选择复制路径");
                pathOpt.SetRejectMessage("\n请选择多段线、圆弧或直线！");
                pathOpt.AddAllowedClass(typeof(Polyline), false);
                pathOpt.AddAllowedClass(typeof(Arc), false);
                pathOpt.AddAllowedClass(typeof(Line), false);
                PromptEntityResult pathRes = ed.GetEntity(pathOpt);
                ObjectId pathId = pathRes.ObjectId;
                Curve pathCurve = trans.GetObject(pathId,OpenMode.ForRead) as Curve;
                //3.选择复制的起终点
                //起点
                PromptPointOptions pt1Opt = new PromptPointOptions("\n选择起点");
                PromptPointResult pt1Res = ed.GetPoint(pt1Opt);
                if (pt1Res.Status != PromptStatus.OK) return;
                Point3d pt1 = pt1Res.Value;
                pt1 = pathCurve.GetClosestPointTo(pt1,false);  //使用GetClosedPointTo函数保证起点严格在曲线上
                //终点
                PromptPointOptions pt2Opt = new PromptPointOptions("\n选择终点");
                PromptPointResult pt2Res = ed.GetPoint(pt2Opt);
                if (pt1Res.Status != PromptStatus.OK) return;
                Point3d pt2 = pt2Res.Value;
                pt2 = pathCurve.GetClosestPointTo(pt2, false);  //使用GetClosedPointTo函数保证终点严格在曲线上
                //4.执行复制命令
                //4.1 复制并旋转图元
                Vector3d vct1 = pathCurve.GetFirstDerivative(pt1);  //起点切向量
                double ang1 = Math.Atan2(vct1.Y, vct1.X);  //起点切向量角度
                Vector3d vct2 = pathCurve.GetFirstDerivative(pt2);  //终点切向量
                double ang2 = Math.Atan2(vct2.Y, vct2.X);  //起点切向量角度
                ObjectId newEntId;
                foreach (ObjectId entId in entIds)
                {
                    newEntId = entId.Copy(pt1, pt2);  //复制图元
                    newEntId.Rotate(pt2, ang2 - ang1);  //旋转图元
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 图元沿曲线复制命令
        /// 操作顺序：选择图元、选择路径、选择起点、输入沿路径的复制距离
        /// 复制后图元按照输入的距离复制，且在各个位置与曲线切线夹角保持不变
        /// </summary>
        [CommandMethod("DA_CopyAlongPathAtDists")]
        public void CopyAlongPathAtDists()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            ObjectId[] entIds;  //待复制图元Id
            Curve pathCurve;  //复制路劲曲线
            Point3d pt1;  //复制起点
            using (Transaction trans = db.TransactionManager.StartTransaction())//开始事务处理
            {
                //1.选择要复制的图元
                PromptSelectionOptions entsOpt = new PromptSelectionOptions();
                entsOpt.MessageForAdding = "\n选择要复制的图元";
                PromptSelectionResult entsRes = ed.GetSelection(entsOpt);
                if (entsRes.Status != PromptStatus.OK) return;
                SelectionSet entSS = entsRes.Value;
                entIds = entSS.GetObjectIds();
                //2.选择路径线
                PromptEntityOptions pathOpt = new PromptEntityOptions("\n选择复制路径");
                pathOpt.SetRejectMessage("\n请选择多段线、圆弧或直线！");
                pathOpt.AddAllowedClass(typeof(Polyline), false);
                pathOpt.AddAllowedClass(typeof(Arc), false);
                pathOpt.AddAllowedClass(typeof(Line), false);
                PromptEntityResult pathRes = ed.GetEntity(pathOpt);
                ObjectId pathId = pathRes.ObjectId;
                pathCurve = pathId.GetObject(OpenMode.ForRead) as Curve;
                //3.选择复制起点
                PromptPointOptions pt1Opt = new PromptPointOptions("\n选择起点");
                PromptPointResult pt1Res = ed.GetPoint(pt1Opt);
                if (pt1Res.Status != PromptStatus.OK) return;
                pt1 = pt1Res.Value;
                pt1 = pathCurve.GetClosestPointTo(pt1, false);  //使用GetClosedPointTo函数保证起点严格在曲线上
                trans.Commit();
            }
            //4.输入复制距离并进行复制和旋转
            double distOfStPt = pathCurve.GetDistAtPoint(pt1);  //获取复制起点在曲线上的距离参数
            double distCum = distOfStPt;  //初始化累计长度
            Point3d pt2;  //初始化终点
            ObjectId newEntId;  //初始化复制图元Id
            Vector3d vct1 = pathCurve.GetFirstDerivative(pt1);  //起点切向量
            double ang1 = Math.Atan2(vct1.Y, vct1.X);  //起点切向量角度
            Vector3d vct2;  //初始化终点切向量
            double ang2;  //初始化起点切向量角度
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
                            if (distCum + dists.Sum() > pathCurve.GetDistAtPoint(pathCurve.EndPoint))
                            {
                                ed.WriteMessage("超出多段线范围,请重新输入！");
                                trans.Abort();
                                continue;
                            }
                            foreach(double dist in dists)
                            {
                                distCum += dist;  //终点距离累积前进
                                pt2 = pathCurve.GetPointAtDist(distCum);//重置当前前进起始点
                                foreach(ObjectId entId in entIds)
                                {
                                    newEntId = entId.Copy(pt1, pt2);
                                    vct2 = pathCurve.GetFirstDerivative(pt2);  //终点切向量
                                    ang2 = Math.Atan2(vct2.Y, vct2.X);  //起点切向量角度
                                    newEntId.Rotate(pt2, ang2 - ang1);  //旋转图元
                                }
                            }
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
                    if (double.TryParse(seg, out dist))//如果解析为double值成功
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
    }
}
