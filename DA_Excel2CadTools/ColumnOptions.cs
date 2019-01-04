using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Excel2CadTools
{
    public class ColumnOptions
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string ColName { get; set; }
        /// <summary>
        /// 列宽
        /// </summary>
        public double ColWidth { get; set; }
        /// <summary>
        /// 水平对齐方式
        /// 0-靠左，1-居中，2-靠右
        /// </summary>
        public int HrAlignment { get; set; }
        /// <summary>
        /// 竖直对齐方式
        /// 0-靠上，1-居中，2-靠下
        /// </summary>
        public int VtAlignment { get; set; }
        public ColumnOptions(string colName, double colWidth, int hrAlignment=1, int vtAlignment=1)
        {
            ColName = colName; ColWidth = colWidth; HrAlignment = hrAlignment; VtAlignment = vtAlignment;
        }
        public ColumnOptions()
        {
            ColName = "A1"; ColWidth = 80; HrAlignment = 1; VtAlignment = 1;
        }
    }
}
