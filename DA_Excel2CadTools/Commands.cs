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

namespace DA_Excel2CadTools
{
    /// <summary>
    /// 命令类
    /// </summary>
    public class Commands
    {
        [CommandMethod("DA_FromExcel2CAD")]
        public void FromExcel2CAD()
        {
            Excel2CADSettings settingDlg = new Excel2CADSettings();
            Application.ShowModalWindow(settingDlg);
        }
    }
}
