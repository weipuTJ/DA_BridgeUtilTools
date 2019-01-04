using System;
using System.ComponentModel;

namespace DA_TendonToolsWpf
{
    public static class TendonGeneralParameters
    {
        /// <summary>
        /// 管道偏差系数（1/m）
        /// </summary>
        private static double kii = 0.0015;
        public static double Kii
        {
            get { return kii; }
            set
            {
                kii = value;
            }
        }
        /// <summary>
        /// 管道摩阻系数（1/rad）
        /// </summary>
        private static double miu = 0.16;
        public static double Miu
        {
            get { return miu; }
            set
            {
                miu = value;
            }
        }
        /// <summary>
        /// 钢束弹性模量（MPa）
        /// </summary>
        private static double ep = 1.95E5;
        public static double Ep
        {
            get { return ep; }
            set
            {
                ep = value;
            }
        }
        /// <summary>
        /// 张拉控制应力（MPa）
        /// </summary>
        private static double ctrlStress = 1395;
        public static double CtrlStress
        {
            get { return ctrlStress; }
            set
            {
                ctrlStress = value;
            }
        }
        /// <summary>
        /// 工作长度（mm）
        /// </summary>
        private static double workLen = 800;
        public static double WorkLen
        {
            get { return workLen; }
            set
            {
                workLen = value;
            }
        }
    }
}
